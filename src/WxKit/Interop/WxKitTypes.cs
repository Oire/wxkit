namespace WxKit.Interop;

/// <summary>A wxWindow, or any class derived from it. Only ever used through a pointer.</summary>
public struct WxkWindow;

/// <summary>Text the shim hands out, freed with <see cref="WxKitNative.StringFree"/>.</summary>
public struct WxkString;
