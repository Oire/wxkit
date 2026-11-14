using WxKit.Interop;

namespace WxKit;

internal static unsafe class NativeWindowList {
	// Takes ownership of a list the shim hands out and frees it, wrapping each window in its one
	// managed object.
	public static Window[] Take(WxkWindowList* list) {
		if (list is null) return [];
		try {
			List<Window> windows = new(checked((int)WxKitNative.WindowListGetCount(list)));
			for (nuint index = 0; index < (nuint)windows.Capacity; index++) {
				if (ObjectRegistry.Wrap<Window>(WxKitNative.WindowListItem(list, index)) is Window window) windows.Add(window);
			}
			return [.. windows];
		} finally {
			WxKitNative.WindowListFree(list);
		}
	}
}
