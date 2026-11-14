using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WxKit.Interop;

namespace WxKit;

/// <summary>
/// Describes a window to screen readers and other assistive technology, through Microsoft Active
/// Accessibility. Wraps wxAccessible. Derive from it to describe a control that draws itself, such
/// as a grid or a game board, whose parts Windows cannot see.
/// </summary>
/// <remarks>
/// <para>
/// Each member answers a question about the window itself, when the id is 0, or about one of its
/// parts, numbered from 1. A member returns null when it has no answer, and Windows then answers
/// from the window's standard accessible object, so a class only overrides what the control needs.
/// Standard controls are accessible already.
/// </para>
/// <para>
/// A member that throws <see cref="ArgumentOutOfRangeException"/> tells the screen reader that it
/// has no such part. Any other exception goes to <see cref="Application.UnhandledException"/>, and
/// the screen reader is told that the question failed.
/// </para>
/// <para>Microsoft Active Accessibility is Windows's, so on other platforms, nothing asks.</para>
/// </remarks>
public abstract unsafe class Accessible {
	private const int Failed = 0;
	private const int False = 1;
	private const int Ok = 2;
	private const int NotImplemented = 3;
	private const int NotSupported = 4;
	private const int InvalidArgument = 5;
	private static bool _callbacksSet;
	private WxkAccessible* _native;

	/// <summary>Gives the window this accessible object, which replaces the one it had. The window keeps it for as long as it exists.</summary>
	protected Accessible(Window window) {
		ArgumentNullException.ThrowIfNull(window);
		Application.VerifyAccess();
		SetCallbacks();
		Window = window;
		GCHandle handle = GCHandle.Alloc(this);
		_native = WxKitNative.ManagedAccessibleNew(window.Native, (void*)GCHandle.ToIntPtr(handle));
		if (_native is null) {
			handle.Free();
			throw new InvalidOperationException("The window could not be given an accessible object.");
		}
		window.AccessibleObject = this;
	}

	/// <summary>The window this describes.</summary>
	public Window Window { get; }

	internal WxkAccessible* Native => _native is not null ? _native : throw new InvalidOperationException("The window this accessible object described has been destroyed, or given another one.");

	/// <summary>
	/// Tells screen readers that something changed, such as the focus moving to another part, or a
	/// part's name or state changing. Wraps wxAccessible::NotifyEvent.
	/// </summary>
	/// <param name="eventType">What changed, such as <see cref="AccessibleEvent.ObjectFocus"/>.</param>
	/// <param name="window">The window it changed in.</param>
	/// <param name="objectType">Which of the window's objects, usually <see cref="AccessibleObject.Client"/>.</param>
	/// <param name="objectId">The part that changed, or 0 for the window itself.</param>
	public static void NotifyEvent(AccessibleEvent eventType, Window window, AccessibleObject objectType, int objectId) {
		ArgumentNullException.ThrowIfNull(window);
		Application.VerifyAccess();
		WxKitNative.AccessibleNotifyEvent((int)eventType, window.Native, (int)objectType, objectId);
	}

	/// <summary>The name of the window or a part, which a screen reader says first, such as a cell's text.</summary>
	protected virtual string? GetName(int childId) => null;

	/// <summary>A longer description of the window or a part.</summary>
	protected virtual string? GetDescription(int childId) => null;

	/// <summary>Help for the window or a part, like a tooltip.</summary>
	protected virtual string? GetHelpText(int childId) => null;

	/// <summary>The keyboard shortcut of the window or a part, such as "Alt+K".</summary>
	protected virtual string? GetKeyboardShortcut(int childId) => null;

	/// <summary>
	/// What the window's or a part's default action is, such as "Press" or "Check". It describes the
	/// action, not its result: a button that prints has the action "Press", not "Print". An empty
	/// string means that it has none.
	/// </summary>
	protected virtual string? GetDefaultAction(int childId) => null;

	/// <summary>The value of the window or a part, such as a slider's position or a cell's contents.</summary>
	protected virtual string? GetValue(int childId) => null;

