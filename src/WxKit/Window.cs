using WxKit.Interop;

namespace WxKit;

/// <summary>
/// A window: a frame, a dialog, a panel, or a control. Wraps wxWindow. wxWidgets owns every window.
/// A child belongs to its parent and is destroyed with it, and a top-level window is destroyed when
/// it closes. Call <see cref="Destroy"/> to destroy one yourself.
/// </summary>
public unsafe class Window : EvtHandler {
	private Sizer? _sizer;

	internal Window(WxkWindow* native) : base((WxkEvtHandler*)native) {
	}

	/// <summary>The window's id, which its command events carry.</summary>
	public int Id => WxKitNative.WindowGetId(Native);

	/// <summary>The window's parent, or null for a top-level window without one.</summary>
	public Window? Parent => ObjectRegistry.Wrap<Window>(WxKitNative.WindowGetParent(Native));

	/// <summary>The window's label: a control's text, or a top-level window's title.</summary>
	public string Label {
		get => Text.Take(WxKitNative.WindowGetLabel(Native));
		set {
			ArgumentNullException.ThrowIfNull(value);
			fixed (char* text = value) WxKitNative.WindowSetLabel(Native, text, Text.Length(value));
		}
	}

	public bool IsShown => WxKitNative.WindowIsShown(Native) != 0;

	/// <summary>Whether the window accepts input. A disabled window's children are disabled too.</summary>
	public bool IsEnabled {
		get => WxKitNative.WindowIsEnabled(Native) != 0;
		set => WxKitNative.WindowEnable(Native, value ? (byte)1 : (byte)0);
	}

	/// <summary>
	/// The sizer that lays out the window's children, or null for none. A sizer belongs to the
	/// window it is set on: replacing it, or destroying the window, destroys it.
	/// </summary>
	public Sizer? Sizer {
		get {
			_ = Native;
			return _sizer;
		}
		set {
			WxkWindow* native = Native;
			if (ReferenceEquals(value, _sizer)) return;
			value?.Attach(this);
			WxKitNative.WindowSetSizer(native, value is null ? null : value.Native);
			_sizer?.MarkDestroyed();
			_sizer = value;
		}
	}

	internal WxkWindow* Native => (WxkWindow*)Handle;

	/// <summary>Shows or hides the window. Returns false when it was already shown or hidden.</summary>
	public bool Show(bool show = true) => WxKitNative.WindowShow(Native, show ? (byte)1 : (byte)0) != 0;

	public bool Hide() => Show(false);

	/// <summary>Moves the keyboard focus to the window.</summary>
	public void SetFocus() => WxKitNative.WindowSetFocus(Native);

	/// <summary>
	/// Asks the window to close, which raises its close event. A handler can veto the close unless
	/// <paramref name="force"/> is true. Returns whether the window closed.
	/// </summary>
	public bool Close(bool force = false) => WxKitNative.WindowClose(Native, force ? (byte)1 : (byte)0) != 0;

	/// <summary>
	/// Destroys the window and its children. A top-level window is destroyed once the event loop is
	/// next idle, so events already on their way still reach it.
	/// </summary>
	public bool Destroy() => WxKitNative.WindowDestroy(Native) != 0;

	/// <summary>Resizes the window to fit its children.</summary>
	public void Fit() => WxKitNative.WindowFit(Native);

	/// <summary>Lays the children out again with the sizer, after they change size or are added.</summary>
	public void Layout() => WxKitNative.WindowLayout(Native);

	/// <summary>Centers the window on its parent, or on the screen for a top-level window without one.</summary>
	public void Center(Orientation direction = Orientation.Both) => WxKitNative.WindowCentre(Native, (int)direction);

	private protected override void OnDestroyed() {
		_sizer?.MarkDestroyed();
		_sizer = null;
	}

	internal static WxkWindow* NativeOf(Window? window) => window is null ? null : window.Native;
}

/// <summary>A window with a title bar, such as a frame or a dialog. Wraps wxTopLevelWindow.</summary>
public unsafe class TopLevelWindow : Window {
	internal TopLevelWindow(WxkWindow* native) : base(native) {
	}

	public string Title {
		get => Text.Take(WxKitNative.TopLevelWindowGetTitle(Native));
		set {
			ArgumentNullException.ThrowIfNull(value);
			fixed (char* text = value) WxKitNative.TopLevelWindowSetTitle(Native, text, Text.Length(value));
		}
	}

	/// <summary>
	/// Raised when something asks the window to close, such as the user or <see cref="Window.Close"/>.
	/// The window closes once the handlers return, unless one calls <see cref="CloseEvent.Veto"/>.
	/// </summary>
	public event EventHandler<CloseEvent>? Closing {
		add => AddHandler(EventType.Close, StandardId.Any, StandardId.Any, value, this);
		remove => RemoveHandler(EventType.Close, StandardId.Any, StandardId.Any, value);
	}
}

/// <summary>A top-level window that can have a menu bar. Wraps wxFrame.</summary>
public unsafe class Frame : TopLevelWindow {
	private MenuBar? _menuBar;

	public Frame(Window? parent, string title, Point? position = null, Size? size = null, FrameStyle style = FrameStyle.Default, int id = StandardId.Any) : base(Create(parent, title, position, size, style, id)) {
	}

	internal Frame(WxkWindow* native) : base(native) {
	}

