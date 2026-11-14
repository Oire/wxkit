using WxKit.Interop;

namespace WxKit;

/// <summary>
/// An event, passed to its handlers. Wraps wxEvent. It only exists while its handlers run: after
/// they return, every member throws <see cref="ObjectDisposedException"/>, so copy out anything you
/// need later, such as before an <c>await</c>.
/// </summary>
public unsafe class Event : EventArgs {
	private WxkEvent* _native;

	internal Event(WxkEvent* native) => _native = native;

	/// <summary>The id of the window or menu item the event is from.</summary>
	public int Id => WxKitNative.EventGetId(Handle);

	/// <summary>The object the event is from, when it is one WxKit wraps.</summary>
	public EvtHandler? EventObject => ObjectRegistry.Wrap<EvtHandler>(WxKitNative.EventGetEventObject(Handle));

	/// <summary>Whether the event has been skipped, so wxWidgets keeps processing it after the handlers return.</summary>
	public bool IsSkipped => WxKitNative.EventGetSkipped(Handle) != 0;

	private protected WxkEvent* Handle {
		get {
			Application.VerifyAccess();
			return _native is not null ? _native : throw new ObjectDisposedException(GetType().Name, "An event can only be used while its handlers run.");
		}
	}

	/// <summary>
	/// Lets wxWidgets keep processing the event once the handlers return: a command event then
	/// reaches the parent windows, and a key press reaches the control. A handler consumes its
	/// event unless it calls this.
	/// </summary>
	public void Skip(bool skip = true) => WxKitNative.EventSkip(Handle, skip ? (byte)1 : (byte)0);

	// Runs after the handlers, for events whose default has to be applied once they have all had their say.
	internal virtual void OnHandled() {
	}

	internal void Invalidate() => _native = null;
}

/// <summary>An event from a control or menu item, such as a click. Wraps wxCommandEvent.</summary>
public unsafe class CommandEvent : Event {
	internal CommandEvent(WxkEvent* native) : base(native) {
	}

	/// <summary>The text that goes with the event, such as a text control's new value.</summary>
	public string String => Text.Take(WxKitNative.CommandEventGetString(Handle));

	/// <summary>The number that goes with the event, which depends on the control.</summary>
	public int Int => WxKitNative.CommandEventGetInt(Handle);

	/// <summary>The index of the item selected, for list controls.</summary>
	public int Selection => WxKitNative.CommandEventGetSelection(Handle);

	/// <summary>Whether a check box or check menu item is now checked.</summary>
	public bool IsChecked => WxKitNative.CommandEventIsChecked(Handle) != 0;
}

/// <summary>
/// A request to close a window. Wraps wxCloseEvent. Unlike other events, the window closes after
/// the handlers return unless one of them calls <see cref="Veto"/>.
/// </summary>
public unsafe class CloseEvent : Event {
	internal CloseEvent(WxkEvent* native) : base(native) {
	}

	/// <summary>Whether <see cref="Veto"/> can keep the window open. When the system is shutting down, it cannot.</summary>
	public bool CanVeto => WxKitNative.CloseEventCanVeto(Handle) != 0;

	public bool IsVetoed => WxKitNative.CloseEventGetVeto(Handle) != 0;

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
