// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagxpm.h>

WXK_FUNCTION(void) wxk_Image_AddXpmHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxXPMHandler>(wxBITMAP_TYPE_XPM);
	});
}
