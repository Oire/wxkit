// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagbmp.h>

WXK_FUNCTION(void) wxk_Image_AddIconHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxICOHandler>(wxBITMAP_TYPE_ICO);
		wxk::AddImageHandler<wxCURHandler>(wxBITMAP_TYPE_CUR);
		wxk::AddImageHandler<wxANIHandler>(wxBITMAP_TYPE_ANI);
	});
}
