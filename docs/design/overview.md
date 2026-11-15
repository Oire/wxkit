# WxKit design overview

WxKit is a set of .NET bindings to [wxWidgets](https://wxwidgets.org), the C++ library for native desktop user interfaces. This document describes what WxKit is for, how its parts fit together, and the order in which they get built. The other documents in this folder cover each part in detail.

Status: draft. The first two milestones are done on Windows, Linux, and macOS, and the third, breadth, is under way: the generator binds 117 classes. The first preview, 0.1.0-preview.1, is on nuget.org for Windows. Decisions made so far, and the ones still open, are in [Open questions](open-questions.md).

## Goals

- Give C# developers wxWidgets through an API that reads like modern .NET, not like C++ with a C# accent. Properties instead of getter and setter pairs, standard .NET events, enums instead of integer flags, and nullable annotations throughout.
- Work with Native AOT. A published application is one executable with wxWidgets linked into it, and nothing ships beside it.
- Cover wxWidgets broadly. The aim is the whole GUI surface that a desktop application uses, not a hand-picked subset. A generator does the mechanical work so that coverage can grow without hand-writing thousands of functions.
- Keep native controls native. wxWidgets uses the platform's own controls, so screen readers and other assistive technology work with them out of the box. WxKit must not get in the way of that, and its samples and defaults should model accessible practice.
- Be usable from F#, and from Visual Basic where it costs little. C# comes first. See [Language support](languages.md).

## Non-goals

- Binding the non-GUI parts of wxWidgets that .NET already does better. For example, `wxString`, `wxDateTime`, `wxFileName`, `wxRegEx`, `wxThread`, `wxSocket`, and the wx containers. WxKit maps these to .NET types at the boundary instead.
- Mobile platforms. wxWidgets has no supported iOS or Android port.
- A visual designer. XRC files, which other tools can produce, are in scope later.
- Source compatibility with wxPython, wxRuby, or wxDragon. WxKit borrows ideas from them, not their API shapes.

## Supported platforms

WxKit targets the three wxWidgets ports that matter on the desktop:

- Windows, through wxMSW, on `win-x64` and `win-arm64`. `win-x86` is left out for now: Windows 11 has no 32-bit edition, and 32-bit Windows 10 is out of support. The build script can already build it, so it would take a package and a CI job if someone needs it.
- macOS, through wxOSX Cocoa, on `osx-arm64` and `osx-x64`.
- Linux, through wxGTK 3, on `linux-x64` and `linux-arm64`.

Windows came first, then Linux, on `linux-x64` and `linux-arm64`, and macOS, on `osx-arm64` and `osx-x64`.

## Architecture

WxKit has four layers. Each one only talks to the layer directly below it.

1. wxWidgets itself. It is a git submodule under `native/wxWidgets`, pinned to a release tag, and built with CMake as static libraries.
2. The native shim, under `native/shim`. This is C++ code that exposes wxWidgets through a flat C interface, because .NET cannot call C++ directly. Every function has a `wxk_` prefix, takes and returns only C types, and never lets a C++ exception escape. Most of the shim is generated. The rest, such as the application object and event dispatch, is written by hand. The shim and wxWidgets build together into one library, called `wxkitnative`.
3. The raw interop layer, in the `WxKit.Interop` namespace. It declares every shim function with `LibraryImport`, one to one. It is generated.
4. The managed API, in the `WxKit` namespace. These are the classes that applications use, such as `Frame`, `Button`, and `BoxSizer`. Each class is a generated partial class, with a hand-written partial beside it wherever the generated part needs help.

A call from application code goes down through these layers like this. `button.Label = "Save"` calls a property setter in the managed API. The setter pins the string and calls `WxKitNative.ButtonSetLabel` in the interop layer. That function calls `wxk_Button_SetLabel` in the shim, which converts the text to a `wxString` and calls `wxButton::SetLabel`.

An event goes the other way. wxWidgets calls a functor that the shim bound with `Bind`. The functor calls a function pointer into an `UnmanagedCallersOnly` method in the managed API, which raises the .NET event.

## Documents in this folder

- [Native layer](native-layer.md) covers the shim: its C conventions, strings, errors, callbacks, and how C++ virtual methods are overridden from .NET.
- [Managed API](managed-api.md) covers naming, object lifetime, events, threading, and the application model.
- [Generator](generator.md) covers how bindings are produced from the wxWidgets interface headers.
- [Building and packaging](building-and-packaging.md) covers native builds, static linking for Native AOT, NuGet packages, and CI.
- [Language support](languages.md) covers F# and Visual Basic.
- [Testing](testing.md) covers how GUI code gets tested on three platforms.
- [Open questions](open-questions.md) lists the decisions made so far and the ones still open.

## Conventions

WxKit follows these conventions:

- Every text file uses LF line endings, which `.editorconfig` and `.gitattributes` enforce. C# is indented with tabs, Markdown and YAML with two spaces, and F# with four spaces. Code is formatted with `dotnet format`, and style warnings show in every build.
- `Directory.Build.props` sets the language version, nullable reference types, implicit usings, unsafe blocks, and code style enforcement for every project.
- The solution is a `.slnx` file.
- Build output goes under `artifacts/`, which git ignores.
- Native code is built by a PowerShell script under `scripts/`, which CI runs on each platform.
- Commit messages start with a lowercase verb for fixes, such as "fix a crash when a frame closes twice", and a capitalized verb for new work, such as "Add the tree control".
- Comments explain why the code does something, not what it does.

## Repository layout

The planned layout is:

- `native/wxWidgets`: the wxWidgets submodule.
- `native/shim`: the C++ shim and its CMake project. Generated sources go in `native/shim/generated`.
- `src/WxKit`: the managed library, with `build/WxKit.targets` for consumers.
- `src/WxKit.Generator`: the binding generator, a console application that is not packed.
- `tests/WxKit.Tests`: tests for the managed API.
- `tests/WxKit.Generator.Tests`: tests for the generator.
- `samples/Hello`: a C# sample that publishes with Native AOT.
- `samples/HelloFSharp` and `samples/HelloVisualBasic`: the same sample in the other languages.
- `scripts/build-native.ps1`: builds wxWidgets and the shim for the given runtimes.
- `scripts/generate.ps1`: runs Doxygen and the generator, and rewrites the generated files.
- `docs/design`: these documents.

## Milestones

The plan builds a small slice of wxWidgets by hand first, to settle the hard questions, and only then automates.

1. Hand-written vertical slice, on one platform. An application object, `Frame`, `Panel`, `Button`, `StaticText`, `TextCtrl`, `BoxSizer`, a menu bar, `MessageDialog`, button and menu events, `CallAfter`, and a timer. The sample publishes with Native AOT as a single executable and runs. This milestone settles strings, lifetime, events, threading, and static linking.
2. Generator, reproducing the slice. The generator emits the same shim, interop, and managed code that the slice has by hand. The hand-written versions are then deleted.
3. Breadth. The generator covers the core and advanced controls, sizers, dialogs, menus, toolbars, drawing, images, the clipboard, and drag and drop.
4. All three platforms in CI, with packages for every runtime.
5. Overridable virtual methods for the classes that need them, such as virtual list controls and data view models.
6. Optional libraries: AUI, the styled text control, the property grid, the web view, and rich text. XRC comes last.
7. F# and Visual Basic samples in CI, and an F# helper package if one turns out to be worth it.
