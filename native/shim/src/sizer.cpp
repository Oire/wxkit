#include "shim.h"

using wxk::Guard;
using wxk::Unwrap;

namespace {

int Border(int32_t border) {
	return border < 0 ? wxSizerFlags::GetDefaultBorder() : border;
}

} // namespace

WXK_FUNCTION(wxk_Sizer*) wxk_BoxSizer_new(int32_t orientation) noexcept {
	return Guard<wxk_Sizer*>(nullptr, [&] {
		wxSizer* sizer = new wxBoxSizer(orientation);
		return wxk::Wrap(sizer);
	});
}

WXK_FUNCTION(void) wxk_Sizer_AddWindow(wxk_Sizer* sizer, wxk_Window* window, int32_t proportion, int32_t flags, int32_t border) noexcept {
	Guard([&] { Unwrap(sizer)->Add(Unwrap(window), proportion, flags, Border(border)); });
}

WXK_FUNCTION(void) wxk_Sizer_AddSizer(wxk_Sizer* sizer, wxk_Sizer* child, int32_t proportion, int32_t flags, int32_t border) noexcept {
	Guard([&] { Unwrap(sizer)->Add(Unwrap(child), proportion, flags, Border(border)); });
}

WXK_FUNCTION(void) wxk_Sizer_AddSpacer(wxk_Sizer* sizer, int32_t size) noexcept {
	Guard([&] { Unwrap(sizer)->AddSpacer(size); });
}

WXK_FUNCTION(void) wxk_Sizer_AddStretchSpacer(wxk_Sizer* sizer, int32_t proportion) noexcept {
	Guard([&] { Unwrap(sizer)->AddStretchSpacer(proportion); });
}

WXK_FUNCTION(void) wxk_Sizer_Free(wxk_Sizer* sizer) noexcept {
	Guard([&] { delete Unwrap(sizer); });
}
