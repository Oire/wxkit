# Managed API

This document describes the C# API that applications use: how wxWidgets names map to .NET names, who owns which objects, how events work, and how the application starts.

## A first look

This is the program that the first milestone aims for:

```csharp
using WxKit;

Application.Run(() => {
	Frame frame = new(null, "Hello");
	Panel panel = new(frame);
	BoxSizer sizer = new(Orientation.Vertical);
	StaticText label = new(panel, "&Name:");
	TextCtrl name = new(panel);
	Button greet = new(panel, "&Greet");
	greet.Click += (sender, e) => MessageBox.Show($"Hello, {name.Value}!", "Greeting", parent: frame);
	sizer.Add(label, SizerFlags.Default.Border());
	sizer.Add(name, SizerFlags.Default.Expand().Border());
	sizer.Add(greet, SizerFlags.Default.Border());
	panel.Sizer = sizer;
	frame.Show();
});
```

The label is created before the text box, so a screen reader names the text box after it, and the tab order follows. WxKit's samples follow this pattern throughout.

## Naming

WxKit uses the wxWidgets names, adjusted to .NET conventions:

- The `wx` prefix is dropped from types. `wxFrame` becomes `Frame`, and `wxBoxSizer` becomes `BoxSizer`. The `WxKit` namespace keeps them apart from other libraries' types.
- Methods keep their names, which wxWidgets already writes in Pascal case. `wxWindow::Refresh` becomes `Window.Refresh`.
- A getter and setter pair becomes a property. `GetLabel` and `SetLabel` become `Label`. `IsEnabled` and `Enable` become `IsEnabled`, with a setter. A getter without a setter becomes a read-only property, unless it is expensive or has parameters, in which case it stays a method.
- Constants become enums. Style flags become `[Flags]` enums, one per class, such as `ButtonStyle` and `FrameStyle`, holding the styles that wxWidgets documents for that class plus the shared window styles. Standard ids, such as `wxID_OK`, become `int` constants on a static `StandardId` class, such as `StandardId.Ok`. They are not an enum, because ids are plain numbers in wxWidgets, and an application's own ids sit beside the standard ones, so an enum would need a cast everywhere an id is passed.
- Parameters keep their wxWidgets names, converted to camel case, so that named arguments read naturally.

- Names use the American spellings that .NET uses, not the British ones that wxWidgets uses. `wxColour` becomes `Color`, `Centre` becomes `Center`, and `wxEVT_COLOUR_CHANGED` becomes `ColorChanged`. The generator applies a fixed list of these respellings, so a search of the wxWidgets documentation for a WxKit name only needs the British spelling put back. Each type's documentation comment names its wxWidgets class, so the original is always one step away.

Type names that collide with common .NET types, such as `Timer`, `Font`, `Image`, and `Menu`, are kept. The `WxKit` namespace separates them. `Timer` needs care: implicit usings import `System.Threading` into every new project, so a bare `Timer` would be ambiguous there. When a C# project has implicit usings on, WxKit's MSBuild targets add `global using Timer = WxKit.Timer;`. An alias wins over an imported namespace, so the bare name means WxKit's timer. Set the `WxKitTimerAlias` property to `false` to leave the alias out.

## Construction

wxWidgets supports one-step creation, where the constructor creates the native window, and two-step creation, where a default constructor is followed by `Create`. WxKit only exposes one-step creation. Two-step creation exists in C++ mostly for subclassing and for setting extra styles before the window exists, and the director and extra-style support cover those cases.

Constructors use optional parameters with the same defaults as wxWidgets. `wxDefaultPosition` and `wxDefaultSize` become `null`, and the parameter types are `Point?` and `Size?`. The window id defaults to `wxID_ANY`, which most code never needs to change, since WxKit's events are bound to objects rather than ids.

## Value types

Small wxWidgets classes that are values in practice become `readonly record struct` types with the same layout as their shim structs, so they cross the boundary without conversion:

