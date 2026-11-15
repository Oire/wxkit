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
typedef struct wxk_DataViewColumn wxk_DataViewColumn;
typedef struct wxk_ToolBarTool wxk_ToolBarTool;
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

// A wxVariant, which holds one value of any type, and which the caller frees with wxk_Variant_Free.
typedef struct wxk_Variant wxk_Variant;

// A wxVector<wxVariant>, such as a data view list row's values, freed with wxk_VariantList_Free.
typedef struct wxk_VariantList wxk_VariantList;

// The types of value a wxVariant crosses with. Any other type reads as text.
typedef enum wxk_VariantType {
	WXK_VARIANT_NULL = 0,
	WXK_VARIANT_STRING = 1,
	WXK_VARIANT_BOOL = 2,
	WXK_VARIANT_LONG = 3,
	WXK_VARIANT_DOUBLE = 4,
	WXK_VARIANT_BITMAP = 5,
	WXK_VARIANT_ICON = 6,
	WXK_VARIANT_ICON_TEXT = 7
} wxk_VariantType;

// A wxDataViewModel, which is reference counted.
typedef struct wxk_DataViewModel wxk_DataViewModel;

// The wxDataViewItemArray a model fills with an item's children.
typedef struct wxk_DataViewItems wxk_DataViewItems;

// The callbacks of every model whose items come from .NET. An item is the id .NET gave it.
typedef struct wxk_DataViewModelCallbacks {
	void (*getValue)(void* userdata, void* item, uint32_t column, wxk_Variant* value);
	uint8_t (*setValue)(void* userdata, const wxk_Variant* value, void* item, uint32_t column);
	void* (*getParent)(void* userdata, void* item);
	uint8_t (*isContainer)(void* userdata, void* item);
	void (*getChildren)(void* userdata, void* item, wxk_DataViewItems* children);
	uint8_t (*isEnabled)(void* userdata, void* item, uint32_t column);
	uint8_t (*hasContainerColumns)(void* userdata, void* item);
	void (*onFree)(void* userdata);
} wxk_DataViewModelCallbacks;

// The callbacks of every virtual list model whose rows come from .NET.
typedef struct wxk_VirtualListModelCallbacks {
	void (*getValue)(void* userdata, uint32_t row, uint32_t column, wxk_Variant* value);
	uint8_t (*setValue)(void* userdata, const wxk_Variant* value, uint32_t row, uint32_t column);
	uint8_t (*isEnabled)(void* userdata, uint32_t row, uint32_t column);
	void (*onFree)(void* userdata);
} wxk_VirtualListModelCallbacks;

// Clipboard content in several formats, which wxk_Clipboard_SetContent takes ownership of.
typedef struct wxk_ClipboardContent wxk_ClipboardContent;

// The standard clipboard formats.
typedef enum wxk_ClipboardFormat {
	WXK_CLIPBOARD_TEXT = 0,
	WXK_CLIPBOARD_BITMAP = 1,
	WXK_CLIPBOARD_FILES = 2,
	WXK_CLIPBOARD_HTML = 3
} wxk_ClipboardFormat;

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
typedef uint8_t (*wxk_ValidateCallback)(void* userdata, wxk_Window* window, wxk_Window* parent);
typedef uint8_t (*wxk_TransferCallback)(void* userdata, wxk_Window* window);
typedef void (*wxk_Callback)(void* userdata);
typedef void (*wxk_EventCallback)(void* userdata, wxk_Event* event);

// Application

// Runs the application: initializes wxWidgets, calls onInit, and runs the event loop until it
// ends. Returns the exit code, or -1 when wxWidgets could not start.
WXK_FUNCTION(int32_t) wxk_App_Run(wxk_AppInitCallback onInit, void* userdata) WXK_NOEXCEPT;
// Whether the event loop can run on the calling thread, which on macOS has to be the main thread.
WXK_FUNCTION(uint8_t) wxk_App_CanRunOnThisThread(void) WXK_NOEXCEPT;

// Queues callback to run on the GUI thread. Safe from any thread. Returns 0 when no application is running.
WXK_FUNCTION(uint8_t) wxk_App_CallAfter(wxk_Callback callback, void* userdata) WXK_NOEXCEPT;

WXK_FUNCTION(void) wxk_App_ExitMainLoop(void) WXK_NOEXCEPT;

// The wxWidgets version, such as "wxWidgets 3.3.4".
WXK_FUNCTION(wxk_String*) wxk_Version_GetString(void) WXK_NOEXCEPT;

// Text

