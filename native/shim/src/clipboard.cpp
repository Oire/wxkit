#include "shim.h"

#include <wx/clipbrd.h>
#include <wx/imagpng.h>

#include <cstring>

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

// Content in several formats, which the clipboard offers together. Applications pick the format
// they understand, and the first added is preferred.
WXK_FUNCTION(wxk_ClipboardContent*) wxk_ClipboardContent_new(void) noexcept {
	return wxk::Guard<wxk_ClipboardContent*>(nullptr, [] { return reinterpret_cast<wxk_ClipboardContent*>(new wxDataObjectComposite()); });
}

WXK_FUNCTION(void) wxk_ClipboardContent_Free(wxk_ClipboardContent* content) noexcept {
	wxk::Guard([&] { delete reinterpret_cast<wxDataObjectComposite*>(content); });
}

namespace {

// wxGTK hands a bitmap to the clipboard, and takes one back, as a PNG image, which needs the PNG
// handler, so the clipboard's bitmaps add it, as Image.AddPngHandler would.
void UsePngForBitmaps() {
#ifdef __WXGTK__
	wxk::AddImageHandler<wxPNGHandler>(wxBITMAP_TYPE_PNG);
#endif
}

void Add(wxk_ClipboardContent* content, wxDataObjectSimple* data) {
	// The composite takes ownership of each part.
	reinterpret_cast<wxDataObjectComposite*>(content)->Add(data);
}

} // namespace

WXK_FUNCTION(void) wxk_ClipboardContent_AddText(wxk_ClipboardContent* content, const char16_t* text, size_t textLength) noexcept {
	wxk::Guard([&] { Add(content, new wxTextDataObject(wxk::FromUtf16(text, textLength))); });
}

WXK_FUNCTION(void) wxk_ClipboardContent_AddHtml(wxk_ClipboardContent* content, const char16_t* html, size_t htmlLength) noexcept {
	wxk::Guard([&] { Add(content, new wxHTMLDataObject(wxk::FromUtf16(html, htmlLength))); });
}

WXK_FUNCTION(void) wxk_ClipboardContent_AddBitmap(wxk_ClipboardContent* content, wxk_Object* bitmap) noexcept {
	wxk::Guard([&] {
		UsePngForBitmaps();
		Add(content, new wxBitmapDataObject(*static_cast<wxBitmap*>(wxk::Unwrap(bitmap))));
	});
}

WXK_FUNCTION(void) wxk_ClipboardContent_AddFiles(wxk_ClipboardContent* content, const wxk_StringArray* files) noexcept {
	wxk::Guard([&] {
		auto* data = new wxFileDataObject();
		for (const wxString& file : *wxk::Unwrap(const_cast<wxk_StringArray*>(files))) data->AddFile(file);
		Add(content, data);
	});
}

WXK_FUNCTION(void) wxk_ClipboardContent_AddData(wxk_ClipboardContent* content, const char16_t* format, size_t formatLength, const uint8_t* data, size_t size) noexcept {
	wxk::Guard([&] {
		auto* custom = new wxCustomDataObject(wxDataFormat(wxk::FromUtf16(format, formatLength)));
		custom->SetData(size, data);
		Add(content, custom);
	});
}

// Replaces the clipboard's content. The clipboard takes ownership of the content, and frees it if
// it cannot be opened.
WXK_FUNCTION(uint8_t) wxk_Clipboard_SetContent(wxk_ClipboardContent* content) noexcept {
	auto* data = reinterpret_cast<wxDataObjectComposite*>(content);
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t {
		ClipboardSession clipboard;
		if (!clipboard) {
			delete data;
			return 0;
		}
		return wxTheClipboard->SetData(data) ? 1 : 0;
	});
}

