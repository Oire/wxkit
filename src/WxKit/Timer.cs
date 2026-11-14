using WxKit.Interop;

namespace WxKit;

/// <summary>
/// Raises <see cref="Tick"/> on the GUI thread after an interval, once or repeatedly. Wraps wxTimer.
/// Dispose it when you are done with it; wxWidgets does not free timers on its own.
/// </summary>
public sealed unsafe class Timer : EvtHandler, IDisposable {
	public Timer() : base((WxkEvtHandler*)WxKitNative.TimerNew()) {
	}

	public bool IsRunning => WxKitNative.TimerIsRunning(Native) != 0;

	private WxkTimer* Native => (WxkTimer*)Handle;

	public event EventHandler<Event>? Tick {
		add => AddHandler(EventType.Timer, StandardId.Any, StandardId.Any, value, this);
		remove => RemoveHandler(EventType.Timer, StandardId.Any, StandardId.Any, value);
	}

	/// <summary>Starts the timer, or restarts it if it is running. Returns false when the system could not start it.</summary>
	public bool Start(TimeSpan interval, bool oneShot = false) {
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);
		return WxKitNative.TimerStart(Native, checked((int)Math.Ceiling(interval.TotalMilliseconds)), oneShot ? (byte)1 : (byte)0) != 0;
	}

	public void Stop() => WxKitNative.TimerStop(Native);

	public void Dispose() {
		if (!IsDestroyed) WxKitNative.TimerFree(Native);
	}
}
