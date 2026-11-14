// Each format is in a file of its own, so a statically linked application only links the formats it adds.
#include "../shim.h"

#include <wx/imagpcx.h>

WXK_FUNCTION(void) wxk_Image_AddPcxHandler(void) noexcept {
	wxk::Guard([] {
		wxk::AddImageHandler<wxPCXHandler>(wxBITMAP_TYPE_PCX);
	});
}
