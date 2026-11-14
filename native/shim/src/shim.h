// Helpers shared by the shim's sources. Not part of the C interface.
#pragma once

#include <wx/wx.h>

#include <string>

#include "wxkit.h"

struct wxk_String {
	std::u16string text;
};

namespace wxk {

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

inline wxWindow* AsWindow(wxk_Window* window) {
	return reinterpret_cast<wxWindow*>(window);
}

inline wxk_Window* FromWindow(wxWindow* window) {
	return reinterpret_cast<wxk_Window*>(window);
}

} // namespace wxk
