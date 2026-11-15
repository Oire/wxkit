# Building and packaging

This document describes how WxKit builds wxWidgets and its shim, how Native AOT links them into an application, and how they ship on NuGet.

## Native builds

`scripts/build-native.ps1` builds wxWidgets and the shim for each runtime it is given, and defaults to `win-x64`:

```powershell
./scripts/build-native.ps1 -Runtimes win-x64, win-arm64
```

For each runtime it makes two builds:

- A static build, for Native AOT. wxWidgets and the shim are built as static libraries, and the script writes a `WxKit.Native.targets` file beside them that lists everything the final link needs. This keeps platform details out of the package's own targets file.
- A shared build, for everything else. wxWidgets is linked statically into one shared library, `wxkitnative.dll`, `libwxkitnative.dylib`, or `libwxkitnative.so`, which exports only the shim's functions. An application that does not use Native AOT loads this library from the package's `runtimes` folder.

Output goes to `artifacts/native/<runtime>/static` and `artifacts/native/<runtime>/shared`.

### Windows

- wxWidgets and the shim are built with the static C runtime, `/MT`, because that is what Native AOT links. The script finds the newest Visual Studio with the C++ tools and names its CMake generator explicitly.
- `win-arm64` builds on an x64 machine as well as an ARM64 one, with Visual Studio's ARM64 build tools, and Native AOT publishes for it from either. Only an ARM64 machine can run what it makes, so CI tests it on one.
- The static link lists the Windows libraries that wxMSW uses, such as `comctl32`, `uxtheme`, `oleacc`, `gdiplus`, `shlwapi`, and `version`.
- wxMSW needs version 6 of the common controls for themed controls, and wants per-monitor DPI awareness. Both come from the application manifest. The package's targets set `ApplicationManifest` to WxKit's own manifest, which declares both, unless the application sets its own. The compiler embeds it in the managed assembly, as it would any manifest, and Native AOT copies it into the executable from there.
- wxMSW's resources, such as its cursors, its default icons, and the menu that MDI frames use, live in `wx.rc`. With a static link, the application has to contain them. The script compiles them, without a manifest, to `wxkit.res`. The linker only accepts one set of Win32 resources, though, and Native AOT already gives it the application's own, which hold its icon, version, and manifest. So the package's targets turn off Native AOT's copy with `IlcGenerateWin32Resources`, and a small build task reads the application's resources out of its managed assembly and writes them, with WxKit's, into one `.res` file for the linker. The application's resources win wherever both have one with the same type, name, and language.
- Native AOT finds the linker through `vcvarsall.bat`. In the Visual Studio 2026 Build Tools, that script runs `vswhere.exe` by name, so if the Visual Studio Installer folder is not on `PATH`, its error message ends up inside the linker's path and the link fails. Until that is fixed upstream, add `%ProgramFiles(x86)%\Microsoft Visual Studio\Installer` to `PATH`.

### macOS

- wxOSX is built for Cocoa with the same deployment target as the .NET version WxKit targets.
- The link targets list the frameworks wxOSX uses, such as `AppKit`, `Carbon`, `IOKit`, `QuartzCore`, `WebKit`, and `AudioToolbox`.
- A GUI application on macOS behaves best inside an app bundle. Without one, the menu bar and focus can misbehave. Creating a bundle from a published executable is separate tooling, which is a later milestone.

### Linux

- wxGTK is built against GTK 3. wxWidgets is linked statically, but GTK and its dependencies must come from the system, because they are part of the desktop.
- The system libraries are read from `pkg-config` on the build machine, and named by their sonames, such as `-l:libgtk-3.so.0`, so that an application's build machine needs the libraries but not their development packages.
- The shared builds are made on the oldest supported Ubuntu, so they work with its glibc and anything newer.
- Native AOT links with clang, so a Linux publish needs `clang` and the GTK 3 libraries.

## Native AOT and static linking

The package's `buildTransitive/WxKit.targets` works like this:

- With `PublishAot` on, `WxKitStaticLink` defaults to `true`. The targets add `DirectPInvoke` for `wxkitnative` and import the runtime's `WxKit.Native.targets`, so the shim's functions are linked into the executable and called directly, without loading anything at run time.
- With `WxKitStaticLink` set to `false`, the shared library ships beside the executable instead.
- Without Native AOT, the shared library loads from the package's `runtimes` folder.
- The targets fail with a clear message when there is no static build for the runtime being published.

The managed library is marked `IsAotCompatible`, so the trimming and AOT analyzers run on every build. The library uses no reflection, no runtime code generation, and no marshalled delegates. The `SYSLIB1054` diagnostic is an error, so every native call goes through `LibraryImport`.

CI publishes the C# sample with Native AOT on each platform and runs it in a smoke test mode, which shows a frame, waits for the event loop to start, and exits. This catches link errors that ordinary builds cannot.

## Packages

wxWidgets is large. On `win-x64`, the static libraries for the base and core libraries and the shim come to about 175 megabytes, and nuget.org limits a package to 250 megabytes. They compress to about 37 megabytes. An application that links them is much smaller: the Hello sample publishes as a single executable of about 5.2 megabytes.

