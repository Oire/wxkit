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
   - The shim's declarations in `native/shim/include/wxkit_generated.h`, which `wxkit.h` includes, and their C++ in `native/shim/generated/generated.cpp`.
   - `LibraryImport` declarations in `src/WxKit/Interop/WxKitNative.g.cs`.
   - Managed partial classes, the event types, the style enums, and the table `ObjectRegistry` uses to wrap native objects, in `src/WxKit/Generated`.

It also writes a report of everything it skipped, with the reason, to `artifacts/generator-report.txt`.

## Rules the generator applies

These are the decisions it makes without a tweak.

### Classes

- A class's handle depends on its ancestry. Windows cross as `wxk_Window`, sizers as `wxk_Sizer`, menus as `wxk_Menu`, events as `wxk_Event`, and other event handlers as `wxk_EvtHandler`. Value objects, which are `wxGDIObject`s and `wxImage`, and drawing contexts, which are `wxDC`s, cross as `wxk_Object`. A class a tweak marks static-only, such as `wxSystemSettings`, becomes a C# static class.
- A class's managed base is its nearest ancestor that is also bound and has the same lifetime: WxKit owns value objects and drawing contexts, and wxWidgets owns the others. Those WxKit owns that have no such ancestor derive from the hand-written `NativeObject`. A tweak can mark a class as one WxKit owns, as validators are, though they are event handlers.
- The members of a secondary base, such as `wxTextEntry` under `wxTextCtrl`, are bound on the class itself, with the secondary base's own bases, as `wxItemContainer` brings `wxItemContainerImmutable`. The shim calls them through a cast to that base, which picks the right one when two bases both have a member, and still calls overrides.
- A class that declares a pure virtual member gets no constructors.
- A virtual member a tweak lists, which is often protected, becomes a `protected virtual` method C# can override, and the class and the classes derived from it get directors, which the shim creates instead of them. A director's parameters can only be numbers, enums, and booleans for now.

### Members

- A window's constructor with no parameters is left out, since two-step creation is not exposed, and so is the `Create` that completes it. So are copy constructors, destructors, operators, and deprecated members.
- A member the interface headers repeat in a derived class, as they do with every override, is bound once, in the base. A static member is the exception: the derived one stays, with the `new` modifier, since it does something else there.
- Two members with the same managed signature are bound once, such as `CentreOnParent` and `CenterOnParent`, and so are two constructors.
- A trailing parameter the generator cannot convert, or whose default it cannot write, is left out with the parameters after it, so the C++ default applies. An array cut off this way takes its count with it. A parameter without a default that it cannot convert makes the whole function unbound.
- A static member checks the thread before it calls the shim, since it has no handle that would.
- A property and a method with the same name fail the generator, until a tweak renames one, since which to rename is a choice.

### Constructors

- A window id without a default gets `StandardId.Any`, and moves after the label or title, before the position.
- A window constructor's position and size can always be left out, since the overloads that require them mean the same by the default.
- A window's name is always left to its default.
- A top-level window's parent can be null.
- Required parameters move ahead of optional ones, keeping their order, so `new Choice(parent, items)` works.

### Types and defaults

