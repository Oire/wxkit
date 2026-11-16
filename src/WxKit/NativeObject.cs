using System.Runtime.InteropServices;
using WxKit.Interop;

namespace WxKit;

/// <summary>
/// A wxWidgets object WxKit owns, rather than a window wxWidgets owns: a value such as a font, a
/// bitmap, or an image, which shares its data by reference counting, or a drawing context. Dispose
/// it when you are done with it, which a drawing context needs promptly, or let the garbage
/// collector free it, which happens on the GUI thread.
/// </summary>
public abstract unsafe class NativeObject : IDisposable {
	private readonly ValueHandle _handle;

	private protected NativeObject(WxkObject* native) : this(native, owns: true) {
	}

	// An object wxWidgets owns, such as the DC a printout draws a page on, which WxKit only lends
	// for as long as wxWidgets says, and never frees.
	private protected NativeObject(WxkObject* native, bool owns) {
		if (native is null) throw new InvalidOperationException($"wxWidgets could not create the {GetType().Name}.");
		_handle = new ValueHandle(native) { Owns = owns };
	}

	/// <summary>Whether the object has been disposed, after which nothing else on it can be used.</summary>
	public bool IsDisposed => _handle.IsClosed;

	internal WxkObject* Native {
		get {
			Application.VerifyAccess();
			return !_handle.IsClosed ? (WxkObject*)_handle.DangerousGetHandle() : throw new ObjectDisposedException(GetType().Name);
		}
	}

	public void Dispose() {
		_handle.Dispose();
		GC.SuppressFinalize(this);
	}

	// Gives the object to wxWidgets, which frees it from then on, as a preview does its printouts.
	internal void Disown() => _handle.Owns = false;

	// Marks the object freed, once wxWidgets has freed one it was given.
	internal void Invalidate() {
		_handle.SetHandleAsInvalid();
		GC.SuppressFinalize(this);
	}

	// wxWidgets reference counts are not thread safe, so a reference is only ever released on the
	// GUI thread. One released anywhere else, as by the finalizer, is posted there. That also keeps
	// it alive through any native call the GUI thread is in the middle of with it, since the post
	// only runs once that call returns. Once the application has ended, wxWidgets is gone, and the
	// reference goes with the process.
	private sealed class ValueHandle : SafeHandle {
		public ValueHandle(WxkObject* native) : base(0, ownsHandle: true) => SetHandle((nint)native);

		public bool Owns { get; set; } = true;

		public override bool IsInvalid => handle == 0;

		protected override bool ReleaseHandle() {
			if (!Owns) return true;
			nint native = handle;
			if (Application.IsGuiThread) {
				WxKitNative.ObjectFree((WxkObject*)native);
			} else {
				Application.TryPost(() => WxKitNative.ObjectFree((WxkObject*)native));
			}
			return true;
		}
	}
}
