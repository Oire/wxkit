#include "shim.h"

#include <wx/print.h>

namespace {

wxk_PrintoutCallbacks callbacks{};

// A printout whose virtual members ask .NET. wxWidgets's own OnBeginDocument and OnEndDocument
// start and end the document on the DC, so they run around .NET's.
class ManagedPrintout final : public wxPrintout {
public:
	ManagedPrintout(const wxString& title, void* userdata) : wxPrintout(title), _userdata(userdata) {}

	~ManagedPrintout() override {
		if (callbacks.onFree != nullptr) callbacks.onFree(_userdata);
	}

	bool OnPrintPage(int page) override {
		return callbacks.printPage != nullptr && callbacks.printPage(_userdata, page, wxk::Wrap(static_cast<wxObject*>(GetDC()))) != 0;
	}

	bool HasPage(int page) override {
		return callbacks.hasPage != nullptr && callbacks.hasPage(_userdata, page) != 0;
	}

	void GetPageInfo(int* minPage, int* maxPage, int* fromPage, int* toPage) override {
		int32_t values[4] = {1, 1, 1, 1};
		if (callbacks.getPageInfo != nullptr) callbacks.getPageInfo(_userdata, &values[0], &values[1], &values[2], &values[3]);
		*minPage = values[0];
		*maxPage = values[1];
		*fromPage = values[2];
		*toPage = values[3];
	}

	void OnPreparePrinting() override {
		if (callbacks.preparePrinting != nullptr) callbacks.preparePrinting(_userdata);
	}

	void OnBeginPrinting() override {
		if (callbacks.beginPrinting != nullptr) callbacks.beginPrinting(_userdata);
	}

	void OnEndPrinting() override {
		if (callbacks.endPrinting != nullptr) callbacks.endPrinting(_userdata);
	}

	bool OnBeginDocument(int startPage, int endPage) override {
		if (!wxPrintout::OnBeginDocument(startPage, endPage)) return false;
		return callbacks.beginDocument == nullptr || callbacks.beginDocument(_userdata, startPage, endPage) != 0;
	}

	void OnEndDocument() override {
		if (callbacks.endDocument != nullptr) callbacks.endDocument(_userdata);
		wxPrintout::OnEndDocument();
	}

private:
	void* _userdata;
};

}

WXK_FUNCTION(void) wxk_Printout_SetCallbacks(const wxk_PrintoutCallbacks* set) noexcept {
	callbacks = *set;
}

WXK_FUNCTION(wxk_Object*) wxk_ManagedPrintout_new(const char16_t* title, size_t titleLength, void* userdata) noexcept {
	return wxk::Guard<wxk_Object*>(nullptr, [&]() -> wxk_Object* { return wxk::Wrap(static_cast<wxObject*>(new ManagedPrintout(wxk::FromUtf16(title, titleLength), userdata))); });
}

WXK_FUNCTION(wxk_Window*) wxk_PrintPreview_Show(wxk_Object* printout, wxk_Object* printoutForPrinting, wxk_Object* data, wxk_Window* parent, const char16_t* title, size_t titleLength, wxk_Size size) noexcept {
	return wxk::Guard<wxk_Window*>(nullptr, [&]() -> wxk_Window* {
		auto* preview = new wxPrintPreview(static_cast<wxPrintout*>(wxk::Unwrap(printout)), printoutForPrinting != nullptr ? static_cast<wxPrintout*>(wxk::Unwrap(printoutForPrinting)) : nullptr, data != nullptr ? static_cast<wxPrintDialogData*>(wxk::Unwrap(data)) : nullptr);
		// A preview that is not ready, as when there is no printer, deletes its printouts with it.
		if (!preview->IsOk()) {
			delete preview;
			return nullptr;
		}
		auto* frame = new wxPreviewFrame(preview, wxk::Unwrap(parent), wxk::FromUtf16(title, titleLength), wxDefaultPosition, wxk::FromSize(size));
		frame->Initialize();
		frame->Show();
		return wxk::Wrap(static_cast<wxWindow*>(frame));
	});
}
