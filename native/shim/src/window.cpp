#include "shim.h"

using wxk::Guard;
using wxk::Unwrap;

WXK_FUNCTION(int32_t) wxk_Window_GetId(wxk_Window* window) noexcept {
	return Unwrap(window)->GetId();
}

WXK_FUNCTION(void*) wxk_Window_GetHandle(wxk_Window* window) noexcept {
	return reinterpret_cast<void*>(Unwrap(window)->GetHandle());
}

WXK_FUNCTION(wxk_Window*) wxk_Window_GetParent(wxk_Window* window) noexcept {
	return wxk::Wrap(Unwrap(window)->GetParent());
}

WXK_FUNCTION(wxk_String*) wxk_Window_GetLabel(wxk_Window* window) noexcept {
	return Guard<wxk_String*>(nullptr, [&] { return wxk::ToUtf16(Unwrap(window)->GetLabel()); });
}

WXK_FUNCTION(void) wxk_Window_SetLabel(wxk_Window* window, const char16_t* label, size_t labelLength) noexcept {
	Guard([&] { Unwrap(window)->SetLabel(wxk::FromUtf16(label, labelLength)); });
}

WXK_FUNCTION(uint8_t) wxk_Window_Show(wxk_Window* window, uint8_t show) noexcept {
	return Guard<uint8_t>(0, [&] { return Unwrap(window)->Show(show != 0) ? 1 : 0; });
}

WXK_FUNCTION(uint8_t) wxk_Window_IsShown(wxk_Window* window) noexcept {
	return Unwrap(window)->IsShown() ? 1 : 0;
}

WXK_FUNCTION(uint8_t) wxk_Window_Enable(wxk_Window* window, uint8_t enable) noexcept {
	return Guard<uint8_t>(0, [&] { return Unwrap(window)->Enable(enable != 0) ? 1 : 0; });
}

WXK_FUNCTION(uint8_t) wxk_Window_IsEnabled(wxk_Window* window) noexcept {
	return Unwrap(window)->IsEnabled() ? 1 : 0;
}

WXK_FUNCTION(void) wxk_Window_SetFocus(wxk_Window* window) noexcept {
	Guard([&] { Unwrap(window)->SetFocus(); });
}

WXK_FUNCTION(uint8_t) wxk_Window_Close(wxk_Window* window, uint8_t force) noexcept {
	return Guard<uint8_t>(0, [&] { return Unwrap(window)->Close(force != 0) ? 1 : 0; });
}

WXK_FUNCTION(uint8_t) wxk_Window_Destroy(wxk_Window* window) noexcept {
	return Guard<uint8_t>(0, [&] { return Unwrap(window)->Destroy() ? 1 : 0; });
}

WXK_FUNCTION(void) wxk_Window_SetSizer(wxk_Window* window, wxk_Sizer* sizer) noexcept {
	Guard([&] { Unwrap(window)->SetSizer(Unwrap(sizer), true); });
}

WXK_FUNCTION(void) wxk_Window_Fit(wxk_Window* window) noexcept {
	Guard([&] { Unwrap(window)->Fit(); });
}

WXK_FUNCTION(uint8_t) wxk_Window_Layout(wxk_Window* window) noexcept {
	return Guard<uint8_t>(0, [&] { return Unwrap(window)->Layout() ? 1 : 0; });
}

WXK_FUNCTION(void) wxk_Window_Centre(wxk_Window* window, int32_t direction) noexcept {
	Guard([&] { Unwrap(window)->Centre(direction); });
}

WXK_FUNCTION(wxk_String*) wxk_TopLevelWindow_GetTitle(wxk_Window* window) noexcept {
	return Guard<wxk_String*>(nullptr, [&] { return wxk::ToUtf16(wxk::As<wxTopLevelWindow>(window)->GetTitle()); });
}

WXK_FUNCTION(void) wxk_TopLevelWindow_SetTitle(wxk_Window* window, const char16_t* title, size_t titleLength) noexcept {
	Guard([&] { wxk::As<wxTopLevelWindow>(window)->SetTitle(wxk::FromUtf16(title, titleLength)); });
}

