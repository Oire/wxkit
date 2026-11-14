using WxKit.Interop;

namespace WxKit;

/// <summary>
/// The system clipboard. Wraps wxTheClipboard. Each call opens the clipboard and closes it again,
/// so other applications can use it in between.
/// </summary>
/// <remarks>
/// Each setter replaces everything on the clipboard. To offer the same content in several formats,
/// so that each application takes the one it understands, set a <see cref="ClipboardContent"/>.
/// The setters return false when the clipboard could not be opened, which happens when another
/// application has it open.
/// </remarks>
public static unsafe class Clipboard {
	/// <summary>Whether the clipboard holds text.</summary>
	public static bool HasText {
		get {
			Application.VerifyAccess();
			return WxKitNative.ClipboardHasText() != 0;
		}
	}

	/// <summary>Whether the clipboard holds a bitmap.</summary>
	public static bool HasBitmap => Has(WxkClipboardFormat.Bitmap);

	/// <summary>Whether the clipboard holds a list of files, such as ones copied in a file manager.</summary>
	public static bool HasFiles => Has(WxkClipboardFormat.Files);

	/// <summary>Whether the clipboard holds HTML.</summary>
	public static bool HasHtml => Has(WxkClipboardFormat.Html);

	/// <summary>The clipboard's text, or null when it holds none.</summary>
	public static string? GetText() {
		Application.VerifyAccess();
		WxkString* text = WxKitNative.ClipboardGetText();
		return text is null ? null : NativeString.Take(text);
	}

	/// <summary>Puts text on the clipboard.</summary>
	public static bool SetText(string text) {
		ArgumentNullException.ThrowIfNull(text);
		Application.VerifyAccess();
		fixed (char* data = text) return WxKitNative.ClipboardSetText(data, NativeString.Length(text)) != 0;
	}

	/// <summary>The clipboard's bitmap, or null when it holds none.</summary>
	public static Bitmap? GetBitmap() {
		Application.VerifyAccess();
		return Bitmap.Take(WxKitNative.ClipboardGetBitmap());
	}

	/// <summary>Puts a bitmap on the clipboard.</summary>
	public static bool SetBitmap(Bitmap bitmap) {
		ArgumentNullException.ThrowIfNull(bitmap);
		return SetContent(new ClipboardContent { Bitmap = bitmap });
	}

	/// <summary>The paths of the files on the clipboard, or null when it holds none.</summary>
	public static string[]? GetFiles() {
		Application.VerifyAccess();
		WxkStringArray* files = WxKitNative.ClipboardGetFiles();
		return files is null ? null : NativeStringArray.Take(files);
	}

	/// <summary>Puts a list of files on the clipboard, which a file manager can paste.</summary>
	/// <param name="paths">The files' full paths.</param>
	public static bool SetFiles(IEnumerable<string> paths) {
		ArgumentNullException.ThrowIfNull(paths);
		return SetContent(new ClipboardContent { Files = [.. paths] });
	}

	/// <summary>The clipboard's HTML fragment, or null when it holds none.</summary>
	public static string? GetHtml() {
		Application.VerifyAccess();
		WxkString* html = WxKitNative.ClipboardGetHtml();
		return html is null ? null : NativeString.Take(html);
	}

	/// <summary>Puts an HTML fragment on the clipboard. Applications that paste rich text take it as formatted text.</summary>
	public static bool SetHtml(string html) {
		ArgumentNullException.ThrowIfNull(html);
		return SetContent(new ClipboardContent { Html = html });
	}

	/// <summary>Whether the clipboard holds data in a format the application names.</summary>
	/// <param name="format">The format's name, such as "MyApp.Shape".</param>
	public static bool HasData(string format) {
		ArgumentNullException.ThrowIfNull(format);
		Application.VerifyAccess();
		fixed (char* name = format) return WxKitNative.ClipboardHasData(name, NativeString.Length(format)) != 0;
	}

	/// <summary>The clipboard's data in a format the application names, or null when it holds none.</summary>
	/// <param name="format">The format's name, such as "MyApp.Shape".</param>
	public static byte[]? GetData(string format) {
		ArgumentNullException.ThrowIfNull(format);
		Application.VerifyAccess();
		nuint size;
		byte* data;
		fixed (char* name = format) data = WxKitNative.ClipboardGetData(name, NativeString.Length(format), &size);
		if (data is null) return null;
		try {
			return new ReadOnlySpan<byte>(data, checked((int)size)).ToArray();
		} finally {
			WxKitNative.BytesFree(data);
		}
	}

