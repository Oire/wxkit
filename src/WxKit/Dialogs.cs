using WxKit.Interop;

namespace WxKit;

/// <summary>
/// A top-level window for a task the user finishes and dismisses. Wraps wxDialog. Show a dialog
/// with <see cref="ShowModal"/> inside a <c>using</c> block, which destroys it once it closes.
/// </summary>
public unsafe class Dialog : TopLevelWindow, IDisposable {
	internal Dialog(WxkWindow* native) : base(native) {
	}

	/// <summary>Shows the dialog and waits until the user dismisses it. Returns the id of the button that did, such as <see cref="StandardId.Ok"/>.</summary>
	public int ShowModal() => WxKitNative.DialogShowModal(Native);

	/// <summary>Destroys the dialog, unless wxWidgets already has.</summary>
	public void Dispose() {
		if (!IsDestroyed) Destroy();
		GC.SuppressFinalize(this);
	}
}

/// <summary>The platform's standard dialog for a short message and a few buttons. Wraps wxMessageDialog.</summary>
public unsafe class MessageDialog : Dialog {
	public MessageDialog(Window? parent, string message, string caption = "Message", MessageDialogStyle style = MessageDialogStyle.Ok | MessageDialogStyle.Center) : base(Create(parent, message, caption, style)) {
	}

	private static WxkWindow* Create(Window? parent, string message, string caption, MessageDialogStyle style) {
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(caption);
		fixed (char* messageText = message)
		fixed (char* captionText = caption) {
			return WxKitNative.MessageDialogNew(NativeOf(parent), messageText, Text.Length(message), captionText, Text.Length(caption), (long)style);
		}
	}
}

/// <summary>Shows a message in one call.</summary>
public static class MessageBox {
	/// <summary>Shows a <see cref="MessageDialog"/> and returns the id of the button that closed it, such as <see cref="StandardId.Yes"/>.</summary>
	public static int Show(string message, string caption = "Message", MessageDialogStyle style = MessageDialogStyle.Ok | MessageDialogStyle.Center, Window? parent = null) {
		using MessageDialog dialog = new(parent, message, caption, style);
		return dialog.ShowModal();
	}
}