WXK_FUNCTION(wxk_Window*) wxk_Frame_new(wxk_Window* parent, int32_t id, const char16_t* title, size_t titleLength, wxk_Point position, wxk_Size size, int64_t style) noexcept {
	return Guard<wxk_Window*>(nullptr, [&] {
		wxWindow* frame = new wxFrame(Unwrap(parent), id, wxk::FromUtf16(title, titleLength), wxk::FromPoint(position), wxk::FromSize(size), static_cast<long>(style));
		return wxk::Wrap(frame);
	});
}

WXK_FUNCTION(void) wxk_Frame_SetMenuBar(wxk_Window* frame, wxk_Window* menuBar) noexcept {
	Guard([&] { wxk::As<wxFrame>(frame)->SetMenuBar(wxk::As<wxMenuBar>(menuBar)); });
}

WXK_FUNCTION(wxk_Window*) wxk_Panel_new(wxk_Window* parent, int32_t id, wxk_Point position, wxk_Size size, int64_t style) noexcept {
	return Guard<wxk_Window*>(nullptr, [&] {
		wxWindow* panel = new wxPanel(Unwrap(parent), id, wxk::FromPoint(position), wxk::FromSize(size), static_cast<long>(style));
		return wxk::Wrap(panel);
	});
}

WXK_FUNCTION(wxk_Window*) wxk_Button_new(wxk_Window* parent, int32_t id, const char16_t* label, size_t labelLength, wxk_Point position, wxk_Size size, int64_t style) noexcept {
	return Guard<wxk_Window*>(nullptr, [&] {
		wxWindow* button = new wxButton(Unwrap(parent), id, wxk::FromUtf16(label, labelLength), wxk::FromPoint(position), wxk::FromSize(size), static_cast<long>(style));
		return wxk::Wrap(button);
	});
}

WXK_FUNCTION(wxk_Window*) wxk_StaticText_new(wxk_Window* parent, int32_t id, const char16_t* label, size_t labelLength, wxk_Point position, wxk_Size size, int64_t style) noexcept {
	return Guard<wxk_Window*>(nullptr, [&] {
		wxWindow* text = new wxStaticText(Unwrap(parent), id, wxk::FromUtf16(label, labelLength), wxk::FromPoint(position), wxk::FromSize(size), static_cast<long>(style));
		return wxk::Wrap(text);
	});
}

WXK_FUNCTION(wxk_Window*) wxk_TextCtrl_new(wxk_Window* parent, int32_t id, const char16_t* value, size_t valueLength, wxk_Point position, wxk_Size size, int64_t style) noexcept {
	return Guard<wxk_Window*>(nullptr, [&] {
		wxWindow* text = new wxTextCtrl(Unwrap(parent), id, wxk::FromUtf16(value, valueLength), wxk::FromPoint(position), wxk::FromSize(size), static_cast<long>(style));
		return wxk::Wrap(text);
	});
}

WXK_FUNCTION(wxk_String*) wxk_TextCtrl_GetValue(wxk_Window* text) noexcept {
	return Guard<wxk_String*>(nullptr, [&] { return wxk::ToUtf16(wxk::As<wxTextCtrl>(text)->GetValue()); });
}

WXK_FUNCTION(void) wxk_TextCtrl_SetValue(wxk_Window* text, const char16_t* value, size_t valueLength) noexcept {
	Guard([&] { wxk::As<wxTextCtrl>(text)->SetValue(wxk::FromUtf16(value, valueLength)); });
}

WXK_FUNCTION(void) wxk_TextCtrl_ChangeValue(wxk_Window* text, const char16_t* value, size_t valueLength) noexcept {
	Guard([&] { wxk::As<wxTextCtrl>(text)->ChangeValue(wxk::FromUtf16(value, valueLength)); });
}

WXK_FUNCTION(int32_t) wxk_Dialog_ShowModal(wxk_Window* dialog) noexcept {
	return Guard<int32_t>(wxID_CANCEL, [&] { return wxk::As<wxDialog>(dialog)->ShowModal(); });
}

WXK_FUNCTION(wxk_Window*) wxk_MessageDialog_new(wxk_Window* parent, const char16_t* message, size_t messageLength, const char16_t* caption, size_t captionLength, int64_t style) noexcept {
	return Guard<wxk_Window*>(nullptr, [&] {
		wxWindow* dialog = new wxMessageDialog(Unwrap(parent), wxk::FromUtf16(message, messageLength), wxk::FromUtf16(caption, captionLength), static_cast<long>(style));
		return wxk::Wrap(dialog);
	});
}
