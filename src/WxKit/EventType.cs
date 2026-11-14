using WxKit.Interop;

namespace WxKit;

/// <summary>A kind of event, such as a button click, and the class its handlers receive. Wraps wxEventType.</summary>
public sealed unsafe class EventType<TEvent> where TEvent : Event {
	private readonly Func<int> _resolve;
	private readonly Func<nint, TEvent> _create;
	private int? _value;

	internal EventType(Func<int> resolve, Func<nint, TEvent> create) {
		_resolve = resolve;
		_create = create;
	}

	/// <summary>The number wxWidgets gives this kind of event, which can differ between runs.</summary>
	public int Value => _value ??= _resolve();

	internal TEvent Create(WxkEvent* native) => _create((nint)native);
}

/// <summary>The kinds of events WxKit knows. Wraps the wxEVT_ constants.</summary>
public static unsafe class EventType {
	/// <summary>A button was clicked. Wraps wxEVT_BUTTON.</summary>
	public static EventType<CommandEvent> Button { get; } = new(WxKitNative.EventTypeButton, static native => new CommandEvent((WxkEvent*)native));

	/// <summary>A menu item was chosen. Wraps wxEVT_MENU.</summary>
	public static EventType<CommandEvent> Menu { get; } = new(WxKitNative.EventTypeMenu, static native => new CommandEvent((WxkEvent*)native));

	/// <summary>A text control's text changed. Wraps wxEVT_TEXT.</summary>
	public static EventType<CommandEvent> Text { get; } = new(WxKitNative.EventTypeText, static native => new CommandEvent((WxkEvent*)native));

	/// <summary>A window was asked to close. Wraps wxEVT_CLOSE_WINDOW.</summary>
	public static EventType<CloseEvent> Close { get; } = new(WxKitNative.EventTypeCloseWindow, static native => new CloseEvent((WxkEvent*)native));

	/// <summary>A timer fired. Wraps wxEVT_TIMER.</summary>
	public static EventType<Event> Timer { get; } = new(WxKitNative.EventTypeTimer, static native => new Event((WxkEvent*)native));
}
