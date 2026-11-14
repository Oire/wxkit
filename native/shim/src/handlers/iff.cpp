// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagiff.h>

WXK_FUNCTION(void) wxk_Image_AddIffHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxIFFHandler>(wxBITMAP_TYPE_IFF);
	});
}
