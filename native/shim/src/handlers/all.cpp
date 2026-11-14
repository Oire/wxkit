// Every format wxWidgets has, which links all of them.
#include "../shim.h"

WXK_FUNCTION(void) wxk_Image_AddAllHandlers(void) noexcept {
	wxk::Guard([] { wxInitAllImageHandlers(); });
}
