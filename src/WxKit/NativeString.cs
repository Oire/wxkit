using WxKit.Interop;

namespace WxKit;

internal static unsafe class NativeString {
	// Takes ownership of the shim's string and frees it.
	internal static string Take(WxkString* text) {
		if (text is null) return string.Empty;
		try {
			nuint length;
			char* data = WxKitNative.StringGetData(text, &length);
			return new string(data, 0, checked((int)length));
		} finally {
			WxKitNative.StringFree(text);
		}
	}

	// Hands the shim a copy of the text, which the shim frees, as a director's callback returns it.
	internal static WxkString* Create(string? text) {
		if (string.IsNullOrEmpty(text)) return null;
		fixed (char* data = text) return WxKitNative.StringNew(data, Length(text));
	}

	internal static nuint Length(string text) => (nuint)text.Length;
}
