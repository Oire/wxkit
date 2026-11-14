#include "shim.h"

#include <wx/clipbrd.h>

namespace {

// wxTheClipboard has to be opened around every use, and closed again whatever happens, so that
// other applications can use it.
class ClipboardSession {
public:
	ClipboardSession() : _opened(wxTheClipboard->Open()) {}

	~ClipboardSession() {
		if (_opened) wxTheClipboard->Close();
	}

	explicit operator bool() const { return _opened; }

private:
	bool _opened;
};

} // namespace

WXK_FUNCTION(uint8_t) wxk_Clipboard_SetText(const char16_t* text, size_t textLength) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t {
		ClipboardSession clipboard;
		if (!clipboard) return 0;
		// The clipboard takes ownership of the data object.
		return wxTheClipboard->SetData(new wxTextDataObject(wxk::FromUtf16(text, textLength))) ? 1 : 0;
	});
}

WXK_FUNCTION(wxk_String*) wxk_Clipboard_GetText(void) noexcept {
	return wxk::Guard<wxk_String*>(nullptr, []() -> wxk_String* {
		ClipboardSession clipboard;
		if (!clipboard || !wxTheClipboard->IsSupported(wxDF_UNICODETEXT)) return nullptr;
		wxTextDataObject data;
		return wxTheClipboard->GetData(data) ? wxk::ToUtf16(data.GetText()) : nullptr;
	});
}

WXK_FUNCTION(uint8_t) wxk_Clipboard_HasText(void) noexcept {
	return wxk::Guard<uint8_t>(0, []() -> uint8_t {
		ClipboardSession clipboard;
		return clipboard && wxTheClipboard->IsSupported(wxDF_UNICODETEXT) ? 1 : 0;
	});
}

WXK_FUNCTION(void) wxk_Clipboard_Clear(void) noexcept {
	wxk::Guard([] {
		ClipboardSession clipboard;
		if (clipboard) wxTheClipboard->Clear();
	});
}