- `Point`, `Size`, and `Rect`.
- `Color`, with its alpha channel.
- `SizerFlags`, which is built on the managed side and only converted to a `wxSizerFlags` when added.

## Object lifetime

wxWidgets has three kinds of objects, and each kind gets a different lifetime model in WxKit.

### Windows

wxWidgets owns windows. A child window belongs to its parent and is destroyed with it. A top-level window, such as a frame, is destroyed when it closes, or when you call `Destroy`. You never delete a window directly.

In WxKit, a window object is a proxy for the native window:

- Window classes do not implement `IDisposable`, because disposing a child would conflict with its parent's ownership. To destroy a window, call `Destroy`, or `Close` for a top-level window.
- Dialogs are the exception. wxWidgets recommends creating a modal dialog, showing it, and destroying it when `ShowModal` returns. `Dialog` implements `IDisposable` and calls `Destroy`, so `using MessageDialog dialog = new(...)` does the right thing.
- After the native window is destroyed, every member of its proxy throws `ObjectDisposedException`, except `IsDestroyed`. The shim's object tracking, described in [Native layer](native-layer.md), tells the proxy when this happens.
- A window that is alive keeps its proxy alive. The shim holds a strong `GCHandle` to each proxy, released when the native window is destroyed. This matters when you derive from `Frame` and add your own fields: your object lives exactly as long as the window does, even if nothing on the managed side refers to it.
- There is one proxy per native window. A registry maps native pointers to proxies, so `button.Parent` returns the same `Panel` object you created, not a new wrapper.

When wxWidgets hands WxKit a window it did not create, such as the result of `FindWindowById` or a dialog's own buttons, WxKit creates a proxy of the most specific class it knows. It reads the native object's `wxClassInfo` and walks up its base classes until it finds one that WxKit binds. A generated lookup maps class names to proxy constructors, with no reflection.

### Sizers

A sizer belongs to the window it is set on, or to the sizer it is added to, and is destroyed with it. Setting a window's sizer again destroys the one it had. Sizers are not trackable in wxWidgets, so WxKit tracks them through their owner: when a window is destroyed, its sizer and every sizer inside it are marked as destroyed too. A sizer can only have one owner, and giving it a second throws `InvalidOperationException`.

A sizer that is never given an owner is freed when its proxy is finalized. The finalizer runs on the finalizer thread, so it posts the free to the GUI thread. Removing or detaching an item from a sizer is not bound yet, and will need the same care.

### Reference-counted objects

Bitmaps, fonts, pens, brushes, images, icons, and cursors are reference counted in wxWidgets, and copying one is cheap. In WxKit, each of these owns a heap copy of the wxWidgets object, which holds one reference to its data, through a `SafeHandle` in the hand-written `NativeObject` base. They implement `IDisposable`.

wxWidgets reference counting is not thread safe, so a reference is only ever released on the GUI thread. `Dispose` on the GUI thread releases at once. A release anywhere else, as by the finalizer, is posted to the GUI thread. That also covers a native call still using the object when the collector finds it: the GUI thread is busy with that call, so the posted release only runs once it returns. Once the application has ended, wxWidgets is gone, and the reference goes with the process.

An object going in can be null, which stands for the class's empty object, as null for a font is `wxNullFont`. Setting a window's font or cursor to null restores its default. An object coming back empty is null rather than an object that cannot be used. An object passed by a reference that can change, such as the bitmap a `MemoryDC` draws into, is the managed object's own copy, so the change shows there.

A parameter that takes a bitmap or icon bundle, which wxWidgets 3.3 uses for every resolution, takes a `Bitmap` or `Icon`, the way C++ converts one.

### Drawing contexts

`DC` and its window, client, paint, memory, and screen classes are objects the application creates, uses, and disposes, which a paint handler needs to do before it returns:

```csharp
panel.Paint += (sender, e) => {
	using PaintDC dc = new(panel);
	dc.DrawLine(0, 0, 50, 50);
};
```

