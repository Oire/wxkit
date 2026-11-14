// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagpng.h>

WXK_FUNCTION(void) wxk_Image_AddPngHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxPNGHandler>(wxBITMAP_TYPE_PNG);
	});
}
