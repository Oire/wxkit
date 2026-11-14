#include "shim.h"

using wxk::Guard;
using wxk::Unwrap;

WXK_FUNCTION(void*) wxk_Window_GetHandle(wxk_Window* window) noexcept {
	return reinterpret_cast<void*>(Unwrap(window)->GetHandle());
}

WXK_FUNCTION(void) wxk_Window_SetSizer(wxk_Window* window, wxk_Sizer* sizer) noexcept {
	Guard([&] { Unwrap(window)->SetSizer(Unwrap(sizer), true); });
}

WXK_FUNCTION(void) wxk_Frame_SetMenuBar(wxk_Window* frame, wxk_Window* menuBar) noexcept {
	Guard([&] { wxk::As<wxFrame>(frame)->SetMenuBar(wxk::As<wxMenuBar>(menuBar)); });
}
