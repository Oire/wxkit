# Native layer

This document describes the C interface that WxKit's native shim exposes over wxWidgets. The shim is the only place where WxKit code is C++. Everything above it is C#.

## Why a shim

.NET can call C functions, but not C++ ones. C++ has name mangling that differs by compiler, classes with virtual tables, overloads, exceptions, and templates, none of which `LibraryImport` understands. The shim wraps each wxWidgets function that WxKit uses in a plain `extern "C"` function with a stable name and only C types in its signature.

wxDragon, the Rust binding, takes the same approach with its `wxd_` functions. WxKit uses its own shim rather than wxDragon's, because WxKit's is generated and shaped for .NET. wxDragon's lessons still apply, such as the order in which destroy handlers run.

## Naming

Every exported function is named `wxk_<Class>_<Method>`, keeping the wxWidgets class name without its `wx` prefix. For example:

- `wxk_Button_SetLabel` wraps `wxButton::SetLabel`.
- `wxk_Window_GetSize` wraps `wxWindow::GetSize`.
- `wxk_Frame_new` creates a `wxFrame`.

C has no overloads, so an overloaded method gets a suffix that describes its parameters. For example, `wxk_Window_SetSize_Rect` and `wxk_Window_SetSize_Int4`. The generator picks these suffixes, and a binding tweak can override one when the default reads badly.

Opaque handle types are declared as incomplete structs, such as `typedef struct wxk_Window wxk_Window;`. In C# they become empty structs used through pointers, such as `Window*`, so the compiler keeps one kind of handle from being passed where another is expected.

## Types at the boundary

Only these kinds of values cross the boundary:

- Integers of a fixed width, such as `int32_t`, `int64_t`, and `size_t`.
- `float` and `double`.
- Booleans as `uint8_t`, because the size of C's `bool` is not guaranteed to match what .NET expects.
- Pointers to opaque handles.
- Small value structs that both sides lay out the same way: `wxk_Point`, `wxk_Size`, `wxk_Rect`, and `wxk_Colour`. The shim converts these to and from `wxPoint`, `wxSize`, `wxRect`, and `wxColour`.
- Function pointers with the C calling convention, for callbacks.
- Text, as described in the next section.

The C# side marks the assembly with `DisableRuntimeMarshalling`, so any signature that would need the runtime to marshal it fails to compile. Every type that crosses the boundary has to be blittable. This keeps calls cheap and keeps Native AOT from needing marshalling stubs.

## Text

.NET strings are UTF-16. On Windows, `wxString` is UTF-16 too, so text can cross without conversion there.

Text going into the shim is a pointer to UTF-16 code units and a length: `const char16_t* text, size_t length`. The managed side pins the string with `fixed` and passes its length, so a call allocates nothing on the managed heap. Because the length is explicit, a string that contains a null character arrives intact rather than cut short.

Text coming out of the shim is harder, because the shim owns the `wxString`. The shim returns a `wxk_String*`, an opaque handle to a heap copy of the text. The managed side reads its UTF-16 data and length, makes a .NET string, and frees the handle with `wxk_String_free`. A helper on the managed side wraps this so that generated code calls one function.

On macOS and Linux, `wxString` stores `wchar_t`, which is UTF-32 there. The shim converts with `wxString::FromUTF16` and `wxString::utf16_str`, or their equivalents. This costs a conversion per call on those platforms, which is the same cost any binding pays.

Arrays of strings, such as `wxArrayString`, cross as a `wxk_StringArray*` handle with a count and an accessor, and become `string[]` or `IReadOnlyList<string>` on the managed side.

## Errors

wxWidgets rarely throws C++ exceptions, but it can, for example `std::bad_alloc`. An exception that unwinds into .NET code ends the process, so every shim function catches everything. A caught exception is stored in a thread-local slot with its message, and the function returns a default value. The managed side does not check this slot after every call, which would cost too much. Instead, functions that wxWidgets documents as throwing, and the generator's list of risky calls, check it and throw a `WxException`.

Most wxWidgets failures are not exceptions. They are `false` return values, such as from `wxImage::LoadFile`, or null pointers. The shim passes these through unchanged, and the managed API decides what they mean.

### Assertions

wxWidgets checks its preconditions with `wxASSERT` and `wxCHECK`. When one fails, it calls `wxApp::OnAssertFailure`. By default, a debug build of wxWidgets then shows a dialog.

The shim's application class overrides `OnAssertFailure` and forwards the failure to managed code with the file, line, function, condition, and message. WxKit handles it like this:

- Debug builds of the native library keep assertions on. A failure raises the `Application.AssertFailed` event, and by default writes the failure to the debug output, instead of showing the wxWidgets assertion dialog.
- Release builds compile assertions out, with `wxDEBUG_LEVEL` set to 0, so a shipped application never stops on one.

## Callbacks

The shim never stores a .NET delegate. A callback is a C function pointer and a `void* userdata`. On the managed side, the function pointer is a static method marked `UnmanagedCallersOnly`, and the userdata is a `GCHandle` that leads back to the managed object. This avoids marshalled delegates, which Native AOT supports poorly, and makes the cost of a callback a single indirect call.

