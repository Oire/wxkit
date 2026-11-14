// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagjpeg.h>

WXK_FUNCTION(void) wxk_Image_AddJpegHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxJPEGHandler>(wxBITMAP_TYPE_JPEG);
	});
}
