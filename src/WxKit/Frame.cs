using WxKit.Interop;

namespace WxKit;

/// <summary>A top-level window with a title bar, and optionally a menu bar, toolbar, and status bar. Wraps wxFrame.</summary>
public unsafe class Frame : Window {
	public Frame(Window? parent, string title) : base(Create(parent, title)) {
	}

	private static WxkWindow* Create(Window? parent, string title) {
		ArgumentNullException.ThrowIfNull(title);
		fixed (char* text = title) return WxKitNative.FrameNew(parent is null ? null : parent.Native, -1, text, (nuint)title.Length);
	}
}
