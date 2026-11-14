// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagpnm.h>

WXK_FUNCTION(void) wxk_Image_AddPnmHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxPNMHandler>(wxBITMAP_TYPE_PNM);
	});
}
