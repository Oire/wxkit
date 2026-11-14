namespace WxKit;

// A dialog is shown modally and then destroyed, which IDisposable lets a using block do.
/// <remarks>
/// Show a dialog with <see cref="ShowModal"/> inside a <c>using</c> block, which destroys it once
/// it closes.
/// </remarks>
public partial class Dialog : IDisposable {
	/// <summary>Destroys the dialog, unless wxWidgets already has.</summary>
	public void Dispose() {
		if (!IsDestroyed) Destroy();
		GC.SuppressFinalize(this);
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
