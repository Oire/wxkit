#include "shim.h"

#include <wx/taskbar.h>

namespace {

// A taskbar icon whose menu is the one .NET gives it. wxWidgets shows the menu GetPopupMenu
// returns, and leaves it, so the managed side keeps and frees it.
class ManagedTaskBarIcon final : public wxTaskBarIcon {
public:
	explicit ManagedTaskBarIcon(wxTaskBarIconType type) : wxTaskBarIcon(type) {}

	wxMenu* menu = nullptr;

protected:
	wxMenu* GetPopupMenu() override { return menu; }
};

ManagedTaskBarIcon* IconOf(wxk_EvtHandler* icon) {
	return static_cast<ManagedTaskBarIcon*>(static_cast<wxTaskBarIcon*>(wxk::Unwrap(icon)));
}

}

WXK_FUNCTION(wxk_EvtHandler*) wxk_ManagedTaskBarIcon_new(int32_t type) noexcept {
	// A negative type is the platform's default, whose value differs between platforms.
	const wxTaskBarIconType iconType = type < 0 ? wxTBI_DEFAULT_TYPE : static_cast<wxTaskBarIconType>(type);
	return wxk::Guard<wxk_EvtHandler*>(nullptr, [&]() -> wxk_EvtHandler* { return wxk::Wrap(static_cast<wxEvtHandler*>(new ManagedTaskBarIcon(iconType))); });
}

WXK_FUNCTION(void) wxk_ManagedTaskBarIcon_SetMenu(wxk_EvtHandler* icon, wxk_Menu* menu) noexcept {
	wxk::Guard([&] { IconOf(icon)->menu = wxk::Unwrap(menu); });
}

WXK_FUNCTION(void) wxk_ManagedTaskBarIcon_Destroy(wxk_EvtHandler* icon) noexcept {
	wxk::Guard([&] {
		ManagedTaskBarIcon* taskBarIcon = IconOf(icon);
		taskBarIcon->menu = nullptr;
		taskBarIcon->RemoveIcon();
		taskBarIcon->Destroy();
	});
}
