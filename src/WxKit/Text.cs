using WxKit.Interop;

namespace WxKit;

internal static unsafe class Text {
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

	internal static nuint Length(string text) => (nuint)text.Length;
}
