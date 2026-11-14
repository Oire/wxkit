using System.Runtime.InteropServices;
using WxKit.Interop;

namespace WxKit;

/// <summary>
/// A wxWidgets object that is copied like a value and shares its data by reference counting, such
/// as a font, a bitmap, or an image. Each one owns its own reference. Dispose it when you are done
/// with it, or let the garbage collector free it, which happens on the GUI thread.
/// </summary>
public abstract unsafe class NativeValue : IDisposable {
	private readonly ValueHandle _handle;

	private protected NativeValue(WxkObject* native) {
		if (native is null) throw new InvalidOperationException($"wxWidgets could not create the {GetType().Name}.");
		_handle = new ValueHandle(native);
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

	// wxWidgets reference counts are not thread safe, so a reference is only ever released on the
	// GUI thread. One released anywhere else, as by the finalizer, is posted there. That also keeps
	// it alive through any native call the GUI thread is in the middle of with it, since the post
	// only runs once that call returns. Once the application has ended, wxWidgets is gone, and the
	// reference goes with the process.
	private sealed class ValueHandle : SafeHandle {
		public ValueHandle(WxkObject* native) : base(0, ownsHandle: true) => SetHandle((nint)native);

		public override bool IsInvalid => handle == 0;

		protected override bool ReleaseHandle() {
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
