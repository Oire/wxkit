using WxKit.Interop;

namespace WxKit;

/// <summary>
/// Sends mouse and keyboard input as if the user had given it, as a test of an application's
/// user interface does. Wraps wxUIActionSimulator. Each method returns false when the system could
/// not send the input.
/// </summary>
/// <remarks>
/// The input goes where the system sends real input: to the window under the mouse, or the one
/// with the keyboard focus, which must be in a window that is shown and active. It arrives as the
/// event loop runs, not before the method returns. On macOS, the application needs permission to
/// control the computer, in the Privacy and Security settings.
/// </remarks>
public static unsafe class UIActionSimulator {
	/// <summary>Moves the mouse to a point, in screen coordinates.</summary>
	public static bool MouseMove(Point point) => Send(() => WxKitNative.UIActionSimulatorMouseMove(point));

	public static bool MouseDown(MouseButton button = MouseButton.Left) => Send(() => WxKitNative.UIActionSimulatorMouseDown((int)button));

	public static bool MouseUp(MouseButton button = MouseButton.Left) => Send(() => WxKitNative.UIActionSimulatorMouseUp((int)button));

	/// <summary>Presses and releases a mouse button where the mouse is.</summary>
	public static bool MouseClick(MouseButton button = MouseButton.Left) => Send(() => WxKitNative.UIActionSimulatorMouseClick((int)button));

	public static bool MouseDoubleClick(MouseButton button = MouseButton.Left) => Send(() => WxKitNative.UIActionSimulatorMouseDoubleClick((int)button));

	/// <summary>Presses a key, holding the modifiers, without releasing it.</summary>
	public static bool KeyDown(KeyCode key, KeyModifiers modifiers = KeyModifiers.None) => Send(() => WxKitNative.UIActionSimulatorKeyDown((int)key, (int)modifiers));

	public static bool KeyUp(KeyCode key, KeyModifiers modifiers = KeyModifiers.None) => Send(() => WxKitNative.UIActionSimulatorKeyUp((int)key, (int)modifiers));

	/// <summary>Presses and releases a key, holding the modifiers, such as Ctrl+S, which a letter's code is the character of, as in <c>(KeyCode)'S'</c>.</summary>
	public static bool Char(KeyCode key, KeyModifiers modifiers = KeyModifiers.None) => Send(() => WxKitNative.UIActionSimulatorChar((int)key, (int)modifiers));

	/// <summary>Types text, one key at a time, which can only use the keys of a US keyboard.</summary>
	public static bool Text(string text) {
		ArgumentNullException.ThrowIfNull(text);
		Application.VerifyAccess();
		fixed (char* data = text) return WxKitNative.UIActionSimulatorText(data, NativeString.Length(text)) != 0;
	}

	/// <summary>Chooses the item with the text in the control with the keyboard focus, such as a choice or combo box, as a user would with the keyboard.</summary>
	public static bool Select(string text) {
		ArgumentNullException.ThrowIfNull(text);
		Application.VerifyAccess();
		fixed (char* data = text) return WxKitNative.UIActionSimulatorSelect(data, NativeString.Length(text)) != 0;
	}

	private static bool Send(Func<byte> send) {
		Application.VerifyAccess();
		return send() != 0;
	}
}
