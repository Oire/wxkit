// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagtiff.h>

WXK_FUNCTION(void) wxk_Image_AddTiffHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxTIFFHandler>(wxBITMAP_TYPE_TIFF);
	});
}
