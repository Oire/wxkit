using System.Runtime.InteropServices;

namespace WxKit.Tests;

// Sends windows the messages Windows sends when the user uses a control, so tests can click a
// button or choose a menu item. SendMessage runs the handling before it returns, events included.
internal static partial class Win32 {
	private const uint WmCommand = 0x0111;

	// BM_CLICK only works on a button in the active window, so this sends the BN_CLICKED
	// notification a click makes the button send its parent, which works whether or not it is shown.
	public static void Click(Button button) => SendMessage(button.Parent!.NativeHandle, WmCommand, button.Id & 0xFFFF, button.NativeHandle);

	public static void ChooseMenuItem(Frame frame, MenuItem item) => SendMessage(frame.NativeHandle, WmCommand, item.Id & 0xFFFF, 0);

	[LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
	private static partial nint SendMessage(nint window, uint message, nint wParam, nint lParam);
}
