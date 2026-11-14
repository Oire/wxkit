# Generator

wxWidgets has hundreds of GUI classes and thousands of methods. Binding them by hand would take years and drift from wxWidgets with every release. WxKit generates most of its bindings instead. This document describes how.

## Input

wxWidgets keeps a second copy of its public API in the `interface/wx` folder. These headers hold every class, method, enum, and event with its documentation, written for Doxygen and without the platform-specific implementation details of the real headers. They are what the official reference documentation is built from.

The generator does not parse C++. Doxygen parses the interface headers and writes XML, and the generator reads that XML. wxPython and wxRuby3 generate their bindings the same way, which shows that the XML carries enough detail.

The XML records what the generator needs:

- Classes, their base classes, and which header defines them.
- Methods with their parameter types, default values, constness, and whether they are virtual or static.
- Enums and constants.
- The styles each class supports, from its style table.
- The events each class sends, from its event table, including the event class each one uses.
- Which methods exist only on some ports, from `@onlyfor`.
- The brief and detailed description of each item.

Doxygen is a tool for regenerating bindings, not for building WxKit. The generated files are checked in, so a normal build needs neither Doxygen nor the generator. `scripts/generate.ps1` runs both and rewrites the generated files, and CI checks that the checked-in files match what the script produces.

## Binding tweaks

Not everything can be inferred from the XML. Some methods should not be bound at all, some names read badly after conversion, some pointers can be null, and some virtual methods need directors. The generator takes these decisions from tweak files: C# code in the generator project, one file per area, such as `Tweaks/Controls.cs` and `Tweaks/Sizers.cs`.

A tweak file reads like this:

```csharp
Class("wxButton", button => {
	button.Event("EVT_BUTTON", "Click");
	button.Ignore("SetAuthNeeded");
});

Class("wxListCtrl", list => {
	list.Director("OnGetItemText", "OnGetItemAttr", "OnGetItemImage");
	list.Nullable("GetImageList", ParameterOrReturn.Return);
});
```

Tweaks are code rather than a data format such as YAML so that they get compile-time checking, refactoring support, and the ability to share helpers. The generator fails when a tweak names a class or method that does not exist, so tweaks cannot quietly rot when wxWidgets changes.

## Pipeline

The generator runs in four stages:

1. Read. It loads the Doxygen XML into a model of classes, methods, enums, and events.
2. Filter. It drops what WxKit does not bind: the non-GUI classes listed in the overview's non-goals, deprecated methods, and anything a tweak ignores. It also drops methods whose types it does not know how to convert, and lists them in a report, so the gaps are visible.
3. Shape. It applies naming rules and tweaks, groups getters and setters into properties, picks overload suffixes, and turns style tables and event tables into enums and events.
4. Emit. It writes three sets of files from the shaped model:
   - C++ shim sources and headers in `native/shim/generated`.
   - `LibraryImport` declarations in `src/WxKit/Interop/Generated`.
   - Managed partial classes in `src/WxKit/Generated`.

## Type conversions

Each C++ type that crosses the boundary has a converter, which knows three things: its C type in the shim, its C# type in the interop layer, and its C# type in the managed API, along with the code that converts between them on each side. For example, the `const wxString&` converter emits a `char16_t*` and length in C, a `char*` and `nuint` in interop, and `string` in the managed API with a `fixed` statement.

Adding support for a new type means adding one converter. Every method that uses the type then gets bound on the next run.

## Generated code style

Generated code follows the `.editorconfig` like the rest of the repository, so it passes `dotnet format` without changes, and it is readable in a diff. Each generated file starts with a comment that says it is generated and names the script that regenerates it.

Managed classes are partial, so a hand-written file can add members that the generator cannot produce, such as convenience overloads, without touching generated files. A hand-written member can also replace a generated one, by telling the generator to skip it in a tweak.

## Stable output

When wxWidgets adds a method, the diff should show one new method, not a reshuffled file. The generator sorts everything it emits by a stable key and never includes timestamps or absolute paths. Overload suffixes depend only on the overload's own parameter types, so adding an overload does not rename the existing ones.

## Writing the generator

The generator is a .NET console application in `src/WxKit.Generator`. It targets the same .NET version as the library and does not need to support Native AOT itself. Its tests in `tests/WxKit.Generator.Tests` run against small hand-written Doxygen XML samples, so they do not need wxWidgets or Doxygen installed.