Every `UnmanagedCallersOnly` method catches every exception, because one that escapes ends the process. What happens to that exception depends on the callback. See the events section of [Managed API](managed-api.md).

## Events

The shim binds events with one generic function:

```c
uint8_t wxk_EvtHandler_Bind(wxk_EvtHandler* handler, int32_t eventType, int32_t id, int32_t lastId, wxk_EventCallback callback, void* userdata, wxk_FreeCallback free);
```

It binds a small C++ functor that holds the function pointer and userdata. When the event fires, the functor calls the function pointer with the userdata and a pointer to the `wxEvent`. When wxWidgets destroys the functor, which happens when the handler is unbound or the window is destroyed, its destructor calls `free` with the userdata. That is how the managed side learns it can release the `GCHandle`. Tying the release to the functor's destructor means it always happens exactly once, after the last possible call, with no ordering problems between handlers.

Event types are runtime values in wxWidgets, such as `wxEVT_BUTTON`. The shim exports a getter for each event type, and the managed side reads them once.

## Object tracking

The managed side needs to know when a native object it wraps is destroyed, so that later calls throw `ObjectDisposedException` instead of crashing.

Every window and event handler in wxWidgets derives from `wxTrackable`, which is what `wxWeakRef` uses. The shim attaches a `wxTrackerNode` to each object the managed side wraps. When wxWidgets destroys the object, the node's `OnObjectDestroy` runs and calls back into managed code. This works for windows WxKit created and for ones it did not, such as the buttons inside a standard dialog.

Objects that are not trackable, such as sizers, are tracked through the window that owns them. The details are in [Managed API](managed-api.md).

## Overriding virtual methods

Some wxWidgets classes are only useful if you override a virtual method. For example, a virtual list control calls `OnGetItemText` for each visible row, and a `wxDataViewModel` is entirely virtual methods.

For these classes, the generator emits a director: a C++ class that derives from the wxWidgets class and overrides each virtual member a tweak lists. WxKit creates the director instead of the class, so the constructor exports only change which class they create, and inheriting constructors give the director all of the class's. For example, `ListCtrlDirector` derives from `wxListCtrl` and overrides `OnGetItemText`, and a class derived from it, such as `wxListView`, gets a director of its own with the same overrides.

Each override calls a function pointer into .NET, which the managed class sets once, before it creates the first object, through `wxk_ListCtrl_SetDirectors`. The callback takes the object's handle, and finds the managed object through the registry, so the director keeps no handle of its own. Until the pointers are set, an override calls the class's own implementation.

On the managed side, the class has a matching `protected virtual` method. Its default calls the class's own implementation through an exported `wxk_ListCtrl_base_OnGetItemText` function, which reaches the member, often protected, through a struct derived from the class. The callback also calls that implementation when there is no managed object yet, or no longer, as while one is created or destroyed. The director always calls into managed code, whether or not the method is overridden. This costs one transition per call, but it avoids reflection to detect overrides, which does not fit Native AOT well.

A director's parameters can only be numbers, enums, and booleans for now, and its result can also be a string.

Only the virtual methods a binding tweak lists get directors. Most wxWidgets virtual methods are implementation details, and overriding them from .NET is a way to break things.

Two directors are written by hand. `ManagedAccessible`, in `native/shim/src/accessible.cpp`, is the accessible object `Accessible` gives a window. Its callbacks return a `wxAccStatus`, and fill in their result when they answer, and it answers "not implemented" for what .NET leaves to Windows. The window owns it, and its destructor frees the `GCHandle` that reaches the managed object. The other is `ManagedValidator`, in `native/shim/src/validator.cpp`, which `CustomValidator` creates. A validator is copied, since a window clones the one it is given, so the copies share their table and userdata, a weak `GCHandle`, through a `shared_ptr`. Each clone a window holds asks .NET to make the handle strong while it lives, so the managed validator lives as long as a window uses it, and the last copy to go frees the handle. Its methods call into .NET without a base implementation to fall back on, since `wxValidator`'s own checks only return true.

## Platform-specific functions

Some wxWidgets functions exist on only one port. For example, `wxWindow::MSWGetBgBrush`. The shim exports every function on every platform. On a platform that lacks the function, the export is a stub that stores an error and returns a default value. This matters for static linking: if a symbol were missing on one platform, every Native AOT application that references it would fail to link there, even if it never calls it.

The managed side marks these members with `SupportedOSPlatform`, so the platform compatibility analyzer warns about calls that are not guarded with `OperatingSystem.IsWindows()` or similar.

## wxWidgets build options

WxKit builds wxWidgets with a fixed set of options, so the generator knows which features exist. These include:

- Unicode, which is the only mode in wxWidgets 3.3.
- `wxUSE_ACCESSIBILITY` on, on Windows.
- The built-in copies of zlib, libpng, libjpeg, libtiff, expat, PCRE2, and libwebp, rather than system copies, so that builds are the same on every machine. GTK and its dependencies are the exception on Linux, because those must come from the system.
- No compatibility with wxWidgets 3.0 or 3.2 APIs, so that deprecated functions are not exposed.

The exact list lives in `scripts/build-native.ps1` and is documented beside each option.
