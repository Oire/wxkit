using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WxKit.Interop;

/// <summary>
/// The shim's C interface, one to one. The types in <see cref="WxKit"/> wrap it safely, and calling
/// it directly gives up their lifetime and threading checks. It can change between preview releases.
/// </summary>
public static unsafe partial class WxKitNative {
	public const string Library = "wxkitnative";

	[LibraryImport(Library, EntryPoint = "wxk_App_Run")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int AppRun(delegate* unmanaged[Cdecl]<void*, byte> onInit, void* userdata);

	[LibraryImport(Library, EntryPoint = "wxk_App_CallAfter")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte AppCallAfter(delegate* unmanaged[Cdecl]<void*, void> callback, void* userdata);

	[LibraryImport(Library, EntryPoint = "wxk_App_ExitMainLoop")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void AppExitMainLoop();

	[LibraryImport(Library, EntryPoint = "wxk_Version_GetString")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* VersionGetString();

	[LibraryImport(Library, EntryPoint = "wxk_String_GetData")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial char* StringGetData(WxkString* text, nuint* length);

	[LibraryImport(Library, EntryPoint = "wxk_String_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void StringFree(WxkString* text);

	[LibraryImport(Library, EntryPoint = "wxk_Frame_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* FrameNew(WxkWindow* parent, int id, char* title, nuint titleLength);

	[LibraryImport(Library, EntryPoint = "wxk_Window_Show")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte WindowShow(WxkWindow* window, byte show);

	[LibraryImport(Library, EntryPoint = "wxk_Window_Close")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte WindowClose(WxkWindow* window, byte force);
}
