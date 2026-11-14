using WxKit.Interop;

namespace WxKit;

// Image file formats are opted into by adding them. Each is a function of its own in the shim, so
// Native AOT only links the formats an application adds, and the image libraries behind them, the
// way a Cargo feature would. BMP is always there.
/// <remarks>
/// An image can only load or save the file formats that have been added, other than BMP. Add the
/// ones the application uses, such as with <see cref="AddPngHandler"/>, before loading images.
/// </remarks>
public partial class Image {
	/// <summary>Lets images load and save PNG files. Does nothing when they already can.</summary>
	public static void AddPngHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddPngHandler();
	}

	/// <summary>Lets images load and save JPEG files. Does nothing when they already can.</summary>
	public static void AddJpegHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddJpegHandler();
	}

	/// <summary>Lets images load and save TIFF files. Does nothing when they already can.</summary>
	public static void AddTiffHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddTiffHandler();
	}

	/// <summary>Lets images load and save GIF files. Does nothing when they already can.</summary>
	public static void AddGifHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddGifHandler();
	}

	/// <summary>Lets images load and save WebP files. Does nothing when they already can.</summary>
	public static void AddWebpHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddWebpHandler();
	}

	/// <summary>Lets images load and save PNM files. Does nothing when they already can.</summary>
	public static void AddPnmHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddPnmHandler();
	}

	/// <summary>Lets images load and save PCX files. Does nothing when they already can.</summary>
	public static void AddPcxHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddPcxHandler();
	}

	/// <summary>Lets images load and save IFF files. Does nothing when they already can.</summary>
	public static void AddIffHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddIffHandler();
	}

	/// <summary>Lets images load and save TGA files. Does nothing when they already can.</summary>
	public static void AddTgaHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddTgaHandler();
	}

	/// <summary>Lets images load and save XPM files. Does nothing when they already can.</summary>
	public static void AddXpmHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddXpmHandler();
	}

	/// <summary>Lets images load and save ICO, CUR, and ANI files. Does nothing when they already can.</summary>
	public static void AddIconHandler() {
		Application.VerifyAccess();
		WxKitNative.ImageAddIconHandler();
	}

	/// <summary>Lets images load and save every format wxWidgets has, which links all of them into the application.</summary>
	public static void AddAllHandlers() {
		Application.VerifyAccess();
		WxKitNative.ImageAddAllHandlers();
	}
}
