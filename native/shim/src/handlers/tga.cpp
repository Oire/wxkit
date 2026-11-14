// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagtga.h>

WXK_FUNCTION(void) wxk_Image_AddTgaHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxTGAHandler>(wxBITMAP_TYPE_TGA);
	});
}