	/// <summary>Puts data in a format the application names on the clipboard, which only applications that know the format can paste.</summary>
	/// <param name="format">The format's name, such as "MyApp.Shape". The system registers it the first time it is used.</param>
	/// <param name="data">The data, which the clipboard copies.</param>
	public static bool SetData(string format, ReadOnlySpan<byte> data) {
		ClipboardContent content = new();
		content.Add(format, data);
		return SetContent(content);
	}

	/// <summary>Puts content on the clipboard in all the formats it has.</summary>
	public static bool SetContent(ClipboardContent content) {
		ArgumentNullException.ThrowIfNull(content);
		Application.VerifyAccess();
		if (content.IsEmpty) throw new ArgumentException("The content has no formats.", nameof(content));
		WxkClipboardContent* native = content.CreateNative();
		return WxKitNative.ClipboardSetContent(native) != 0;
	}

	/// <summary>Empties the clipboard.</summary>
	public static void Clear() {
		Application.VerifyAccess();
		WxKitNative.ClipboardClear();
	}

	/// <summary>
	/// Keeps what the application put on the clipboard after it exits. Otherwise the content goes
	/// with the application, since the clipboard asks the application for it when another one pastes.
	/// </summary>
	public static bool Flush() {
		Application.VerifyAccess();
		return WxKitNative.ClipboardFlush() != 0;
	}

	private static bool Has(WxkClipboardFormat format) {
		Application.VerifyAccess();
		return WxKitNative.ClipboardHas(format) != 0;
	}
}

/// <summary>
/// Content for the clipboard in several formats, such as text with the same text as HTML. Wraps
/// wxDataObjectComposite. Applications that paste take the format they understand, and those that
/// understand several prefer text, then HTML, then a bitmap, then files, then the named formats in
/// the order they were added.
/// </summary>
public sealed class ClipboardContent {
	private readonly List<(string Format, byte[] Data)> _data = [];

	/// <summary>Plain text, or null for none.</summary>
	public string? Text { get; set; }

	/// <summary>An HTML fragment, or null for none.</summary>
	public string? Html { get; set; }

	/// <summary>A bitmap, or null for none.</summary>
	public Bitmap? Bitmap { get; set; }

	/// <summary>The full paths of files, or null for none.</summary>
	public IReadOnlyList<string>? Files { get; set; }

	internal bool IsEmpty => Text is null && Html is null && Bitmap is null && Files is null && _data.Count == 0;

	/// <summary>Adds data in a format the application names.</summary>
	/// <param name="format">The format's name, such as "MyApp.Shape".</param>
	/// <param name="data">The data, which the content copies.</param>
	public void Add(string format, ReadOnlySpan<byte> data) {
		ArgumentException.ThrowIfNullOrEmpty(format);
		_data.Add((format, data.ToArray()));
	}

	internal unsafe WxkClipboardContent* CreateNative() {
		WxkClipboardContent* native = WxKitNative.ClipboardContentNew();
		try {
			if (Text is not null) {
				fixed (char* text = Text) WxKitNative.ClipboardContentAddText(native, text, NativeString.Length(Text));
			}
			if (Html is not null) {
				fixed (char* html = Html) WxKitNative.ClipboardContentAddHtml(native, html, NativeString.Length(Html));
			}
			if (Bitmap is not null) WxKitNative.ClipboardContentAddBitmap(native, Bitmap.Native);
			if (Files is not null) {
				using NativeStringArray files = new(Files);
				WxKitNative.ClipboardContentAddFiles(native, files.Handle);
			}
			foreach ((string format, byte[] data) in _data) {
				fixed (char* name = format)
				fixed (byte* bytes = data) {
					WxKitNative.ClipboardContentAddData(native, name, NativeString.Length(format), bytes, (nuint)data.Length);
				}
			}
			return native;
		} catch {
			WxKitNative.ClipboardContentFree(native);
			throw;
		}
	}
}
