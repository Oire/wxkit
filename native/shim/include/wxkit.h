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

// Every window handle points at the wxWindow itself. wxWidgets windows derive along a single chain
// of primary bases, so a handle to a frame is also a valid handle to a window.
typedef struct wxk_Window wxk_Window;

// Text that the shim hands out, which the caller frees with wxk_String_Free.
typedef struct wxk_String wxk_String;

typedef uint8_t (*wxk_AppInitCallback)(void* userdata);
typedef void (*wxk_Callback)(void* userdata);

// Runs the application: initializes wxWidgets, calls onInit, and runs the event loop until it
// ends. Returns the exit code, or -1 when wxWidgets could not start.
WXK_FUNCTION(int32_t) wxk_App_Run(wxk_AppInitCallback onInit, void* userdata) WXK_NOEXCEPT;

// Queues callback to run on the GUI thread. Safe from any thread. Returns 0 when no application is running.
WXK_FUNCTION(uint8_t) wxk_App_CallAfter(wxk_Callback callback, void* userdata) WXK_NOEXCEPT;

WXK_FUNCTION(void) wxk_App_ExitMainLoop(void) WXK_NOEXCEPT;

// The wxWidgets version, such as "wxWidgets 3.3.4".
WXK_FUNCTION(wxk_String*) wxk_Version_GetString(void) WXK_NOEXCEPT;

WXK_FUNCTION(const char16_t*) wxk_String_GetData(const wxk_String* text, size_t* length) WXK_NOEXCEPT;

WXK_FUNCTION(void) wxk_String_Free(wxk_String* text) WXK_NOEXCEPT;

WXK_FUNCTION(wxk_Window*) wxk_Frame_new(wxk_Window* parent, int32_t id, const char16_t* title, size_t titleLength) WXK_NOEXCEPT;

WXK_FUNCTION(uint8_t) wxk_Window_Show(wxk_Window* window, uint8_t show) WXK_NOEXCEPT;

WXK_FUNCTION(uint8_t) wxk_Window_Close(wxk_Window* window, uint8_t force) WXK_NOEXCEPT;
