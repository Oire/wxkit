using WxKit.Interop;

namespace WxKit;

// The part of Event the generator cannot write: an event only exists while its handlers run.
/// <remarks>
/// An event only exists while its handlers run: after they return, every member throws
/// <see cref="ObjectDisposedException"/>, so copy out anything you need later, such as before an
/// <c>await</c>. A handler consumes its event unless it calls <see cref="Skip"/>, which lets
/// wxWidgets keep processing it, so a command event reaches the parent windows.
/// </remarks>
public unsafe partial class Event : EventArgs {
	private WxkEvent* _native;

	internal Event(WxkEvent* native) => _native = native;

	private protected WxkEvent* Handle {
		get {
			Application.VerifyAccess();
			return _native is not null ? _native : throw new ObjectDisposedException(GetType().Name, "An event can only be used while its handlers run.");
		}
	}

	// Runs after the handlers, for events whose default has to be applied once they have all had their say.
	internal virtual void OnHandled() {
	}

	internal void Invalidate() => _native = null;
}

/// <remarks>
/// Unlike other events, the window closes after the handlers return unless one of them calls
/// <see cref="Veto"/>.
/// </remarks>
public unsafe partial class CloseEvent {
	/// <summary>Keeps the window open. Throws when <see cref="CanVeto"/> is false.</summary>
	public void Veto(bool veto = true) {
		if (veto && !CanVeto) throw new InvalidOperationException("This close cannot be vetoed.");
		WxKitNative.CloseEventVeto(Handle, veto ? (byte)1 : (byte)0);
	}

	// wxWidgets only closes a window when its close handler destroys it or skips the event, which
	// is easy to forget. A handler that does nothing lets the window close, as .NET developers expect.
	internal override void OnHandled() {
		if (!IsVetoed) Skip();
	}
}