Every runtime's static libraries in one package would not fit, so there is a package per runtime:

- `WxKit`, the managed library and its targets, with no native files.
- A native package per runtime, such as `WxKit.Native.win-x64`, which holds that runtime's shared library in `runtimes/<runtime>/native`, where .NET finds it, and its static libraries, link targets, and resources in `native/<runtime>`. It has no managed code.
- `WxKit` depends on every native package, so an application only references `WxKit`.

Each native package's targets, in `buildTransitive`, set `WxKitNativeDirectory` to its static build when the application's runtime is the package's, and both they and WxKit's targets import that build's `WxKit.Native.targets` when the application links statically. NuGet does not say which package's targets it imports first, so whichever comes second finds the link targets imported already, through a property they set. A project that builds WxKit from source, such as the Hello sample, sets `WxKitNativeDirectory` itself.

`src/WxKit.Native/WxKit.Native.csproj` packs a native package from a runtime's build, given its runtime as `WxKitRuntime`, and fails when there is no build for it. `scripts/pack.ps1` packs one for each runtime given, then `WxKit`, which depends on those runtimes' packages only while it is packed, so building WxKit itself needs none. The native packages carry `THIRD-PARTY-NOTICES.md` as their license file, and each license text in their `licenses` folder.

`scripts/test-package.ps1` uses the packages as an application would. It makes a project that empty `Directory.Build` files shut off from the repository's build, which restores `WxKit` from `artifacts/packages` into a folder of its own, and holds the Hello sample's code. It runs the project as it is, with the shared library, and then publishes it with Native AOT, checks that nothing native ships beside the executable, and runs that.

## Releasing

A tag that names a version, such as `v0.1.0-preview.1`, releases it. CI runs on the tag as it does on a push, packs every package with the tag's version, tests them, and then a publish job pushes them to nuget.org and makes a GitHub release with notes generated from the commits and the packages attached. A version with a suffix, such as `-preview.1`, is a prerelease in both places. `Directory.Build.props` only gives the version of builds that are not releases.

```powershell
git tag v0.1.0-preview.1
git push origin v0.1.0-preview.1
```

The publish job signs in to nuget.org with Trusted Publishing: nuget.org trusts the workflow's GitHub identity, and the `NuGet/login` action trades it for a key that lasts an hour. No key is kept in the repository. The native packages go before `WxKit`, which depends on them, and a version nuget.org has already is skipped, so a run that failed partway can run again.

Setting this up takes three steps, once:

1. On nuget.org, under Trusted Publishing in the account's settings, add a policy for the repository owner `trypsynth`, the repository `wxkit`, the workflow file `ci.yml`, and the environment `nuget`.
2. In the repository's settings on GitHub, under Environments, add one called `nuget`. Adding yourself as a required reviewer there makes every release wait for your approval before it publishes, and limiting it to tags that start with `v` keeps any other ref from using it.
3. Under Actions variables, add `NUGET_USER`, set to the nuget.org user name the policy belongs to.

nuget.org lets a version be unlisted, so a search no longer finds it, but not deleted, so a version number is only ever used once.

The packages carry Source Link, and the managed library's symbols go to nuget.org's symbol server, so a debugger can step into WxKit's sources at the release's commit. A CI build is deterministic, which Source Link needs.

## Target framework

The managed library targets `net10.0`. .NET 8 reaches the end of its support in November 2026, so supporting it would mean shipping for a version that is already out of support. The tests and samples target `net10.0` as well.

## Licensing

WxKit is MIT licensed. wxWidgets uses the wxWindows Library Licence, which is the LGPL with an exception that allows distributing applications linked with it, statically or not, under their own terms. That exception is what makes the single-executable model possible.

The native packages also contain the third-party libraries that wxWidgets builds into its base and core libraries: zlib, libpng, libjpeg, libtiff, PCRE2, libwebp, and NanoSVG. `THIRD-PARTY-NOTICES.md` lists each one with its version and license, and the packages carry their license texts. Optional libraries bring more, such as expat for XRC, and Scintilla and Lexilla for the styled text control, which the notices will list when WxKit binds them.

## Continuous integration

CI runs on GitHub Actions:

- A job per runtime, on a machine of that runtime's architecture, builds wxWidgets and the shim, caches the build by the runtime, the wxWidgets commit, and a hash of the build script, runs the tests, publishes the Native AOT smoke test, and uploads the native build. `win-x64` runs on `windows-2022`, and `win-arm64` on `windows-11-arm`. The x64 job also tests the generator, checks that the generated bindings are current, and checks formatting.
- A pack job gathers every runtime's native build and packs the packages, which CI keeps for a week, as an artifact of the run.
- A job per runtime, on that runtime's architecture again, tests the packages with `test-package.ps1`.
- On a tag, a publish job pushes the packages to nuget.org, as Releasing describes.
- A newer run for the same pull request cancels an older one.

wxWidgets takes a long time to build, so the cache matters. A cold build of one runtime takes about 10 to 20 minutes on a desktop machine, and CI runners are slower.
