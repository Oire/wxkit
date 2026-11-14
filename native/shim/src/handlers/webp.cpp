// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagwebp.h>

WXK_FUNCTION(void) wxk_Image_AddWebpHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxWEBPHandler>(wxBITMAP_TYPE_WEBP);
	});
}
