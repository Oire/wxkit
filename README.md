# WxKit

.NET bindings to [wxWidgets](https://wxwidgets.org), for native desktop applications on Windows, macOS, and Linux. WxKit works with Native AOT, and links wxWidgets into the executable so that an application ships as one file.

WxKit is a preview: it works for trying out and small applications, its API will change before 1.0, and parts of wxWidgets are not bound yet. It runs on Windows, Linux, and macOS, each on x64 and ARM64. The bindings are generated from wxWidgets's own headers, and cover 117 classes: windows and the common controls, list, tree, and data view controls, toolbars, sizers, menus, the standard dialogs, drawing, images, validators, and the clipboard, with events, async support, and accessibility for controls that draw themselves. The [text editor sample](samples/Editor/EditorFrame.cs) shows what an application looks like, with menus, files, find and replace, and fonts. The [Hello sample](samples/Hello/Program.cs) publishes with Native AOT as a single executable of about 5 MB, and an application only links the classes it uses. To see what it will look like and how it will work, start with the [design overview](docs/design/overview.md). Decisions made so far, and the ones still open, are in [Open questions](docs/design/open-questions.md).

## Building

On Windows, you need the following:

- The .NET 10 SDK.
- Visual Studio 2022 or newer, or its Build Tools, with the C++ tools. Building for ARM64 also needs their ARM64 build tools, which the Visual Studio Installer lists as "MSVC ARM64/ARM64EC build tools".
- CMake.
- PowerShell 7.

wxWidgets is a git submodule, built with CMake. The build script builds it, and WxKit's native shim, for the runtimes you give it, and defaults to `win-x64`. Give it `-Runtimes win-x64, win-arm64` for both. An x64 machine can build for ARM64, but only an ARM64 machine can run what it builds. The first build of each runtime takes 10 to 20 minutes.

```powershell
git clone --recurse-submodules https://github.com/trypsynth/wxkit
cd wxkit
./scripts/build-native.ps1
dotnet test --project tests/WxKit.Tests
./scripts/test-aot.ps1
```

On Linux, you need CMake, Ninja, PowerShell 7, the .NET 10 SDK, GTK 3's development files, such as Ubuntu's `libgtk-3-dev`, and, for Native AOT, `clang` and zlib's development files, such as `zlib1g-dev`. The same commands build and test, and the build script defaults to the machine's own runtime, such as `linux-x64`.

On macOS, you need Xcode, the .NET 10 SDK, PowerShell 7, and CMake and Ninja, such as from Homebrew. A Mac with Apple Silicon builds both `osx-arm64` and `osx-x64`, and runs the latter through Rosetta.

The tests create real windows, so they need a desktop session. On Linux without one, run them under `xvfb-run`. `test-aot.ps1` publishes the samples with Native AOT and checks that each starts and exits cleanly.

## Packages

WxKit is not on nuget.org yet. A tag will publish it there, as [Building and packaging](docs/design/building-and-packaging.md#releasing) describes. Until then, to use it from an application of your own, build its packages, which go to `artifacts/packages`:

```powershell
./scripts/build-native.ps1
./scripts/pack.ps1
./scripts/test-package.ps1
```

Give `pack.ps1` `-Runtimes win-x64, win-arm64` to pack both, as CI does. `test-package.ps1` checks the packages the way an application would use them, on the machine's own runtime. Each push to master also builds them in CI, which keeps them for a week as an artifact of the run.

Add `artifacts/packages` as a package source, and reference `WxKit`, which brings in the native package for each runtime, such as `WxKit.Native.win-x64`:

```xml
<PackageReference Include="WxKit" Version="0.1.0-preview.1" />
```

An application runs with the shared native library, and a Native AOT publish links wxWidgets into the executable instead. A publish for macOS also makes an app bundle, which [Building and packaging](docs/design/building-and-packaging.md#macos-app-bundles) describes:

```powershell
dotnet publish -r win-x64 -p:PublishAot=true
```

If you publish with Native AOT yourself and it fails with "'vswhere.exe' is not recognized", add `%ProgramFiles(x86)%\Microsoft Visual Studio\Installer` to `PATH`. The Visual Studio 2026 Build Tools need it there for Native AOT to find the linker. `test-aot.ps1` does this for you.

## How this was made

WxKit is heavily vibe coded. Claude, an AI model, wrote nearly all of the code and the docs. I directed the work, reviewed parts of it, and checked that it mostly holds up, but I have not read every line, and you should expect mistakes a careful human author would not make.

If AI-written code is a dealbreaker for you, this is not the project for you, and that's fine. If it isn't, bug reports and pull requests are welcome.

## License

WxKit is MIT licensed. wxWidgets is under the wxWindows Library Licence, which allows applications that link it, statically or not, to use their own terms. The native packages also contain the libraries wxWidgets builds in, such as zlib and libpng, which [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) lists with their licenses.