	/// <summary>What kind of element the window or a part is, such as a cell or a button.</summary>
	protected virtual AccessibleRole? GetRole(int childId) => null;

	/// <summary>The state of the window or a part, such as focused or selected.</summary>
	protected virtual AccessibleStates? GetState(int childId) => null;

	/// <summary>How many parts the window has.</summary>
	protected virtual int? GetChildCount() => null;

	/// <summary>Where the window or a part is, in screen coordinates.</summary>
	/// <param name="elementId">0 for the window, or the part.</param>
	protected virtual Rect? GetLocation(int elementId) => null;

	/// <summary>
	/// A part, numbered from 1: <see cref="AccessibleElement.Child"/> for a simple part, which this
	/// object describes itself, or <see cref="AccessibleElement.Of"/> for one that has its own
	/// accessible object.
	/// </summary>
	protected virtual AccessibleElement? GetChild(int childId) => null;

	/// <summary>
	/// What has the keyboard focus: <see cref="AccessibleElement.Self"/>, a part, or
	/// <see cref="AccessibleElement.None"/> when nothing in the window does.
	/// </summary>
	protected virtual AccessibleElement? GetFocus() => null;

	/// <summary>What is at a point on the screen, or <see cref="AccessibleElement.None"/> when the point is outside the window.</summary>
	/// <param name="point">The point, in screen coordinates.</param>
	protected virtual AccessibleElement? HitTest(Point point) => null;

	/// <summary>The element in a direction from the window or a part, or <see cref="AccessibleElement.None"/> when there is none.</summary>
	protected virtual AccessibleElement? Navigate(NavigationDirection direction, int fromId) => null;

	/// <summary>Does the default action of the window or a part. Returns false when it has none.</summary>
	protected virtual bool? DoDefaultAction(int childId) => null;

	/// <summary>Selects the window or a part, or moves the focus to it, as the flags say. Returns false when it cannot.</summary>
	protected virtual bool? Select(int childId, AccessibleSelection flags) => null;

	private static void SetCallbacks() {
		if (_callbacksSet) return;
		_callbacksSet = true;
		WxkAccessibleCallbacks callbacks = new() {
			GetText = &GetText,
			GetRole = &GetRole,
			GetState = &GetState,
			GetChildCount = &GetChildCount,
			GetLocation = &GetLocation,
			GetElement = &GetElement,
			Act = &Act,
			OnFree = &OnFree,
		};
		WxKitNative.AccessibleSetCallbacks(&callbacks);
	}

	private static Accessible? Of(void* userdata) => (Accessible?)GCHandle.FromIntPtr((nint)userdata).Target;

