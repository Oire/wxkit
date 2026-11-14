namespace WxKit.Interop;

// Each handle type is only ever used through a pointer. As in the shim, every handle points at the
// start of the wxWidgets object, so a pointer to a derived class's handle can be cast to a base's.

/// <summary>Any wxObject.</summary>
public struct WxkObject;

/// <summary>A wxEvtHandler, or any class derived from it.</summary>
public struct WxkEvtHandler;

/// <summary>A wxWindow, or any class derived from it.</summary>
public struct WxkWindow;

/// <summary>A wxSizer, or any class derived from it.</summary>
public struct WxkSizer;

/// <summary>A wxMenu.</summary>
public struct WxkMenu;

/// <summary>A wxMenuItem.</summary>
public struct WxkMenuItem;

/// <summary>A wxTimer.</summary>
public struct WxkTimer;

/// <summary>A wxEvent, or any class derived from it. Only valid while its handlers run.</summary>
public struct WxkEvent;

/// <summary>A wxClassInfo, which lives as long as the program.</summary>
public struct WxkClassInfo;

/// <summary>Text the shim hands out, freed with <see cref="WxKitNative.StringFree"/>.</summary>
public struct WxkString;

/// <summary>A wxColour's channels, and whether it is set, as the shim passes them.</summary>
public readonly record struct WxkColor(byte Red, byte Green, byte Blue, byte Alpha, byte Valid);

/// <summary>A wxArrayString, freed with <see cref="WxKitNative.StringArrayFree"/>.</summary>
public struct WxkStringArray;

/// <summary>A list of windows, freed with <see cref="WxKitNative.WindowListFree"/>.</summary>
public struct WxkWindowList;

/// <summary>Clipboard content in several formats, which <see cref="WxKitNative.ClipboardSetContent"/> takes ownership of.</summary>
public struct WxkClipboardContent;

/// <summary>The standard clipboard formats, as the shim numbers them.</summary>
public enum WxkClipboardFormat {
	Text = 0,
	Bitmap = 1,
	Files = 2,
	Html = 3,
}
