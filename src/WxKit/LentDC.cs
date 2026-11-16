using WxKit.Interop;

namespace WxKit;

public unsafe partial class DC {
	private DC(WxkObject* native, bool owns) : base(native, owns) {
	}

	// A DC wxWidgets owns, which it lends for the length of a call, such as a printout's page.
	// Disposing it, when the call returns, only stops it being used.
	internal static DC Lend(WxkObject* native) => new(native, owns: false);
}
