#include "shim.h"

#include <wx/toolbar.h>

namespace {

wxToolBar* ToolBarOf(wxk_Window* toolBar) {
	return static_cast<wxToolBar*>(wxk::Unwrap(toolBar));
}

}

WXK_FUNCTION(wxk_Window*) wxk_ToolBarTool_GetToolBar(wxk_ToolBarTool* tool) noexcept {
	return wxk::Guard<wxk_Window*>(nullptr, [&]() -> wxk_Window* { return wxk::Wrap(static_cast<wxWindow*>(wxk::Unwrap(tool)->GetToolBar())); });
}

WXK_FUNCTION(uint8_t) wxk_ToolBar_DeleteTool(wxk_Window* toolBar, int32_t toolId) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return ToolBarOf(toolBar)->DeleteTool(toolId) ? 1 : 0; });
}

WXK_FUNCTION(uint8_t) wxk_ToolBar_DeleteToolByPos(wxk_Window* toolBar, size_t position) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return ToolBarOf(toolBar)->DeleteToolByPos(position) ? 1 : 0; });
}

WXK_FUNCTION(void) wxk_ToolBar_ClearTools(wxk_Window* toolBar) noexcept {
	wxk::Guard([&] { ToolBarOf(toolBar)->ClearTools(); });
}
