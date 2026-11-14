# WxKit

.NET bindings to [wxWidgets](https://wxwidgets.org), for native desktop applications on Windows, macOS, and Linux. WxKit works with Native AOT, and links wxWidgets into the executable so that an application ships as one file.

WxKit is in early development, and is not usable for applications yet. So far, on Windows, it can open a frame, and the sample publishes with Native AOT as a single executable. To see what it will look like and how it will work, start with the [design overview](docs/design/overview.md). Decisions made so far, and the ones still open, are in [Open questions](docs/design/open-questions.md).

## Building

You need the following:

- The .NET 10 SDK.
- Visual Studio 2022 or newer, or its Build Tools, with the C++ tools.
- CMake.
- PowerShell 7.

wxWidgets is a git submodule, built with CMake. The build script builds it, and WxKit's native shim, for the runtimes you give it, and defaults to `win-x64`. The first build takes 10 to 20 minutes.

```powershell
git clone --recurse-submodules https://github.com/trypsynth/wxkit
cd wxkit
./scripts/build-native.ps1
dotnet publish samples/Hello
```

If the publish fails with "'vswhere.exe' is not recognized", add `%ProgramFiles(x86)%\Microsoft Visual Studio\Installer` to `PATH`. The Visual Studio 2026 Build Tools need it there for Native AOT to find the linker.

## How this was made

WxKit is heavily vibe coded. Claude, an AI model, wrote nearly all of the code and the docs. I directed the work, reviewed parts of it, and checked that it mostly holds up, but I have not read every line, and you should expect mistakes a careful human author would not make.

If AI-written code is a dealbreaker for you, this is not the project for you, and that's fine. If it isn't, bug reports and pull requests are welcome.

## License

WxKit is MIT licensed. wxWidgets is under the wxWindows Library Licence.
