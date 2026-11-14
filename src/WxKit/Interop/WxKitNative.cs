using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WxKit.Interop;

/// <summary>
/// The shim's C interface, one to one. The types in <see cref="WxKit"/> wrap it safely, and calling
/// it directly gives up their lifetime and threading checks. It can change between preview releases.
/// </summary>
public static unsafe partial class WxKitNative {
	public const string Library = "wxkitnative";

	// Application

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

	// Text

	[LibraryImport(Library, EntryPoint = "wxk_String_GetData")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial char* StringGetData(WxkString* text, nuint* length);

	[LibraryImport(Library, EntryPoint = "wxk_String_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void StringFree(WxkString* text);

	// Objects

	[LibraryImport(Library, EntryPoint = "wxk_EvtHandler_Track")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte EvtHandlerTrack(WxkEvtHandler* handler, delegate* unmanaged[Cdecl]<void*, void> onDestroy, void* userdata);

	[LibraryImport(Library, EntryPoint = "wxk_Object_GetClassInfo")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkClassInfo* ObjectGetClassInfo(WxkObject* obj);

	[LibraryImport(Library, EntryPoint = "wxk_ClassInfo_GetClassName")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* ClassInfoGetClassName(WxkClassInfo* info);

	[LibraryImport(Library, EntryPoint = "wxk_ClassInfo_GetBaseClass")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkClassInfo* ClassInfoGetBaseClass(WxkClassInfo* info);

	// Events

	[LibraryImport(Library, EntryPoint = "wxk_EvtHandler_Bind")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte EvtHandlerBind(WxkEvtHandler* handler, int eventType, int id, int lastId, delegate* unmanaged[Cdecl]<void*, WxkEvent*, void> callback, void* userdata, delegate* unmanaged[Cdecl]<void*, void> onFree);

	[LibraryImport(Library, EntryPoint = "wxk_Event_GetEventType")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int EventGetEventType(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_CloseEvent_Veto")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void CloseEventVeto(WxkEvent* e, byte veto);

	// Windows

	[LibraryImport(Library, EntryPoint = "wxk_Window_GetHandle")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void* WindowGetHandle(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_SetSizer")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void WindowSetSizer(WxkWindow* window, WxkSizer* sizer);

	[LibraryImport(Library, EntryPoint = "wxk_Frame_SetMenuBar")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void FrameSetMenuBar(WxkWindow* frame, WxkWindow* menuBar);

	// Dialogs

	// Sizers

	[LibraryImport(Library, EntryPoint = "wxk_Sizer_AddWindow")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SizerAddWindow(WxkSizer* sizer, WxkWindow* window, int proportion, int flags, int border);

	[LibraryImport(Library, EntryPoint = "wxk_Sizer_AddSizer")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SizerAddSizer(WxkSizer* sizer, WxkSizer* child, int proportion, int flags, int border);

	[LibraryImport(Library, EntryPoint = "wxk_Sizer_AddSpacer")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SizerAddSpacer(WxkSizer* sizer, int size);

	[LibraryImport(Library, EntryPoint = "wxk_Sizer_AddStretchSpacer")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SizerAddStretchSpacer(WxkSizer* sizer, int proportion);

	[LibraryImport(Library, EntryPoint = "wxk_Sizer_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SizerFree(WxkSizer* sizer);

	// Menus

	[LibraryImport(Library, EntryPoint = "wxk_Menu_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkMenu* MenuNew();

	[LibraryImport(Library, EntryPoint = "wxk_Menu_Append")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkMenuItem* MenuAppend(WxkMenu* menu, int id, char* text, nuint textLength, char* help, nuint helpLength, int kind);

	[LibraryImport(Library, EntryPoint = "wxk_Menu_AppendSeparator")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkMenuItem* MenuAppendSeparator(WxkMenu* menu);

	[LibraryImport(Library, EntryPoint = "wxk_Menu_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void MenuFree(WxkMenu* menu);

	[LibraryImport(Library, EntryPoint = "wxk_MenuItem_GetId")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int MenuItemGetId(WxkMenuItem* item);

	// Timers

	[LibraryImport(Library, EntryPoint = "wxk_Timer_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkTimer* TimerNew();

	[LibraryImport(Library, EntryPoint = "wxk_Timer_Start")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte TimerStart(WxkTimer* timer, int milliseconds, byte oneShot);

	[LibraryImport(Library, EntryPoint = "wxk_Timer_Stop")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void TimerStop(WxkTimer* timer);

	[LibraryImport(Library, EntryPoint = "wxk_Timer_IsRunning")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte TimerIsRunning(WxkTimer* timer);

	[LibraryImport(Library, EntryPoint = "wxk_Timer_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void TimerFree(WxkTimer* timer);
}
