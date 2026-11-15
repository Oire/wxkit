using WxKit.Interop;

namespace WxKit;

// The handles of a list of bitmaps, such as a tree's images, for the length of one call. The
// bitmaps stay alive until it is disposed, so none is freed while wxWidgets reads it.
internal readonly unsafe ref struct NativeBitmapList {
	private readonly Bitmap[] _bitmaps;

	public NativeBitmapList(IEnumerable<Bitmap> bitmaps) {
		ArgumentNullException.ThrowIfNull(bitmaps);
		_bitmaps = [.. bitmaps];
		Handles = new nint[_bitmaps.Length];
		for (int index = 0; index < _bitmaps.Length; index++) {
			Bitmap bitmap = _bitmaps[index] ?? throw new ArgumentNullException(nameof(bitmaps), "A list of bitmaps cannot hold null.");
			Handles[index] = (nint)bitmap.Native;
		}
	}

	public nint[] Handles { get; }

	public void Dispose() => GC.KeepAlive(_bitmaps);
}
