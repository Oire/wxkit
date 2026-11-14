// The flat C interface over wxWidgets that WxKit calls. Every function is safe to call from C:
// none lets a C++ exception escape, and only C types cross the boundary.
#pragma once

#include <stddef.h>
#include <stdint.h>

#if defined(WXK_BUILDING_SHARED) && defined(_WIN32)
	#define WXK_API __declspec(dllexport)
#elif defined(WXK_BUILDING_SHARED)
	#define WXK_API __attribute__((visibility("default")))
#else
	#define WXK_API
#endif

#ifdef __cplusplus
	#define WXK_EXTERN extern "C"
	#define WXK_NOEXCEPT noexcept
#else
	#define WXK_EXTERN
	#define WXK_NOEXCEPT
#endif

#define WXK_FUNCTION(type) WXK_EXTERN WXK_API type

// Every handle points at the start of the wxWidgets object. wxObject, wxEvtHandler, and wxWindow
// form a chain of primary bases, so the same address is a valid handle to any of the classes an
// object derives from: a frame's handle is also a window's and an event handler's.
typedef struct wxk_Object wxk_Object;
typedef struct wxk_EvtHandler wxk_EvtHandler;
typedef struct wxk_Window wxk_Window;
typedef struct wxk_Sizer wxk_Sizer;
typedef struct wxk_Menu wxk_Menu;
typedef struct wxk_MenuItem wxk_MenuItem;
typedef struct wxk_Timer wxk_Timer;
typedef struct wxk_Event wxk_Event;
typedef struct wxk_ClassInfo wxk_ClassInfo;

// Text that the shim hands out, which the caller frees with wxk_String_Free.
typedef struct wxk_String wxk_String;

// A wxArrayString, which the caller frees with wxk_StringArray_Free.
typedef struct wxk_StringArray wxk_StringArray;

// A list of windows, such as a window's children, which the caller frees with wxk_WindowList_Free.
typedef struct wxk_WindowList wxk_WindowList;

typedef struct wxk_Point {
	int32_t x;
	int32_t y;
} wxk_Point;

typedef struct wxk_Size {
	int32_t width;
	int32_t height;
} wxk_Size;

typedef struct wxk_Rect {
	int32_t x;
	int32_t y;
	int32_t width;
	int32_t height;
} wxk_Rect;

// A wxColour's channels, and whether it is set: wxNullColour has valid 0.
typedef struct wxk_Colour {
	uint8_t red;
	uint8_t green;
	uint8_t blue;
	uint8_t alpha;
	uint8_t valid;
} wxk_Colour;

typedef uint8_t (*wxk_AppInitCallback)(void* userdata);
typedef void (*wxk_Callback)(void* userdata);
typedef void (*wxk_EventCallback)(void* userdata, wxk_Event* event);

// Application

// Runs the application: initializes wxWidgets, calls onInit, and runs the event loop until it
// ends. Returns the exit code, or -1 when wxWidgets could not start.
WXK_FUNCTION(int32_t) wxk_App_Run(wxk_AppInitCallback onInit, void* userdata) WXK_NOEXCEPT;

// Queues callback to run on the GUI thread. Safe from any thread. Returns 0 when no application is running.
WXK_FUNCTION(uint8_t) wxk_App_CallAfter(wxk_Callback callback, void* userdata) WXK_NOEXCEPT;

WXK_FUNCTION(void) wxk_App_ExitMainLoop(void) WXK_NOEXCEPT;

// The wxWidgets version, such as "wxWidgets 3.3.4".
WXK_FUNCTION(wxk_String*) wxk_Version_GetString(void) WXK_NOEXCEPT;

// Text

WXK_FUNCTION(const char16_t*) wxk_String_GetData(const wxk_String* text, size_t* length) WXK_NOEXCEPT;

WXK_FUNCTION(void) wxk_String_Free(wxk_String* text) WXK_NOEXCEPT;

WXK_FUNCTION(wxk_StringArray*) wxk_StringArray_new(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_StringArray_Add(wxk_StringArray* array, const char16_t* text, size_t textLength) WXK_NOEXCEPT;
WXK_FUNCTION(size_t) wxk_StringArray_GetCount(const wxk_StringArray* array) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_String*) wxk_StringArray_Item(const wxk_StringArray* array, size_t index) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_StringArray_Free(wxk_StringArray* array) WXK_NOEXCEPT;

WXK_FUNCTION(size_t) wxk_WindowList_GetCount(const wxk_WindowList* list) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_Window*) wxk_WindowList_Item(const wxk_WindowList* list, size_t index) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_WindowList_Free(wxk_WindowList* list) WXK_NOEXCEPT;

// Objects

