#include "shim.h"

#include <wx/notifmsg.h>

WXK_FUNCTION(void) wxk_NotificationMessage_Free(wxk_EvtHandler* notification) noexcept {
	wxk::Guard([&] { delete static_cast<wxNotificationMessage*>(wxk::Unwrap(notification)); });
}
