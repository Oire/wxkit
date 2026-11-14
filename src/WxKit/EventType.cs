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
public static partial class EventType;
