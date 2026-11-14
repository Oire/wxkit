#include "shim.h"

WXK_FUNCTION(wxk_Window*) wxk_Frame_new(wxk_Window* parent, int32_t id, const char16_t* title, size_t titleLength) noexcept {
	try {
		return wxk::FromWindow(new wxFrame(wxk::AsWindow(parent), id, wxk::FromUtf16(title, titleLength)));
	} catch (...) {
		return nullptr;
	}
}

WXK_FUNCTION(uint8_t) wxk_Window_Show(wxk_Window* window, uint8_t show) noexcept {
	try {
		return wxk::AsWindow(window)->Show(show != 0) ? 1 : 0;
	} catch (...) {
		return 0;
	}
}

WXK_FUNCTION(uint8_t) wxk_Window_Close(wxk_Window* window, uint8_t force) noexcept {
	try {
		return wxk::AsWindow(window)->Close(force != 0) ? 1 : 0;
	} catch (...) {
		return 0;
	}
}