	/// <summary>The frame's menu bar. Setting it gives the menu bar to the frame, which destroys it along with itself.</summary>
	public MenuBar? MenuBar {
		get {
			_ = Native;
			return _menuBar;
		}
		set {
			WxkWindow* native = Native;
			WxKitNative.FrameSetMenuBar(native, NativeOf(value));
			_menuBar = value;
		}
	}

	private static WxkWindow* Create(Window? parent, string title, Point? position, Size? size, FrameStyle style, int id) {
		ArgumentNullException.ThrowIfNull(title);
		fixed (char* text = title) return WxKitNative.FrameNew(NativeOf(parent), id, text, Text.Length(title), position ?? Point.Default, size ?? Size.Default, (long)style);
	}
}

/// <summary>A window to hold controls, which gives them the right background and keyboard navigation. Wraps wxPanel.</summary>
public unsafe class Panel : Window {
	// wxTAB_TRAVERSAL | wxBORDER_NONE, wxPanel's own default.
	private const long DefaultStyle = 0x00080000 | 0x00200000;

	public Panel(Window parent, Point? position = null, Size? size = null, int id = StandardId.Any) : base(Create(parent, position, size, id)) {
	}

	internal Panel(WxkWindow* native) : base(native) {
	}

	private static WxkWindow* Create(Window parent, Point? position, Size? size, int id) {
		ArgumentNullException.ThrowIfNull(parent);
		return WxKitNative.PanelNew(parent.Native, id, position ?? Point.Default, size ?? Size.Default, DefaultStyle);
	}
}

/// <summary>A push button. Wraps wxButton.</summary>
public unsafe class Button : Window {
	/// <summary>
	/// Creates a button. With a standard id, such as <see cref="StandardId.Ok"/>, and no label, the
	/// button gets the platform's standard label for it.
	/// </summary>
	public Button(Window parent, string label = "", int id = StandardId.Any, Point? position = null, Size? size = null) : base(Create(parent, label, id, position, size)) {
	}

	internal Button(WxkWindow* native) : base(native) {
	}

	public event EventHandler<CommandEvent>? Click {
		add => AddHandler(EventType.Button, StandardId.Any, StandardId.Any, value, this);
		remove => RemoveHandler(EventType.Button, StandardId.Any, StandardId.Any, value);
	}

	private static WxkWindow* Create(Window parent, string label, int id, Point? position, Size? size) {
		ArgumentNullException.ThrowIfNull(parent);
		ArgumentNullException.ThrowIfNull(label);
		fixed (char* text = label) return WxKitNative.ButtonNew(parent.Native, id, text, Text.Length(label), position ?? Point.Default, size ?? Size.Default, 0);
	}
}

/// <summary>
/// A text label. Wraps wxStaticText. A screen reader names a control after the label created
/// just before it, so create a control's label first.
/// </summary>
public unsafe class StaticText : Window {
	public StaticText(Window parent, string label, int id = StandardId.Any, Point? position = null, Size? size = null) : base(Create(parent, label, id, position, size)) {
	}

	internal StaticText(WxkWindow* native) : base(native) {
	}

	private static WxkWindow* Create(Window parent, string label, int id, Point? position, Size? size) {
		ArgumentNullException.ThrowIfNull(parent);
		ArgumentNullException.ThrowIfNull(label);
		fixed (char* text = label) return WxKitNative.StaticTextNew(parent.Native, id, text, Text.Length(label), position ?? Point.Default, size ?? Size.Default, 0);
	}
}

/// <summary>A box to type text into, on one line or several. Wraps wxTextCtrl.</summary>
public unsafe class TextCtrl : Window {
	public TextCtrl(Window parent, string value = "", TextCtrlStyle style = TextCtrlStyle.None, int id = StandardId.Any, Point? position = null, Size? size = null) : base(Create(parent, value, style, id, position, size)) {
	}

	internal TextCtrl(WxkWindow* native) : base(native) {
	}

	/// <summary>The text. Setting it raises <see cref="TextChanged"/>; <see cref="ChangeValue"/> does not.</summary>
	public string Value {
		get => Text.Take(WxKitNative.TextCtrlGetValue(Native));
		set {
			ArgumentNullException.ThrowIfNull(value);
			fixed (char* text = value) WxKitNative.TextCtrlSetValue(Native, text, Text.Length(value));
		}
	}

	/// <summary>Raised whenever the text changes, whether the user or the program changed it.</summary>
	public event EventHandler<CommandEvent>? TextChanged {
		add => AddHandler(EventType.Text, StandardId.Any, StandardId.Any, value, this);
		remove => RemoveHandler(EventType.Text, StandardId.Any, StandardId.Any, value);
	}

	/// <summary>Sets the text without raising <see cref="TextChanged"/>.</summary>
	public void ChangeValue(string value) {
		ArgumentNullException.ThrowIfNull(value);
		fixed (char* text = value) WxKitNative.TextCtrlChangeValue(Native, text, Text.Length(value));
	}

	private static WxkWindow* Create(Window parent, string value, TextCtrlStyle style, int id, Point? position, Size? size) {
		ArgumentNullException.ThrowIfNull(parent);
		ArgumentNullException.ThrowIfNull(value);
		fixed (char* text = value) return WxKitNative.TextCtrlNew(parent.Native, id, text, Text.Length(value), position ?? Point.Default, size ?? Size.Default, (long)style);
	}
}
