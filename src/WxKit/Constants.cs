namespace WxKit;

/// <summary>
/// The ids wxWidgets gives standard commands, such as wxID_OK. A button or menu item with one of
/// these gets the platform's standard label, and a dialog's buttons with them close it.
/// </summary>
public static class StandardId {
	/// <summary>Lets wxWidgets pick an id. Wraps wxID_ANY.</summary>
	public const int Any = -1;
	public const int Open = 5000;
	public const int Close = 5001;
	public const int New = 5002;
	public const int Save = 5003;
	public const int SaveAs = 5004;
	public const int Revert = 5005;
	public const int Exit = 5006;
	public const int Undo = 5007;
	public const int Redo = 5008;
	public const int Help = 5009;
	public const int Print = 5010;
	public const int PrintSetup = 5011;
	public const int PageSetup = 5012;
	public const int Preview = 5013;
	public const int About = 5014;
	public const int Ok = 5100;
	public const int Cancel = 5101;
	public const int Apply = 5102;
	public const int Yes = 5103;
	public const int No = 5104;
}

/// <summary>Which way a sizer lays its items out, or which way to center a window.</summary>
[Flags]
public enum Orientation {
	Horizontal = 0x0004,
	Vertical = 0x0008,
	Both = Horizontal | Vertical,
}

/// <summary>The sides of an item that a border applies to.</summary>
[Flags]
public enum Direction {
	Left = 0x0010,
	Right = 0x0020,
	Top = 0x0040,
	Bottom = 0x0080,
	All = Left | Right | Top | Bottom,
}

/// <summary>Styles for <see cref="Frame"/>.</summary>
[Flags]
public enum FrameStyle : long {
	None = 0,
	ResizeBorder = 0x0040,
	MaximizeBox = 0x0200,
	MinimizeBox = 0x0400,
	SystemMenu = 0x0800,
	CloseBox = 0x1000,
	StayOnTop = 0x8000,
	ClipChildren = 0x00400000,
	Caption = 0x20000000,
	/// <summary>A normal, resizable frame. Wraps wxDEFAULT_FRAME_STYLE.</summary>
	Default = SystemMenu | ResizeBorder | MinimizeBox | MaximizeBox | CloseBox | Caption | ClipChildren,
}

/// <summary>Styles for <see cref="TextCtrl"/>.</summary>
[Flags]
public enum TextCtrlStyle : long {
	None = 0,
	ReadOnly = 0x0010,
	MultiLine = 0x0020,
	ProcessEnter = 0x0400,
	Password = 0x0800,
}

/// <summary>Buttons, icons, and options for <see cref="MessageDialog"/>.</summary>
[Flags]
public enum MessageDialogStyle : long {
	Center = 0x00000001,
	Yes = 0x00000002,
	Ok = 0x00000004,
	No = 0x00000008,
	YesNo = Yes | No,
	Cancel = 0x00000010,
	/// <summary>Makes No the default button. Only valid with <see cref="YesNo"/>.</summary>
	NoDefault = 0x00000080,
	IconWarning = 0x00000100,
	IconError = 0x00000200,
	IconQuestion = 0x00000400,
	IconInformation = 0x00000800,
	Help = 0x00001000,
	IconNone = 0x00040000,
	/// <summary>Makes Cancel the default button. Only valid with <see cref="Cancel"/>.</summary>
	CancelDefault = 0x80000000,
}

/// <summary>What kind of item a menu item is.</summary>
public enum MenuItemKind {
	Normal = 0,
	Check = 1,
	Radio = 2,
}