// Calls onDestroy with userdata when wxWidgets destroys the handler, after its own destructor has
// run and every event handler bound to it has been released.
WXK_FUNCTION(uint8_t) wxk_EvtHandler_Track(wxk_EvtHandler* handler, wxk_Callback onDestroy, void* userdata) WXK_NOEXCEPT;

WXK_FUNCTION(const wxk_ClassInfo*) wxk_Object_GetClassInfo(wxk_Object* object) WXK_NOEXCEPT;

// Deletes an object the shim copied out, such as a font or a bitmap, which releases its reference.
WXK_FUNCTION(void) wxk_Object_Free(wxk_Object* object) WXK_NOEXCEPT;

WXK_FUNCTION(wxk_String*) wxk_ClassInfo_GetClassName(const wxk_ClassInfo* info) WXK_NOEXCEPT;

// The first base class, or null for wxObject.
WXK_FUNCTION(const wxk_ClassInfo*) wxk_ClassInfo_GetBaseClass(const wxk_ClassInfo* info) WXK_NOEXCEPT;

// The clipboard's text. Each call opens the clipboard and closes it again.

WXK_FUNCTION(uint8_t) wxk_Clipboard_SetText(const char16_t* text, size_t textLength) WXK_NOEXCEPT;
// Null when the clipboard holds no text.
WXK_FUNCTION(wxk_String*) wxk_Clipboard_GetText(void) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_Clipboard_HasText(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Clipboard_Clear(void) WXK_NOEXCEPT;

// Events

// Binds callback to events of eventType from ids in [id, lastId], where -1 means any. When
// wxWidgets releases the binding, which it does when the handler is destroyed, it calls onFree.
WXK_FUNCTION(uint8_t) wxk_EvtHandler_Bind(wxk_EvtHandler* handler, int32_t eventType, int32_t id, int32_t lastId, wxk_EventCallback callback, void* userdata, wxk_Callback onFree) WXK_NOEXCEPT;

WXK_FUNCTION(int32_t) wxk_Event_GetEventType(wxk_Event* event) WXK_NOEXCEPT;

WXK_FUNCTION(void) wxk_CloseEvent_Veto(wxk_Event* event, uint8_t veto) WXK_NOEXCEPT;

// Windows

// The platform's own handle: an HWND on Windows, an NSView* on macOS, and a GtkWidget* on Linux.
WXK_FUNCTION(void*) wxk_Window_GetHandle(wxk_Window* window) WXK_NOEXCEPT;
// Sets the window's sizer, deleting the one it had.
WXK_FUNCTION(void) wxk_Window_SetSizer(wxk_Window* window, wxk_Sizer* sizer) WXK_NOEXCEPT;

WXK_FUNCTION(void) wxk_Frame_SetMenuBar(wxk_Window* frame, wxk_Window* menuBar) WXK_NOEXCEPT;

// Dialogs

// Sizers

// A negative border means the platform's default border.
WXK_FUNCTION(void) wxk_Sizer_AddWindow(wxk_Sizer* sizer, wxk_Window* window, int32_t proportion, int32_t flags, int32_t border) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Sizer_AddSizer(wxk_Sizer* sizer, wxk_Sizer* child, int32_t proportion, int32_t flags, int32_t border) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Sizer_AddSpacer(wxk_Sizer* sizer, int32_t size) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Sizer_AddStretchSpacer(wxk_Sizer* sizer, int32_t proportion) WXK_NOEXCEPT;
// Deletes a sizer that no window or other sizer owns.
WXK_FUNCTION(void) wxk_Sizer_Free(wxk_Sizer* sizer) WXK_NOEXCEPT;

// Menus

WXK_FUNCTION(wxk_Menu*) wxk_Menu_new(void) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_MenuItem*) wxk_Menu_Append(wxk_Menu* menu, int32_t id, const char16_t* text, size_t textLength, const char16_t* help, size_t helpLength, int32_t kind) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_MenuItem*) wxk_Menu_AppendSeparator(wxk_Menu* menu) WXK_NOEXCEPT;
// Deletes a menu that no menu bar or other menu owns.
WXK_FUNCTION(void) wxk_Menu_Free(wxk_Menu* menu) WXK_NOEXCEPT;

WXK_FUNCTION(int32_t) wxk_MenuItem_GetId(wxk_MenuItem* item) WXK_NOEXCEPT;

// Timers

// A timer that sends its events to itself.
WXK_FUNCTION(wxk_Timer*) wxk_Timer_new(void) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_Timer_Start(wxk_Timer* timer, int32_t milliseconds, uint8_t oneShot) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Timer_Stop(wxk_Timer* timer) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_Timer_IsRunning(wxk_Timer* timer) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Timer_Free(wxk_Timer* timer) WXK_NOEXCEPT;

// The functions generated from the wxWidgets interface headers.
#include "wxkit_generated.h"
