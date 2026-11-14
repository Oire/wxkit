#include "shim.h"

WXK_FUNCTION(const char16_t*) wxk_String_GetData(const wxk_String* text, size_t* length) noexcept {
	if (text == nullptr) {
		*length = 0;
		return nullptr;
	}
	*length = text->text.size();
	return text->text.data();
}

WXK_FUNCTION(void) wxk_String_Free(wxk_String* text) noexcept {
	delete text;
}
