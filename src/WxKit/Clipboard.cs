using WxKit.Interop;

namespace WxKit;

/// <summary>
/// The system clipboard's text. Wraps wxTheClipboard with a wxTextDataObject. Each call opens the
/// clipboard and closes it again, so other applications can use it in between.
/// </summary>
public static unsafe class Clipboard {
	/// <summary>Whether the clipboard holds text.</summary>
	public static bool HasText {
		get {
			Application.VerifyAccess();
			return WxKitNative.ClipboardHasText() != 0;
		}
	}

	/// <summary>The clipboard's text, or null when it holds none.</summary>
	public static string? GetText() {
		Application.VerifyAccess();
		WxkString* text = WxKitNative.ClipboardGetText();
		return text is null ? null : NativeString.Take(text);
	}

	/// <summary>Puts text on the clipboard. Returns false when the clipboard could not be opened.</summary>
	public static bool SetText(string text) {
		ArgumentNullException.ThrowIfNull(text);
		Application.VerifyAccess();
		fixed (char* data = text) return WxKitNative.ClipboardSetText(data, NativeString.Length(text)) != 0;
	}

	public static void Clear() {
		Application.VerifyAccess();
		WxKitNative.ClipboardClear();
	}
}
