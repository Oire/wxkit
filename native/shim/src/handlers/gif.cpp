// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imaggif.h>

WXK_FUNCTION(void) wxk_Image_AddGifHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxGIFHandler>(wxBITMAP_TYPE_GIF);
	});
}
