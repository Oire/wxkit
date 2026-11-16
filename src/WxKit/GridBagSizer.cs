using WxKit.Interop;

namespace WxKit;

// Adding takes SizerFlags, as Sizer.Add does, though a grid bag sizer only uses their flags and
// border, since its cells, not proportions, decide how items grow.
public unsafe partial class GridBagSizer {
	/// <summary>Adds a window at a cell. Returns false when an item is there already, which leaves the window out.</summary>
	public bool Add(Window window, GridPosition position, GridSpan? span = null, SizerFlags flags = default) {
		ArgumentNullException.ThrowIfNull(window);
		return WxKitNative.GridBagSizerAddWindow(Native, window.Native, position, span ?? GridSpan.Default, flags.FlagsValue, flags.BorderValue) != 0;
	}

	/// <summary>Adds a sizer at a cell, which then belongs to this one. Returns false when an item is there already, which leaves the sizer out.</summary>
	public bool Add(Sizer sizer, GridPosition position, GridSpan? span = null, SizerFlags flags = default) => Adopt(sizer, child => WxKitNative.GridBagSizerAddSizer(Native, (WxkSizer*)child, position, span ?? GridSpan.Default, flags.FlagsValue, flags.BorderValue) != 0);

	/// <summary>Adds empty space of a size at a cell. Returns false when an item is there already.</summary>
	public bool AddSpacer(Size size, GridPosition position, GridSpan? span = null, SizerFlags flags = default) => WxKitNative.GridBagSizerAddSpacer(Native, size, position, span ?? GridSpan.Default, flags.FlagsValue, flags.BorderValue) != 0;
}
