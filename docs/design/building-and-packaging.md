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

wxWidgets is large. On `win-x64`, the static libraries for just the base and core libraries come to about 150 megabytes, most of it in the core library, and nuget.org limits a package to 250 megabytes. An application that links them is much smaller: the Hello sample publishes as a single executable of about 4.6 megabytes.

One package per operating system will not fit, since one runtime's static libraries already take more than half the limit, before the optional libraries are added. The proposal is:

- `WxKit`, the managed library and its targets, with no native files.
- One native package per runtime, such as `WxKit.Native.win-x64`, holding that runtime's static and shared builds.
- `WxKit` depends on every native package, so an application only references `WxKit`.

Making the libraries smaller, for example by checking what debug information they carry, may change this.

## Target framework

The managed library targets `net10.0`. .NET 8 reaches the end of its support in November 2026, so supporting it would mean shipping for a version that is already out of support. The tests and samples target `net10.0` as well.

## Licensing

WxKit is MIT licensed. wxWidgets uses the wxWindows Library Licence, which is the LGPL with an exception that allows distributing applications linked with it, statically or not, under their own terms. That exception is what makes the single-executable model possible.

The packages also contain the third-party libraries that wxWidgets builds in, such as zlib, libpng, libjpeg, libtiff, expat, PCRE2, libwebp, NanoSVG, and LunaSVG, plus Scintilla and Lexilla if the styled text control is included. `THIRD-PARTY-NOTICES.md` lists each one with its license, and the packages carry their license texts.

## Continuous integration

CI runs on GitHub Actions:

- A native job per runtime builds wxWidgets and the shim, caches the result by the wxWidgets commit and a hash of the build script, and uploads it.
- A test job per runtime downloads the native build, runs the tests, and publishes the Native AOT smoke test.
- A pack job on tags builds the packages from every runtime's native build.
- A newer run for the same branch cancels an older one.

wxWidgets takes a long time to build, so the cache matters. A cold build of one runtime takes about 10 to 20 minutes on a desktop machine, and CI runners are slower.
