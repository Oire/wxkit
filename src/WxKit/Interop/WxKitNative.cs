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

	[LibraryImport(Library, EntryPoint = "wxk_String_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* StringNew(char* text, nuint textLength);

	[LibraryImport(Library, EntryPoint = "wxk_StringArray_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkStringArray* StringArrayNew();

	[LibraryImport(Library, EntryPoint = "wxk_StringArray_Add")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void StringArrayAdd(WxkStringArray* array, char* text, nuint textLength);

	[LibraryImport(Library, EntryPoint = "wxk_StringArray_GetCount")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial nuint StringArrayGetCount(WxkStringArray* array);

	[LibraryImport(Library, EntryPoint = "wxk_StringArray_Item")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* StringArrayItem(WxkStringArray* array, nuint index);

	[LibraryImport(Library, EntryPoint = "wxk_StringArray_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void StringArrayFree(WxkStringArray* array);

	[LibraryImport(Library, EntryPoint = "wxk_WindowList_GetCount")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial nuint WindowListGetCount(WxkWindowList* list);

	[LibraryImport(Library, EntryPoint = "wxk_WindowList_Item")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* WindowListItem(WxkWindowList* list, nuint index);

	[LibraryImport(Library, EntryPoint = "wxk_WindowList_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void WindowListFree(WxkWindowList* list);

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_SetText")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte ClipboardSetText(char* text, nuint textLength);

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_GetText")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* ClipboardGetText();

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_HasText")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte ClipboardHasText();

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_Clear")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ClipboardClear();

	[LibraryImport(Library, EntryPoint = "wxk_ClipboardContent_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkClipboardContent* ClipboardContentNew();

	[LibraryImport(Library, EntryPoint = "wxk_ClipboardContent_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ClipboardContentFree(WxkClipboardContent* content);

	[LibraryImport(Library, EntryPoint = "wxk_ClipboardContent_AddText")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ClipboardContentAddText(WxkClipboardContent* content, char* text, nuint textLength);

	[LibraryImport(Library, EntryPoint = "wxk_ClipboardContent_AddHtml")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ClipboardContentAddHtml(WxkClipboardContent* content, char* html, nuint htmlLength);

	[LibraryImport(Library, EntryPoint = "wxk_ClipboardContent_AddBitmap")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ClipboardContentAddBitmap(WxkClipboardContent* content, WxkObject* bitmap);

	[LibraryImport(Library, EntryPoint = "wxk_ClipboardContent_AddFiles")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ClipboardContentAddFiles(WxkClipboardContent* content, WxkStringArray* files);

	[LibraryImport(Library, EntryPoint = "wxk_ClipboardContent_AddData")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ClipboardContentAddData(WxkClipboardContent* content, char* format, nuint formatLength, byte* data, nuint size);

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_SetContent")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte ClipboardSetContent(WxkClipboardContent* content);

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_Has")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte ClipboardHas(WxkClipboardFormat format);

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_HasData")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte ClipboardHasData(char* format, nuint formatLength);

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_GetHtml")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* ClipboardGetHtml();

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_GetBitmap")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkObject* ClipboardGetBitmap();

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_GetFiles")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkStringArray* ClipboardGetFiles();

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_GetData")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte* ClipboardGetData(char* format, nuint formatLength, nuint* size);

	[LibraryImport(Library, EntryPoint = "wxk_Bytes_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void BytesFree(byte* bytes);

	[LibraryImport(Library, EntryPoint = "wxk_Clipboard_Flush")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte ClipboardFlush();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddPngHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddPngHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddJpegHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddJpegHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddTiffHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddTiffHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddGifHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddGifHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddWebpHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddWebpHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddPnmHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddPnmHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddPcxHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddPcxHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddIffHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddIffHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddTgaHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddTgaHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddXpmHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddXpmHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddIconHandler")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddIconHandler();

	[LibraryImport(Library, EntryPoint = "wxk_Image_AddAllHandlers")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ImageAddAllHandlers();

	[LibraryImport(Library, EntryPoint = "wxk_Accessible_SetCallbacks")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void AccessibleSetCallbacks(WxkAccessibleCallbacks* callbacks);

	[LibraryImport(Library, EntryPoint = "wxk_ManagedAccessible_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkAccessible* ManagedAccessibleNew(WxkWindow* window, void* userdata);

	[LibraryImport(Library, EntryPoint = "wxk_Accessible_NotifyEvent")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void AccessibleNotifyEvent(int eventType, WxkWindow* window, int objectType, int objectId);

	[LibraryImport(Library, EntryPoint = "wxk_ManagedValidator_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkObject* ManagedValidatorNew(delegate* unmanaged[Cdecl]<void*, WxkWindow*, WxkWindow*, byte> validate, delegate* unmanaged[Cdecl]<void*, WxkWindow*, byte> toWindow, delegate* unmanaged[Cdecl]<void*, WxkWindow*, byte> fromWindow, delegate* unmanaged[Cdecl]<void*, void> retain, delegate* unmanaged[Cdecl]<void*, void> release, delegate* unmanaged[Cdecl]<void*, void> onFree, void* userdata);

	[LibraryImport(Library, EntryPoint = "wxk_Window_SetValidator")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void WindowSetValidator(WxkWindow* window, WxkObject* validator);

	// Objects

	[LibraryImport(Library, EntryPoint = "wxk_EvtHandler_Track")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte EvtHandlerTrack(WxkEvtHandler* handler, delegate* unmanaged[Cdecl]<void*, void> onDestroy, void* userdata);

	[LibraryImport(Library, EntryPoint = "wxk_Object_GetClassInfo")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkClassInfo* ObjectGetClassInfo(WxkObject* obj);

	[LibraryImport(Library, EntryPoint = "wxk_Object_Free")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void ObjectFree(WxkObject* obj);

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