WXK_FUNCTION(const char16_t*) wxk_String_GetData(const wxk_String* text, size_t* length) WXK_NOEXCEPT;

WXK_FUNCTION(void) wxk_String_Free(wxk_String* text) WXK_NOEXCEPT;

// Text that .NET hands to the shim, such as what a director's callback returns. The shim frees it.
WXK_FUNCTION(wxk_String*) wxk_String_new(const char16_t* text, size_t textLength) WXK_NOEXCEPT;

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

// The clipboard. Each call opens the clipboard and closes it again. The getters return null when
// the clipboard does not have their format.

WXK_FUNCTION(uint8_t) wxk_Clipboard_SetText(const char16_t* text, size_t textLength) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_String*) wxk_Clipboard_GetText(void) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_Clipboard_HasText(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Clipboard_Clear(void) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_ClipboardContent*) wxk_ClipboardContent_new(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_ClipboardContent_Free(wxk_ClipboardContent* content) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_ClipboardContent_AddText(wxk_ClipboardContent* content, const char16_t* text, size_t textLength) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_ClipboardContent_AddHtml(wxk_ClipboardContent* content, const char16_t* html, size_t htmlLength) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_ClipboardContent_AddBitmap(wxk_ClipboardContent* content, wxk_Object* bitmap) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_ClipboardContent_AddFiles(wxk_ClipboardContent* content, const wxk_StringArray* files) WXK_NOEXCEPT;
// Data in a format the application names, such as "MyApp.Shape", which the system registers.
WXK_FUNCTION(void) wxk_ClipboardContent_AddData(wxk_ClipboardContent* content, const char16_t* format, size_t formatLength, const uint8_t* data, size_t size) WXK_NOEXCEPT;
// Takes ownership of the content, whether or not it succeeds.
WXK_FUNCTION(uint8_t) wxk_Clipboard_SetContent(wxk_ClipboardContent* content) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_Clipboard_Has(wxk_ClipboardFormat format) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_Clipboard_HasData(const char16_t* format, size_t formatLength) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_String*) wxk_Clipboard_GetHtml(void) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_Object*) wxk_Clipboard_GetBitmap(void) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_StringArray*) wxk_Clipboard_GetFiles(void) WXK_NOEXCEPT;
// The caller frees the data with wxk_Bytes_Free.
WXK_FUNCTION(uint8_t*) wxk_Clipboard_GetData(const char16_t* format, size_t formatLength, size_t* size) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Bytes_Free(uint8_t* bytes) WXK_NOEXCEPT;
// Keeps the clipboard's content after the application exits.
WXK_FUNCTION(uint8_t) wxk_Clipboard_Flush(void) WXK_NOEXCEPT;

// Image file formats. Each is in a file of its own, so a statically linked application only
// links the formats it adds. BMP is always there.

