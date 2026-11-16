#include "shim.h"

namespace {

wxGridBagSizer* GridOf(wxk_Sizer* sizer) {
	return static_cast<wxGridBagSizer*>(wxk::Unwrap(sizer));
}

}

WXK_FUNCTION(uint8_t) wxk_GridBagSizer_AddWindow(wxk_Sizer* sizer, wxk_Window* window, wxk_GridPosition position, wxk_GridSpan span, int32_t flags, int32_t border) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t {
		wxGridBagSizer* grid = GridOf(sizer);
		if (grid->CheckForIntersection(wxk::FromGridPosition(position), wxk::FromGridSpan(span))) return 0;
		return grid->Add(wxk::Unwrap(window), wxk::FromGridPosition(position), wxk::FromGridSpan(span), flags, border) != nullptr ? 1 : 0;
	});
}

WXK_FUNCTION(uint8_t) wxk_GridBagSizer_AddSizer(wxk_Sizer* sizer, wxk_Sizer* child, wxk_GridPosition position, wxk_GridSpan span, int32_t flags, int32_t border) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t {
		wxGridBagSizer* grid = GridOf(sizer);
		if (grid->CheckForIntersection(wxk::FromGridPosition(position), wxk::FromGridSpan(span))) return 0;
		return grid->Add(wxk::Unwrap(child), wxk::FromGridPosition(position), wxk::FromGridSpan(span), flags, border) != nullptr ? 1 : 0;
	});
}

WXK_FUNCTION(uint8_t) wxk_GridBagSizer_AddSpacer(wxk_Sizer* sizer, wxk_Size size, wxk_GridPosition position, wxk_GridSpan span, int32_t flags, int32_t border) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t {
		wxGridBagSizer* grid = GridOf(sizer);
		if (grid->CheckForIntersection(wxk::FromGridPosition(position), wxk::FromGridSpan(span))) return 0;
		return grid->Add(size.width, size.height, wxk::FromGridPosition(position), wxk::FromGridSpan(span), flags, border) != nullptr ? 1 : 0;
	});
}
