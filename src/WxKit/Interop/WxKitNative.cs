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

	[LibraryImport(Library, EntryPoint = "wxk_EventType_Button")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int EventTypeButton();

	[LibraryImport(Library, EntryPoint = "wxk_EventType_Menu")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int EventTypeMenu();

	[LibraryImport(Library, EntryPoint = "wxk_EventType_Text")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int EventTypeText();

	[LibraryImport(Library, EntryPoint = "wxk_EventType_CloseWindow")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int EventTypeCloseWindow();

	[LibraryImport(Library, EntryPoint = "wxk_EventType_Timer")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int EventTypeTimer();

	[LibraryImport(Library, EntryPoint = "wxk_Event_GetEventType")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int EventGetEventType(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_Event_GetId")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int EventGetId(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_Event_GetEventObject")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkObject* EventGetEventObject(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_Event_Skip")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void EventSkip(WxkEvent* e, byte skip);

	[LibraryImport(Library, EntryPoint = "wxk_Event_GetSkipped")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte EventGetSkipped(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_CommandEvent_GetString")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* CommandEventGetString(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_CommandEvent_GetInt")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int CommandEventGetInt(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_CommandEvent_GetSelection")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int CommandEventGetSelection(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_CommandEvent_IsChecked")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte CommandEventIsChecked(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_CloseEvent_CanVeto")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte CloseEventCanVeto(WxkEvent* e);

	[LibraryImport(Library, EntryPoint = "wxk_CloseEvent_Veto")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void CloseEventVeto(WxkEvent* e, byte veto);

	[LibraryImport(Library, EntryPoint = "wxk_CloseEvent_GetVeto")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte CloseEventGetVeto(WxkEvent* e);

	// Windows

	[LibraryImport(Library, EntryPoint = "wxk_Window_GetId")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int WindowGetId(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_GetParent")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* WindowGetParent(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_GetLabel")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* WindowGetLabel(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_SetLabel")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void WindowSetLabel(WxkWindow* window, char* label, nuint labelLength);

	[LibraryImport(Library, EntryPoint = "wxk_Window_Show")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte WindowShow(WxkWindow* window, byte show);

	[LibraryImport(Library, EntryPoint = "wxk_Window_IsShown")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte WindowIsShown(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_Enable")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte WindowEnable(WxkWindow* window, byte enable);

	[LibraryImport(Library, EntryPoint = "wxk_Window_IsEnabled")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte WindowIsEnabled(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_SetFocus")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void WindowSetFocus(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_Close")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte WindowClose(WxkWindow* window, byte force);

	[LibraryImport(Library, EntryPoint = "wxk_Window_Destroy")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte WindowDestroy(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_SetSizer")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void WindowSetSizer(WxkWindow* window, WxkSizer* sizer);

	[LibraryImport(Library, EntryPoint = "wxk_Window_Fit")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void WindowFit(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_Layout")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte WindowLayout(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_Window_Centre")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void WindowCentre(WxkWindow* window, int direction);

	[LibraryImport(Library, EntryPoint = "wxk_TopLevelWindow_GetTitle")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* TopLevelWindowGetTitle(WxkWindow* window);

	[LibraryImport(Library, EntryPoint = "wxk_TopLevelWindow_SetTitle")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void TopLevelWindowSetTitle(WxkWindow* window, char* title, nuint titleLength);

	[LibraryImport(Library, EntryPoint = "wxk_Frame_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* FrameNew(WxkWindow* parent, int id, char* title, nuint titleLength, Point position, Size size, long style);

	[LibraryImport(Library, EntryPoint = "wxk_Frame_SetMenuBar")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void FrameSetMenuBar(WxkWindow* frame, WxkWindow* menuBar);

	[LibraryImport(Library, EntryPoint = "wxk_Panel_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* PanelNew(WxkWindow* parent, int id, Point position, Size size, long style);

	[LibraryImport(Library, EntryPoint = "wxk_Button_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* ButtonNew(WxkWindow* parent, int id, char* label, nuint labelLength, Point position, Size size, long style);

	[LibraryImport(Library, EntryPoint = "wxk_StaticText_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* StaticTextNew(WxkWindow* parent, int id, char* label, nuint labelLength, Point position, Size size, long style);

	[LibraryImport(Library, EntryPoint = "wxk_TextCtrl_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* TextCtrlNew(WxkWindow* parent, int id, char* value, nuint valueLength, Point position, Size size, long style);

	[LibraryImport(Library, EntryPoint = "wxk_TextCtrl_GetValue")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkString* TextCtrlGetValue(WxkWindow* text);

	[LibraryImport(Library, EntryPoint = "wxk_TextCtrl_SetValue")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void TextCtrlSetValue(WxkWindow* text, char* value, nuint valueLength);

	[LibraryImport(Library, EntryPoint = "wxk_TextCtrl_ChangeValue")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void TextCtrlChangeValue(WxkWindow* text, char* value, nuint valueLength);

	// Dialogs

	[LibraryImport(Library, EntryPoint = "wxk_Dialog_ShowModal")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int DialogShowModal(WxkWindow* dialog);

	[LibraryImport(Library, EntryPoint = "wxk_MessageDialog_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* MessageDialogNew(WxkWindow* parent, char* message, nuint messageLength, char* caption, nuint captionLength, long style);

	// Sizers

	[LibraryImport(Library, EntryPoint = "wxk_BoxSizer_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkSizer* BoxSizerNew(int orientation);

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

	[LibraryImport(Library, EntryPoint = "wxk_MenuBar_new")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial WxkWindow* MenuBarNew(long style);

	[LibraryImport(Library, EntryPoint = "wxk_MenuBar_Append")]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial byte MenuBarAppend(WxkWindow* menuBar, WxkMenu* menu, char* title, nuint titleLength);

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
