using WxKit.Interop;

namespace WxKit;

/// <summary>
/// A window. wxWidgets owns every window: a child belongs to its parent, and a top-level window is
/// destroyed when it closes.
/// </summary>
public unsafe class Window {
	// The spike keeps the raw pointer. Tracking when wxWidgets destroys the window comes next.
	private protected Window(WxkWindow* native) {
		if (native is null) throw new InvalidOperationException($"wxWidgets could not create the {GetType().Name}.");
		Native = native;
	}

	internal WxkWindow* Native { get; }

	/// <summary>Shows or hides the window. Returns false when it was already in that state.</summary>
	public bool Show(bool show = true) => WxKitNative.WindowShow(Native, show ? (byte)1 : (byte)0) != 0;

	/// <summary>Asks the window to close, which a top-level window's close handler can veto unless <paramref name="force"/> is true.</summary>
	public bool Close(bool force = false) => WxKitNative.WindowClose(Native, force ? (byte)1 : (byte)0) != 0;
}