They derive from `NativeObject` too, and their finalizers post the delete to the GUI thread, but a drawing context should not wait for the collector. A context that wxWidgets hands back, such as an erase event's, is not WxKit's to delete, so members that return one are not bound yet.

### Validators

A validator checks a control's value, and moves data between the control and the application, when a dialog's OK button is pressed or `Window.Validate`, `TransferDataToWindow`, or `TransferDataFromWindow` runs. `TextValidator` is wxWidgets's, with its filters as `TextValidatorStyle`. A validator written in C# derives from `CustomValidator` and overrides the checks it needs:

```csharp
sealed class Required : CustomValidator {
	protected override bool OnValidate(Window window, Window? parent) {
		if (((TextCtrl)window).Value.Length > 0) return true;
		MessageBox.Show("Fill this in first.");
		return false;
	}
}

name.Validator = new Required();
```

A control holds its own copy of the validator, as in wxWidgets, so one validator can serve several controls, and a change to a `TextValidator` after it is set does not reach the control until it is set again. Validators derive from `NativeObject`, though `wxValidator` is an event handler, since WxKit creates and deletes them. A `CustomValidator` stays alive while any control holds a copy of it, and can be collected after that. An exception in one of its checks goes to `Application.UnhandledException`, and the check fails.

## Opting in to features

WxKit has no switches for optional features, as wxDragon has Cargo features. Native AOT only compiles the methods an application can reach, so a shim function nothing calls is never referenced, and the linker leaves out its object file and the libraries behind it. Opting in is calling the API.

Image file formats work this way. An image can always load and save BMP files, and an application adds the others it uses, such as with `Image.AddPngHandler()`, before it loads images. Each format is a function in a file of its own in the shim, so only the formats an application adds are linked. `Image.AddAllHandlers()` adds every format, which makes a published application about 1.1 MB larger, mostly for JPEG, TIFF, and WebP. PNG costs nothing extra, since wxMSW uses it itself.

Without Native AOT, the shared library has every format in it either way, and the calls only decide which ones images can use.

## Static members

A static member of a wxWidgets class is a static member of its managed class, such as `Window.FindFocus()`. A class with only static members, such as `wxSystemSettings`, is a static class. Static members check the thread first, as instance members do through their handle.

## Events

Each class exposes the events that wxWidgets documents for it as standard .NET events:

```csharp
button.Click += OnClick;
frame.Closing += (sender, e) => {
	if (HasUnsavedChanges && e.CanVeto) e.Veto();
};
textCtrl.TextChanged += (sender, e) => UpdateTitle();
```

Each event uses `EventHandler<T>`, where `T` is the WxKit class for the wxWidgets event, such as `CommandEvent` or `CloseEvent`. These classes derive from `EventArgs`, which keeps the pattern familiar and lets Visual Basic's `Handles` clause work.

The first `+=` for an event type and id range binds one functor in the shim, and later handlers for the same event are added to that functor's list on the managed side. `-=` removes a handler from the list. The functor itself is never unbound: once its list is empty, it skips each event, which leaves wxWidgets to process it as if nothing were bound. The functor is released, and the list with it, when wxWidgets destroys the object. This avoids having to match functors when unbinding, and costs at most one idle functor per event a program ever handled. Event names come from the wxWidgets event macros, converted to .NET style. `EVT_BUTTON` becomes `Click`, `EVT_CLOSE_WINDOW` becomes `Closing`, and so on. A binding tweak sets the name when the conversion reads badly.

For the rare event that a class does not expose, or for an event type the application defines itself, there is a generic method:

```csharp
frame.Bind(EventType.Menu, StandardId.Exit, (sender, e) => frame.Close());
```

### Event objects

A wxWidgets event object only exists while its handlers run. WxKit's event objects follow the same rule: once the handler returns, any member of the event throws `ObjectDisposedException`. Copy what you need out of an event before you start async work.