	// A part that does not exist is an invalid argument, which is the screen reader's to handle.
	// Anything else is the application's.
	private static int Fail(Exception error) {
		if (error is ArgumentOutOfRangeException) return InvalidArgument;
		Application.ReportException(error);
		return Failed;
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static int GetText(void* userdata, int which, int childId, WxkString** text) {
		try {
			if (Of(userdata) is not Accessible accessible) return NotImplemented;
			string? value = which switch {
				0 => accessible.GetName(childId),
				1 => accessible.GetDescription(childId),
				2 => accessible.GetHelpText(childId),
				3 => accessible.GetKeyboardShortcut(childId),
				4 => accessible.GetDefaultAction(childId),
				_ => accessible.GetValue(childId),
			};
			if (value is null) return NotImplemented;
			*text = NativeString.Create(value);
			return Ok;
		} catch (Exception error) {
			return Fail(error);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static int GetRole(void* userdata, int childId, int* role) {
		try {
			if (Of(userdata)?.GetRole(childId) is not AccessibleRole value) return NotImplemented;
			*role = (int)value;
			return Ok;
		} catch (Exception error) {
			return Fail(error);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static int GetState(void* userdata, int childId, long* state) {
		try {
			if (Of(userdata)?.GetState(childId) is not AccessibleStates value) return NotImplemented;
			*state = (long)value;
			return Ok;
		} catch (Exception error) {
			return Fail(error);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static int GetChildCount(void* userdata, int* count) {
		try {
			if (Of(userdata)?.GetChildCount() is not int value) return NotImplemented;
			*count = value;
			return Ok;
		} catch (Exception error) {
			return Fail(error);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static int GetLocation(void* userdata, int elementId, Rect* rect) {
		try {
			if (Of(userdata)?.GetLocation(elementId) is not Rect value) return NotImplemented;
			*rect = value;
			return Ok;
		} catch (Exception error) {
			return Fail(error);
		}
	}

	// A part comes back as its id, and an element with its own accessible object as that object.
	// The focus on the window itself is the window's own object, as wxWidgets asks. An element
	// that is not there is not a failure: a point outside the window is false, as is a direction
	// with nothing in it, and a window without the focus is fine.
	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static int GetElement(void* userdata, int which, int childId, int direction, Point point, int* resultId, WxkAccessible** resultObject) {
		try {
			if (Of(userdata) is not Accessible accessible) return NotImplemented;
			AccessibleElement? answer = which switch {
				0 => accessible.GetChild(childId),
				1 => accessible.GetFocus(),
				2 => accessible.HitTest(point),
				_ => accessible.Navigate((NavigationDirection)direction, childId),
			};
			if (answer is not AccessibleElement element) return NotImplemented;
			if (element.IsNone) return which == 1 ? Ok : which == 0 ? InvalidArgument : False;
			if (element.Object is Accessible other) {
				*resultObject = other.Native;
			} else {
				*resultId = element.ChildId;
				if (which == 1 && element.ChildId == 0) *resultObject = accessible.Native;
			}
			return Ok;
		} catch (Exception error) {
			return Fail(error);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static int Act(void* userdata, int which, int childId, int flags) {
		try {
			if (Of(userdata) is not Accessible accessible) return NotImplemented;
			bool? done = which == 0 ? accessible.DoDefaultAction(childId) : accessible.Select(childId, (AccessibleSelection)flags);
			return done switch {
				null => NotImplemented,
				true => Ok,
				false => NotSupported,
			};
		} catch (Exception error) {
			return Fail(error);
		}
	}

	// The window deleted its accessible object, as it does when it is destroyed or given another.
	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void OnFree(void* userdata) {
		GCHandle handle = GCHandle.FromIntPtr((nint)userdata);
		if (handle.Target is Accessible accessible) {
			accessible._native = null;
			if (accessible.Window.AccessibleObject == accessible) accessible.Window.AccessibleObject = null;
		}
		handle.Free();
	}
}

/// <summary>
/// An element an <see cref="Accessible"/> points a screen reader to: the window itself, one of its
/// parts, an element with an accessible object of its own, or nothing.
/// </summary>
public readonly struct AccessibleElement {
	private AccessibleElement(int childId, Accessible? accessible, bool isNone) {
		ChildId = childId;
		Object = accessible;
		IsNone = isNone;
	}

	/// <summary>The window itself.</summary>
	public static AccessibleElement Self => default;

	/// <summary>No element, such as for a point outside the window.</summary>
	public static AccessibleElement None { get; } = new(0, null, isNone: true);

	/// <summary>The part, numbered from 1, or 0 for the window itself.</summary>
	public int ChildId { get; }

	/// <summary>The element's own accessible object, when it has one.</summary>
	public Accessible? Object { get; }

	public bool IsNone { get; }

	/// <summary>A part the accessible object describes itself, numbered from 1.</summary>
	public static AccessibleElement Child(int childId) {
		ArgumentOutOfRangeException.ThrowIfNegative(childId);
		return new(childId, null, isNone: false);
	}

	/// <summary>An element with an accessible object of its own, such as a child window's.</summary>
	public static AccessibleElement Of(Accessible accessible) {
		ArgumentNullException.ThrowIfNull(accessible);
		return new(0, accessible, isNone: false);
	}
}

// The accessible object WxKit gave a window, if any.
public partial class Window {
	/// <summary>The accessible object a class derived from <see cref="WxKit.Accessible"/> gave the window, or null when it has the standard one.</summary>
	public Accessible? AccessibleObject { get; internal set; }
}