- A tweak can give a number parameter, or a return value, an enum, as `wxTextValidator`'s style takes `TextValidatorStyle`.
- A `style` parameter of a class with a style table takes that class's style enum, such as `ButtonStyle`, and so does a getter named for the style, such as `GetMessageDialogStyle`. A default made of constants the enum has is written with its members, such as `MessageDialogStyle.Ok | MessageDialogStyle.Center`.
- A number whose default is `wxBOTH`, `wxHORIZONTAL`, or `wxVERTICAL` is an `Orientation`.
- A C++ enum becomes a C# enum, with the prefix all its members share dropped. A tweak can bind one no bound member uses, under a name of its own, as `wxAccRole` is `AccessibleRole`, and constants wxWidgets defines one by one with a shared prefix, as `wxACC_STATE_SYSTEM_` are `AccessibleStates`. An enum crosses as a 32-bit int, so a value such as `0xFFFFFFFF` is -1. `wxOrientation`, `wxDirection`, and `wxItemKind` map to WxKit's hand-written enums instead.
- A pointer default of `nullptr`, `NULL`, or `0` is null, and so is `wxDefaultPosition`, `wxDefaultSize`, `wxNullColour`, and the empty object of a value class, such as `wxNullFont`.
- A few constants the interface headers use are only defined in the real headers, from the same pinned wxWidgets: the deprecated GUI constants in `wx/defs.h`, such as `wxDEFAULT`, and the art provider's ids in `wx/artprov.h`. The art ids are written as the `ArtId` and `ArtClient` constants.
- An enum value takes its value from the real headers, which are what the shim compiles against. The interface headers leave some initializers out, as for `wxTextValidatorStyle`'s flags and `wxPenStyle`, which would then count up from zero, and Doxygen cuts others short at a continued line, as for `wxEVT_CATEGORY_ALL`.
- A constant whose value depends on the platform is left out, since a C# constant has one value. The real headers show which ones those are: an enum value inside an `#if`, a value that follows one without an initializer, as `wxCURSOR_ARROWWAIT` follows wxGTK's `wxCURSOR_DEFAULT`, and a `#define` written more than one way, as `wxTR_DEFAULT_STYLE` is.
- An enum parameter whose default is such a constant is nullable instead, and null passes the C++ default, as in `new TreeCtrl(parent, style: null)`.
- The generated shim checks every enum value with a `static_assert`, so a value that is wrong for the platform being built fails the native build rather than passing wxWidgets the wrong number. Styles are compared as the `long` the shim passes them as.

### Names

- `GetX` with no parameters becomes the property `X`, and `IsX`, `HasX`, and `CanX` returning `bool` stay as they are. A `SetX` with one parameter of the same type, and the same staticness, becomes its setter.
- British spellings become American ones, a leading digit is spelled out, as in `ThreeStateValue`, and a few terse parameter names become words, such as `pos` to `position` and `n` to `index`.
- A style enum's members drop the `wx` prefix, and the short prefix at least two of the class's styles share, such as `TE_` for text controls, unless that prefix is a constant itself, as `YES` is in `wxYES_NO`. `wxDEFAULT_FRAME_STYLE` becomes `Default`. Every style enum has `None`. Extra styles, whose names have `_EX_`, are not bound yet.
- Events have .NET names: a table names the common ones, such as `SizeChanged`, `GotFocus`, `SelectionChanged`, and `ValueChanged`, and others are named after their macro, as `EVT_TEXT_ENTER` becomes `TextEnter`. An event that would clash with a member or its class gets an `Event` suffix.

### Events

- Only event types the headers declare are bound, which leaves out group entries such as `EVT_MOUSE_EVENTS` and `EVT_JOY_*`. An entry without its `wxEVT_` constant uses the convention that `EVT_X` sends `wxEVT_X`, if the headers declare it. The shim includes each event type's header.
- An event class WxKit does not bind is delivered as its nearest bound ancestor.
- An event class's own table gives event types, but no .NET events, and a derived class does not repeat its base's events.

## Current coverage

107 classes bind 1833 functions and 167 event types. They are windows and top-level windows, the common controls, list and tree controls, toolbars, sizers, menus, the standard dialogs, the event classes the core and those controls send, fonts, pens, brushes, bitmaps, icons, cursors, images, drawing contexts, system settings, the art provider, validators, and the enums of accessibility, whose `Accessible` is written by hand. The virtual list's members are overridable. The tweaks are in `src/WxKit.Generator/Tweaks/Core.cs`. The clipboard is written by hand, in `Clipboard` and `ClipboardContent`, since `wxClipboard` hands data objects over in ways the generator cannot express, and so is `CustomValidator`, whose checks wxWidgets calls as virtual functions. The report lists about 400 members the generator still cannot bind, most often because of C arrays, per-item client data, raw image buffers, and classes not bound yet, such as the objects a toolbar's tools are.

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
