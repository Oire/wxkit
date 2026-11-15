# WxKit

.NET bindings to [wxWidgets](https://wxwidgets.org), for native desktop applications. WxKit works with Native AOT, and links wxWidgets into the executable, so an application ships as one file of a few megabytes, with native controls that screen readers already understand.

WxKit is a preview. It runs on Windows, on x64 and ARM64, and its API will change before 1.0. macOS and Linux are planned.

## A first application

```csharp
using WxKit;

return Application.Run(() => {
	Frame frame = new(null, "Hello");
	Panel panel = new(frame);
	StaticText label = new(panel, "&Name:");
	TextCtrl name = new(panel);
	Button greet = new(panel, "&Greet");
	greet.Click += (_, _) => MessageBox.Show($"Hello, {name.Value}!", "Hello", MessageDialogStyle.Ok, frame);
	BoxSizer sizer = new(Orientation.Vertical);
	sizer.Add(label, SizerFlags.Default.Border());
	sizer.Add(name, SizerFlags.Default.Expand().Border());
	sizer.Add(greet, SizerFlags.Default.Border());
	panel.Sizer = sizer;
	frame.Show();
});
```

Run it as it is, or publish it as one executable:

```powershell
dotnet publish -r win-x64 -p:PublishAot=true
```

A Native AOT publish needs Visual Studio's C++ tools, as any Native AOT publish on Windows does.

## What it covers

The bindings are generated from wxWidgets's own headers: windows and the common controls, list, tree, and data view controls, toolbars, sizers, menus, the standard dialogs, drawing, images, validators, and the clipboard, with events and async support. Members C# overrides, such as a virtual list's rows and a self-drawn control's description for screen readers, call into .NET. An application only links the classes it uses.

## Packages

This package is the managed library. It brings in a native package for each runtime, such as `WxKit.Native.win-x64`, which holds wxWidgets and WxKit's shim, as a shared library and as the static libraries Native AOT links.

## How this was made

WxKit is heavily vibe coded: Claude, an AI model, wrote nearly all of its code and docs, under its author's direction. If AI-written code is a dealbreaker for you, this is not the project for you. The [repository](https://github.com/trypsynth/wxkit) has the design docs, the source, and the issue tracker.

## License

WxKit is MIT licensed. wxWidgets is under the wxWindows Library Licence, which allows applications that link it, statically or not, to use their own terms. The native packages list the libraries wxWidgets builds in, with their licenses.