WXK_FUNCTION(void) wxk_Image_AddPngHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddJpegHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddTiffHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddGifHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddWebpHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddPnmHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddPcxHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddIffHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddTgaHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddXpmHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddIconHandler(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Image_AddAllHandlers(void) WXK_NOEXCEPT;

// Variants

WXK_FUNCTION(wxk_Variant*) wxk_Variant_new(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Variant_Free(wxk_Variant* variant) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Variant_SetNull(wxk_Variant* variant) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Variant_SetString(wxk_Variant* variant, const char16_t* text, size_t textLength) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Variant_SetBool(wxk_Variant* variant, uint8_t value) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Variant_SetLong(wxk_Variant* variant, int64_t value) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Variant_SetDouble(wxk_Variant* variant, double value) WXK_NOEXCEPT;
// Copies a bitmap, an icon, or a wxDataViewIconText into the variant. Returns 0 for anything else.
WXK_FUNCTION(uint8_t) wxk_Variant_SetObject(wxk_Variant* variant, wxk_Object* value) WXK_NOEXCEPT;
// A wxk_VariantType.
WXK_FUNCTION(int32_t) wxk_Variant_GetType(const wxk_Variant* variant) WXK_NOEXCEPT;
// The value as text, which works for any type.
WXK_FUNCTION(wxk_String*) wxk_Variant_GetString(const wxk_Variant* variant) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_Variant_GetBool(const wxk_Variant* variant) WXK_NOEXCEPT;
WXK_FUNCTION(int64_t) wxk_Variant_GetLong(const wxk_Variant* variant) WXK_NOEXCEPT;
WXK_FUNCTION(double) wxk_Variant_GetDouble(const wxk_Variant* variant) WXK_NOEXCEPT;
// A heap copy of the bitmap, icon, or icon text the variant holds, which the caller owns.
WXK_FUNCTION(wxk_Object*) wxk_Variant_GetObject(const wxk_Variant* variant) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_VariantList*) wxk_VariantList_new(void) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VariantList_Add(wxk_VariantList* list, const wxk_Variant* value) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VariantList_Free(wxk_VariantList* list) WXK_NOEXCEPT;

// Data view models

WXK_FUNCTION(void) wxk_DataViewModel_SetCallbacks(const wxk_DataViewModelCallbacks* callbacks) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VirtualListModel_SetCallbacks(const wxk_VirtualListModelCallbacks* callbacks) WXK_NOEXCEPT;
// A model with one reference, which the caller releases with wxk_DataViewModel_DecRef.
WXK_FUNCTION(wxk_DataViewModel*) wxk_ManagedDataViewModel_new(void* userdata) WXK_NOEXCEPT;
WXK_FUNCTION(wxk_DataViewModel*) wxk_ManagedVirtualListModel_new(void* userdata, uint32_t count) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_DataViewModel_DecRef(wxk_DataViewModel* model) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_DataViewItems_Add(wxk_DataViewItems* items, void* item) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_DataViewModel_ItemAdded(wxk_DataViewModel* model, void* parent, void* item) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_DataViewModel_ItemDeleted(wxk_DataViewModel* model, void* parent, void* item) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_DataViewModel_ItemChanged(wxk_DataViewModel* model, void* item) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_DataViewModel_ValueChanged(wxk_DataViewModel* model, void* item, uint32_t column) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_DataViewModel_Cleared(wxk_DataViewModel* model) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_DataViewModel_Resort(wxk_DataViewModel* model) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VirtualListModel_Reset(wxk_DataViewModel* model, uint32_t count) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VirtualListModel_RowAppended(wxk_DataViewModel* model) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VirtualListModel_RowPrepended(wxk_DataViewModel* model) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VirtualListModel_RowInserted(wxk_DataViewModel* model, uint32_t before) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VirtualListModel_RowDeleted(wxk_DataViewModel* model, uint32_t row) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VirtualListModel_RowChanged(wxk_DataViewModel* model, uint32_t row) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_VirtualListModel_RowValueChanged(wxk_DataViewModel* model, uint32_t row, uint32_t column) WXK_NOEXCEPT;
WXK_FUNCTION(uint32_t) wxk_VirtualListModel_GetCount(wxk_DataViewModel* model) WXK_NOEXCEPT;
WXK_FUNCTION(void*) wxk_VirtualListModel_GetItem(wxk_DataViewModel* model, uint32_t row) WXK_NOEXCEPT;
WXK_FUNCTION(uint32_t) wxk_VirtualListModel_GetRow(wxk_DataViewModel* model, void* item) WXK_NOEXCEPT;
// Shows the model in the control, which takes a reference to it.
// Delete one or all of a data view's columns. The managed side marks their objects destroyed.
WXK_FUNCTION(uint8_t) wxk_DataViewCtrl_DeleteColumn(wxk_Window* control, wxk_DataViewColumn* column) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_DataViewCtrl_ClearColumns(wxk_Window* control) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_DataViewCtrl_AssociateModel(wxk_Window* control, wxk_DataViewModel* model) WXK_NOEXCEPT;

// Accessibility

// An accessible object, which describes a window to screen readers. The window owns it.
typedef struct wxk_Accessible wxk_Accessible;

// Which text an accessible object's getText callback is asked for.
typedef enum wxk_AccessibleText {
	WXK_ACCESSIBLE_NAME = 0,
	WXK_ACCESSIBLE_DESCRIPTION = 1,
	WXK_ACCESSIBLE_HELP_TEXT = 2,
	WXK_ACCESSIBLE_KEYBOARD_SHORTCUT = 3,
	WXK_ACCESSIBLE_DEFAULT_ACTION = 4,
	WXK_ACCESSIBLE_VALUE = 5
} wxk_AccessibleText;

// Which element its getElement callback is asked for: a child, the focus, the element at a point,
// or the one in a direction from another.
typedef enum wxk_AccessibleElement {
	WXK_ACCESSIBLE_CHILD = 0,
	WXK_ACCESSIBLE_FOCUS = 1,
	WXK_ACCESSIBLE_HIT_TEST = 2,
	WXK_ACCESSIBLE_NAVIGATE = 3
} wxk_AccessibleElement;

// What its act callback is asked to do.
typedef enum wxk_AccessibleAction {
	WXK_ACCESSIBLE_DO_DEFAULT_ACTION = 0,
	WXK_ACCESSIBLE_SELECT = 1
} wxk_AccessibleAction;

// The callbacks every managed accessible object calls, which return a wxAccStatus. A callback
// that returns wxACC_OK has filled in its result.
typedef struct wxk_AccessibleCallbacks {
	int32_t (*getText)(void* userdata, int32_t which, int32_t childId, wxk_String** text);
	int32_t (*getRole)(void* userdata, int32_t childId, int32_t* role);
	int32_t (*getState)(void* userdata, int32_t childId, int64_t* state);
	int32_t (*getChildCount)(void* userdata, int32_t* count);
	int32_t (*getLocation)(void* userdata, int32_t elementId, wxk_Rect* rect);
	int32_t (*getElement)(void* userdata, int32_t which, int32_t childId, int32_t direction, wxk_Point point, int32_t* resultId, wxk_Accessible** resultObject);
	int32_t (*act)(void* userdata, int32_t which, int32_t childId, int32_t flags);
	void (*onFree)(void* userdata);
} wxk_AccessibleCallbacks;

WXK_FUNCTION(void) wxk_Accessible_SetCallbacks(const wxk_AccessibleCallbacks* callbacks) WXK_NOEXCEPT;
// Gives the window an accessible object whose answers come from .NET, replacing the one it had.
WXK_FUNCTION(wxk_Accessible*) wxk_ManagedAccessible_new(wxk_Window* window, void* userdata) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_Accessible_NotifyEvent(int32_t eventType, wxk_Window* window, int32_t objectType, int32_t objectId) WXK_NOEXCEPT;

// The About box

// What an About box shows. Each string is the shim's to free, and a null one, or a null list,
// is left out.
typedef struct wxk_AboutInfo {
	wxk_String* name;
	wxk_String* version;
	wxk_String* longVersion;
	wxk_String* description;
	wxk_String* copyright;
	wxk_String* license;
	wxk_String* webSite;
	wxk_String* webSiteDescription;
	const wxk_StringArray* developers;
	const wxk_StringArray* docWriters;
	const wxk_StringArray* artists;
	const wxk_StringArray* translators;
	const wxk_Object* icon;
} wxk_AboutInfo;

// Shows the platform's About box, or wxWidgets's own when the platform's cannot show everything.
WXK_FUNCTION(void) wxk_AboutBox_Show(const wxk_AboutInfo* about, wxk_Window* parent) WXK_NOEXCEPT;

// Notifications

// Deletes a notification, which hides it if it is showing.
WXK_FUNCTION(void) wxk_NotificationMessage_Free(wxk_EvtHandler* notification) WXK_NOEXCEPT;

// Toolbar tools

WXK_FUNCTION(wxk_Window*) wxk_ToolBarTool_GetToolBar(wxk_ToolBarTool* tool) WXK_NOEXCEPT;
// Delete one or all of a toolbar's tools. The managed side marks their objects destroyed.
WXK_FUNCTION(uint8_t) wxk_ToolBar_DeleteTool(wxk_Window* toolBar, int32_t toolId) WXK_NOEXCEPT;
WXK_FUNCTION(uint8_t) wxk_ToolBar_DeleteToolByPos(wxk_Window* toolBar, size_t position) WXK_NOEXCEPT;
WXK_FUNCTION(void) wxk_ToolBar_ClearTools(wxk_Window* toolBar) WXK_NOEXCEPT;

// Item data

// Sets the function that frees the GCHandle of a .NET object a control kept with an item, which
// is called when the control deletes the item.
WXK_FUNCTION(void) wxk_ClientData_SetFree(void (*free)(void* handle)) WXK_NOEXCEPT;
// Gives a tree item the .NET object, or none when the handle is null, deleting the data it had,
// which wxTreeCtrl::SetItemData does not.
WXK_FUNCTION(void) wxk_TreeCtrl_SetItemData(wxk_Window* tree, void* item, void* handle) WXK_NOEXCEPT;

// Validators

// A validator whose checks call back into .NET. retain and release are called as windows take
// and drop copies of it, and onFree once the last copy is gone.
WXK_FUNCTION(wxk_Object*) wxk_ManagedValidator_new(wxk_ValidateCallback validate, wxk_TransferCallback toWindow, wxk_TransferCallback fromWindow, wxk_Callback retain, wxk_Callback release, wxk_Callback onFree, void* userdata) WXK_NOEXCEPT;
// Gives the window a copy of the validator. Null removes the window's validator.
WXK_FUNCTION(void) wxk_Window_SetValidator(wxk_Window* window, wxk_Object* validator) WXK_NOEXCEPT;

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
