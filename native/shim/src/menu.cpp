#include "shim.h"

using wxk::Guard;
using wxk::Unwrap;

WXK_FUNCTION(wxk_Window*) wxk_MenuBar_new(int64_t style) noexcept {
	return Guard<wxk_Window*>(nullptr, [&] {
		wxWindow* menuBar = new wxMenuBar(static_cast<long>(style));
		return wxk::Wrap(menuBar);
	});
}

WXK_FUNCTION(uint8_t) wxk_MenuBar_Append(wxk_Window* menuBar, wxk_Menu* menu, const char16_t* title, size_t titleLength) noexcept {
	return Guard<uint8_t>(0, [&] { return wxk::As<wxMenuBar>(menuBar)->Append(Unwrap(menu), wxk::FromUtf16(title, titleLength)) ? 1 : 0; });
}

WXK_FUNCTION(wxk_Menu*) wxk_Menu_new(void) noexcept {
	return Guard<wxk_Menu*>(nullptr, [] { return wxk::Wrap(new wxMenu()); });
}

WXK_FUNCTION(wxk_MenuItem*) wxk_Menu_Append(wxk_Menu* menu, int32_t id, const char16_t* text, size_t textLength, const char16_t* help, size_t helpLength, int32_t kind) noexcept {
	return Guard<wxk_MenuItem*>(nullptr, [&] {
		return wxk::Wrap(Unwrap(menu)->Append(id, wxk::FromUtf16(text, textLength), wxk::FromUtf16(help, helpLength), static_cast<wxItemKind>(kind)));
	});
}

WXK_FUNCTION(wxk_MenuItem*) wxk_Menu_AppendSeparator(wxk_Menu* menu) noexcept {
	return Guard<wxk_MenuItem*>(nullptr, [&] { return wxk::Wrap(Unwrap(menu)->AppendSeparator()); });
}

WXK_FUNCTION(void) wxk_Menu_Free(wxk_Menu* menu) noexcept {
	Guard([&] { delete Unwrap(menu); });
}

WXK_FUNCTION(int32_t) wxk_MenuItem_GetId(wxk_MenuItem* item) noexcept {
	return Unwrap(item)->GetId();
}