By default, a handler consumes its event, as in wxWidgets. Call `e.Skip()` to let wxWidgets keep processing it, which some events need. For example, a key-down handler that does not call `Skip` stops the text control from receiving the key.

The close event is the exception. In wxWidgets, a window only closes if its close handler destroys it or skips the event, which is easy to forget. In WxKit, `Closing` works like `FormClosing` in Windows Forms: the window closes once the handlers return, unless one of them calls `e.Veto()`.

A menu item's `Click` is bound on the item's menu, with the item's id. wxWidgets sends a menu event to the menu first, so the handler runs whether or not the menu is on a frame's menu bar, and the sender is the `MenuItem`.

### Exceptions in handlers

An exception cannot unwind through wxWidgets, so WxKit catches every exception a handler throws and raises `Application.UnhandledException`, whose arguments are an `ApplicationExceptionEventArgs`. The same goes for actions posted with `Application.Post` and for `await` continuations. A handler of that event can mark the exception handled, and the application keeps running. If nothing does, WxKit exits the event loop and rethrows the exception from `Application.Run`, with its original stack trace. This keeps an exception from being lost and keeps a broken application from limping on.

## Threading

wxWidgets is single-threaded. Every window must be created and used on the thread that runs the event loop.

- Every member of a window proxy checks that it is on the GUI thread, and throws `InvalidOperationException` if not. The check is one comparison, which costs little next to a native call.
- `Application.Post(Action)` runs an action on the GUI thread later, using `wxApp::CallAfter`. It is safe to call from any thread.
- `Application.Run` installs a `SynchronizationContext` for the GUI thread, so `await` in an event handler resumes on the GUI thread, as it does in Windows Forms and WPF.

On macOS, the GUI thread must be the process's main thread, so `Application.Run` runs the event loop on the thread that calls it.

On Windows, the GUI thread must be a single-threaded apartment, because OLE only works in one, and the clipboard, drag and drop, and accessibility use OLE. .NET starts `Main` in the multithreaded apartment unless it is marked `[STAThread]`, which top-level statements cannot be, and a thread's apartment cannot change once it is set. If wxWidgets runs on such a thread, it fails to start OLE without saying so. So when `Application.Run` is called from a thread that is not a single-threaded apartment, it starts one, runs the event loop there, and waits for it to end. A program whose `Main` is marked `[STAThread]` runs the event loop on its own main thread.

## The application object

`Application.Run` creates the shim's `wxApp` subclass, runs your startup code inside `OnInit`, and runs the event loop until the last top-level window closes. It returns the exit code.

For applications that need more, such as handling files opened from the macOS Finder or customizing `OnExit`, you can derive from `Application` and override its virtual methods:

```csharp
return new MyApp().Run();

sealed class MyApp : Application {
	protected override bool OnInit() {
		new MainFrame().Show();
		return true;
	}

	protected override void MacOpenFiles(IReadOnlyList<string> paths) { ... }
}
```

## Nullability

Every public API carries nullable annotations. The wxWidgets documentation rarely says whether a pointer can be null, so the generator assumes it cannot, and binding tweaks mark the exceptions, such as parent windows and `FindWindow` results.

## Documentation comments

The wxWidgets interface headers contain the reference documentation for every class and method. The generator turns the brief description of each one into an XML documentation comment, so IntelliSense shows it, with C++ names in the text rewritten to their WxKit names. Each type's comment also names its wxWidgets class and links to its page in the wxWidgets reference. Detailed descriptions stay in the wxWidgets reference, because they refer to C++ names and macros throughout.

## The raw interop layer

`WxKit.Interop.WxKitNative` declares every shim function, one to one, and it is public. It is an escape hatch for anything the managed API does not wrap yet. Code that uses it is unsafe and gives up the lifetime and threading checks, so the managed API is always the first choice. The shim can still change between preview releases, and code that calls it directly may need updating when it does.
