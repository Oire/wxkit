// Helpers shared by the shim's sources. Not part of the C interface.
#pragma once

#include <wx/wx.h>
#include <wx/variant.h>
#include <wx/datetime.h>
#include <wx/vector.h>

#include <string>
#include <type_traits>
#include <utility>
#include <vector>

#include "wxkit.h"

class wxDataViewColumn;
class wxToolBarToolBase;
class wxTreeItemData;

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

// Takes a string .NET handed over, which can be null for an empty one, and frees it.
inline wxString TakeString(wxk_String* text) {
	if (text == nullptr) return wxString();
	wxString result = FromUtf16(text->text.data(), text->text.size());
	delete text;
	return result;
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

inline wxDateTime FromDateTime(wxk_DateTime value) {
	if (value.valid == 0) return wxInvalidDateTime;
	return wxDateTime(static_cast<wxDateTime::wxDateTime_t>(value.day), static_cast<wxDateTime::Month>(value.month - 1), value.year, static_cast<wxDateTime::wxDateTime_t>(value.hour), static_cast<wxDateTime::wxDateTime_t>(value.minute), static_cast<wxDateTime::wxDateTime_t>(value.second), static_cast<wxDateTime::wxDateTime_t>(value.millisecond));
}

inline wxk_DateTime ToDateTime(const wxDateTime& value) {
	if (!value.IsValid()) return wxk_DateTime{0, 0, 0, 0, 0, 0, 0, 0};
	const wxDateTime::Tm parts = value.GetTm();
	return wxk_DateTime{parts.year, parts.mon + 1, parts.mday, parts.hour, parts.min, parts.sec, parts.msec, 1};
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
WXK_HANDLE(wxk_Variant, wxVariant)
WXK_HANDLE(wxk_VariantList, wxVector<wxVariant>)
WXK_HANDLE(wxk_EvtHandler, wxEvtHandler)
WXK_HANDLE(wxk_Window, wxWindow)
WXK_HANDLE(wxk_Sizer, wxSizer)
WXK_HANDLE(wxk_DataViewColumn, wxDataViewColumn)
WXK_HANDLE(wxk_ToolBarTool, wxToolBarToolBase)
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

// A long output parameter, which the shim gives as int64_t since a C long is 32 bits on Windows.
// The temporary lives until the call returns, and writes the value back as it goes.
struct OutLong {
	int64_t* target;
	long value = 0;

	operator long*() { return &value; }

	~OutLong() {
		if (target != nullptr) *target = value;
	}
};

// A list of windows the shim hands out, such as a window's children.
struct WindowList {
	std::vector<wxWindow*> windows;
};

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

// Copies a value object out, which only adds a reference to its data. An empty one is null, for
// the classes that can be empty, which are the ones with IsOk.
template <typename T>
wxk_Object* CopyValue(const T& value) {
	if constexpr (requires { value.IsOk(); }) {
		if (!value.IsOk()) return nullptr;
	}
	wxObject* copy = new T(value);
	return reinterpret_cast<wxk_Object*>(copy);
}

// Copies a wxWidgets list of windows, which the shim hands out as a wxk_WindowList.
template <typename List>
wxk_WindowList* ToWindowList(const List& list) {
	auto* copy = new WindowList();
	for (wxWindow* window : list) copy->windows.push_back(window);
	return reinterpret_cast<wxk_WindowList*>(copy);
}

// Objects from .NET that a control keeps with its items, such as a list box's or a tree's, as
// GCHandles. The control deletes the data with its item, which frees the handle. Null is no data.
wxClientData* ToClientData(void* handle);
wxTreeItemData* ToTreeItemData(void* handle);
// The handle of an item's data, or null when it has none or the data is not from .NET.
void* FromClientData(const wxClientData* data);

// The bundles a list of bitmaps makes, as a control's images, such as a tree's, take them.
inline wxVector<wxBitmapBundle> ToBitmapBundles(wxk_Object* const* bitmaps, size_t count) {
	wxVector<wxBitmapBundle> bundles;
	bundles.reserve(count);
	for (size_t index = 0; index < count; index++) bundles.push_back(wxBitmapBundle(*static_cast<wxBitmap*>(Unwrap(bitmaps[index]))));
	return bundles;
}

// Adds an image file format, unless it is already added, so calling twice does nothing.
template <typename Handler>
void AddImageHandler(wxBitmapType type) {
	if (wxImage::FindHandler(type) == nullptr) wxImage::AddHandler(new Handler());
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
