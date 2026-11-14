// Helpers shared by the shim's sources. Not part of the C interface.
#pragma once

#include <wx/wx.h>

#include <string>
#include <type_traits>
#include <utility>

#include "wxkit.h"

struct wxk_String {
	std::u16string text;
};

namespace wxk {

// Every exported function runs its body through Guard, because a C++ exception that reached .NET
// would end the process. A failure returns the fallback, which is the function's "nothing" value.
template <typename R, typename F>
R Guard(R fallback, F&& body) noexcept {
	try {
		return std::forward<F>(body)();
	} catch (...) {
		return fallback;
	}
}

template <typename F>
void Guard(F&& body) noexcept {
	try {
		std::forward<F>(body)();
	} catch (...) {
	}
}

// .NET passes text as UTF-16 with a length, which is what wxString holds on Windows already.
inline wxString FromUtf16(const char16_t* text, size_t length) {
	if (text == nullptr || length == 0) return wxString();
	if constexpr (sizeof(wchar_t) == sizeof(char16_t)) {
		return wxString(reinterpret_cast<const wchar_t*>(text), length);
	} else {
		return wxString(reinterpret_cast<const char*>(text), wxMBConvUTF16(), length * sizeof(char16_t));
	}
}

inline wxk_String* ToUtf16(const wxString& text) {
	auto* result = new wxk_String();
	if constexpr (sizeof(wchar_t) == sizeof(char16_t)) {
		const wchar_t* data = text.wc_str();
		result->text.assign(reinterpret_cast<const char16_t*>(data), text.length());
	} else {
		const wxCharBuffer buffer = text.mb_str(wxMBConvUTF16());
		result->text.assign(reinterpret_cast<const char16_t*>(buffer.data()), buffer.length() / sizeof(char16_t));
	}
	return result;
}

inline wxPoint FromPoint(wxk_Point point) {
	return wxPoint(point.x, point.y);
}

inline wxSize FromSize(wxk_Size size) {
	return wxSize(size.width, size.height);
}

inline wxk_Point ToPoint(const wxPoint& point) {
	return wxk_Point{point.x, point.y};
}

inline wxk_Size ToSize(const wxSize& size) {
	return wxk_Size{size.x, size.y};
}

inline wxRect FromRect(wxk_Rect rect) {
	return wxRect(rect.x, rect.y, rect.width, rect.height);
}

inline wxk_Rect ToRect(const wxRect& rect) {
	return wxk_Rect{rect.x, rect.y, rect.width, rect.height};
}

inline wxColour FromColour(wxk_Colour colour) {
	return colour.valid != 0 ? wxColour(colour.red, colour.green, colour.blue, colour.alpha) : wxNullColour;
}

inline wxk_Colour ToColour(const wxColour& colour) {
	if (!colour.IsOk()) return wxk_Colour{0, 0, 0, 0, 0};
	return wxk_Colour{colour.Red(), colour.Green(), colour.Blue(), colour.Alpha(), 1};
}

// Each handle type converts to and from the wxWidgets class it names. A pointer to a derived
// class is first converted to that class, so the handle always points where the C interface says.
#define WXK_HANDLE(handle, type) \
	inline type* Unwrap(handle* value) { return reinterpret_cast<type*>(value); } \
	inline handle* Wrap(type* value) { return reinterpret_cast<handle*>(value); }

WXK_HANDLE(wxk_Object, wxObject)
WXK_HANDLE(wxk_EvtHandler, wxEvtHandler)
WXK_HANDLE(wxk_Window, wxWindow)
WXK_HANDLE(wxk_Sizer, wxSizer)
WXK_HANDLE(wxk_Menu, wxMenu)
WXK_HANDLE(wxk_MenuItem, wxMenuItem)
WXK_HANDLE(wxk_Timer, wxTimer)
WXK_HANDLE(wxk_Event, wxEvent)
WXK_HANDLE(wxk_StringArray, wxArrayString)

#undef WXK_HANDLE

inline const wxArrayString* Unwrap(const wxk_StringArray* value) {
	return reinterpret_cast<const wxArrayString*>(value);
}

inline const wxClassInfo* Unwrap(const wxk_ClassInfo* value) {
	return reinterpret_cast<const wxClassInfo*>(value);
}

inline const wxk_ClassInfo* Wrap(const wxClassInfo* value) {
	return reinterpret_cast<const wxk_ClassInfo*>(value);
}

// A value object, such as a font, crosses as a handle to a heap copy of it. Null stands for the
// class's empty object, as wxNullFont is wxFont().
template <typename T>
const T& ValueOf(const wxk_Object* object) {
	static const T none;
	return object != nullptr ? *static_cast<const T*>(reinterpret_cast<const wxObject*>(object)) : none;
}

// The managed side's own copy of a value object, which a function can change, as a wxMemoryDC draws into its bitmap.
template <typename T>
T& ValueRef(wxk_Object* object) {
	return *static_cast<T*>(reinterpret_cast<wxObject*>(object));
}

// Copies a value object out, which only adds a reference to its data. An empty one is null.
template <typename T>
wxk_Object* CopyValue(const T& value) {
	if (!value.IsOk()) return nullptr;
	wxObject* copy = new T(value);
	return reinterpret_cast<wxk_Object*>(copy);
}

template <typename T>
T* As(wxk_Window* window) {
	return static_cast<T*>(Unwrap(window));
}

template <typename T>
T* As(wxk_Event* event) {
	return static_cast<T*>(Unwrap(event));
}

inline wxk_Window* WrapWindow(wxWindow* window) {
	return Wrap(window);
}

} // namespace wxk
