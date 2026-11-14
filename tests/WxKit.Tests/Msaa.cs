using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WxKit.Tests;

// Microsoft Active Accessibility, as a screen reader reads a window. In the same process and on
// the GUI thread, Windows hands back the window's own IAccessible, so the calls reach WxKit
// directly, as a screen reader's do through COM.
[SupportedOSPlatform("windows")]
internal static partial class Msaa {
	public const int RoleClient = 0x0A;
	public const int RoleTable = 0x18;
	public const int RoleCell = 0x1D;
	public const int StateFocused = 0x04;
	public const int StateSelected = 0x02;
	private const uint ObjectIdClient = 0xFFFFFFFC;

	public static IAccessible Of(Window window) {
		Guid accessibleId = typeof(IAccessible).GUID;
		Marshal.ThrowExceptionForHR(AccessibleObjectFromWindow(window.NativeHandle, ObjectIdClient, ref accessibleId, out nint unknown));
		try {
			return (IAccessible)Marshal.GetObjectForIUnknown(unknown);
		} finally {
			Marshal.Release(unknown);
		}
	}

	[LibraryImport("oleacc.dll")]
	private static partial int AccessibleObjectFromWindow(nint window, uint objectId, ref Guid interfaceId, out nint accessible);
}

// IAccessible's members, in its vtable's order. A child id is a VARIANT, 0 for the object itself.
[ComImport]
[Guid("618736E0-3C3D-11CF-810C-00AA00389B71")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
internal interface IAccessible {
	[return: MarshalAs(UnmanagedType.IDispatch)]
	object GetParent();

	int GetChildCount();

	[return: MarshalAs(UnmanagedType.IDispatch)]
	object GetChild([MarshalAs(UnmanagedType.Struct)] object child);

	[return: MarshalAs(UnmanagedType.BStr)]
	string GetName([MarshalAs(UnmanagedType.Struct)] object child);

	[return: MarshalAs(UnmanagedType.BStr)]
	string GetValue([MarshalAs(UnmanagedType.Struct)] object child);

	[return: MarshalAs(UnmanagedType.BStr)]
	string GetDescription([MarshalAs(UnmanagedType.Struct)] object child);

	[return: MarshalAs(UnmanagedType.Struct)]
	object GetRole([MarshalAs(UnmanagedType.Struct)] object child);

	[return: MarshalAs(UnmanagedType.Struct)]
	object GetState([MarshalAs(UnmanagedType.Struct)] object child);

	[return: MarshalAs(UnmanagedType.BStr)]
	string GetHelp([MarshalAs(UnmanagedType.Struct)] object child);

	int GetHelpTopic([MarshalAs(UnmanagedType.BStr)] out string helpFile, [MarshalAs(UnmanagedType.Struct)] object child);

	[return: MarshalAs(UnmanagedType.BStr)]
	string GetKeyboardShortcut([MarshalAs(UnmanagedType.Struct)] object child);

	[return: MarshalAs(UnmanagedType.Struct)]
	object GetFocus();

	[return: MarshalAs(UnmanagedType.Struct)]
	object GetSelection();

	[return: MarshalAs(UnmanagedType.BStr)]
	string GetDefaultAction([MarshalAs(UnmanagedType.Struct)] object child);

	void Select(int flags, [MarshalAs(UnmanagedType.Struct)] object child);

	void Location(out int left, out int top, out int width, out int height, [MarshalAs(UnmanagedType.Struct)] object child);

	[return: MarshalAs(UnmanagedType.Struct)]
	object Navigate(int direction, [MarshalAs(UnmanagedType.Struct)] object start);

	[return: MarshalAs(UnmanagedType.Struct)]
	object HitTest(int left, int top);

	void DoDefaultAction([MarshalAs(UnmanagedType.Struct)] object child);
}