namespace {

wxDataFormat FormatOf(wxk_ClipboardFormat format) {
	switch (format) {
	case WXK_CLIPBOARD_BITMAP:
		return wxDF_BITMAP;
	case WXK_CLIPBOARD_FILES:
		return wxDF_FILENAME;
	case WXK_CLIPBOARD_HTML:
		return wxDF_HTML;
	default:
		return wxDF_UNICODETEXT;
	}
}

// Opens the clipboard and reads one format from it, if the clipboard has it.
template <typename Data, typename Result, typename Read>
Result GetFormat(const wxDataFormat& format, Data& data, Result none, Read read) {
	ClipboardSession clipboard;
	if (!clipboard || !wxTheClipboard->IsSupported(format) || !wxTheClipboard->GetData(data)) return none;
	return read(data);
}

} // namespace

WXK_FUNCTION(uint8_t) wxk_Clipboard_Has(wxk_ClipboardFormat format) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t {
		ClipboardSession clipboard;
		return clipboard && wxTheClipboard->IsSupported(FormatOf(format)) ? 1 : 0;
	});
}

WXK_FUNCTION(uint8_t) wxk_Clipboard_HasData(const char16_t* format, size_t formatLength) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t {
		ClipboardSession clipboard;
		return clipboard && wxTheClipboard->IsSupported(wxDataFormat(wxk::FromUtf16(format, formatLength))) ? 1 : 0;
	});
}

WXK_FUNCTION(wxk_String*) wxk_Clipboard_GetHtml(void) noexcept {
	return wxk::Guard<wxk_String*>(nullptr, [] {
		wxHTMLDataObject data;
		return GetFormat(wxDF_HTML, data, static_cast<wxk_String*>(nullptr), [](wxHTMLDataObject& html) { return wxk::ToUtf16(html.GetHTML()); });
	});
}

WXK_FUNCTION(wxk_Object*) wxk_Clipboard_GetBitmap(void) noexcept {
	return wxk::Guard<wxk_Object*>(nullptr, [] {
		UsePngForBitmaps();
		wxBitmapDataObject data;
		return GetFormat(wxDF_BITMAP, data, static_cast<wxk_Object*>(nullptr), [](wxBitmapDataObject& bitmap) { return wxk::CopyValue(bitmap.GetBitmap()); });
	});
}

WXK_FUNCTION(wxk_StringArray*) wxk_Clipboard_GetFiles(void) noexcept {
	return wxk::Guard<wxk_StringArray*>(nullptr, [] {
		wxFileDataObject data;
		return GetFormat(wxDF_FILENAME, data, static_cast<wxk_StringArray*>(nullptr), [](wxFileDataObject& files) { return wxk::Wrap(new wxArrayString(files.GetFilenames())); });
	});
}

// Hands out a copy of the data in a custom format, which the caller frees with wxk_Bytes_Free, or
// null when the clipboard does not have it.
WXK_FUNCTION(uint8_t*) wxk_Clipboard_GetData(const char16_t* format, size_t formatLength, size_t* size) noexcept {
	*size = 0;
	return wxk::Guard<uint8_t*>(nullptr, [&]() -> uint8_t* {
		wxDataFormat id(wxk::FromUtf16(format, formatLength));
		wxCustomDataObject data(id);
		return GetFormat(id, data, static_cast<uint8_t*>(nullptr), [&](wxCustomDataObject& custom) -> uint8_t* {
			auto* copy = new uint8_t[custom.GetSize() > 0 ? custom.GetSize() : 1];
			std::memcpy(copy, custom.GetData(), custom.GetSize());
			*size = custom.GetSize();
			return copy;
		});
	});
}

WXK_FUNCTION(void) wxk_Bytes_Free(uint8_t* bytes) noexcept {
	delete[] bytes;
}

// Keeps the clipboard's content after the application exits, which it otherwise takes with it.
WXK_FUNCTION(uint8_t) wxk_Clipboard_Flush(void) noexcept {
	return wxk::Guard<uint8_t>(0, []() -> uint8_t { return wxTheClipboard->Flush() ? 1 : 0; });
}
