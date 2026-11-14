#include "shim.h"

#include <wx/tracker.h>

namespace {

// wxTrackable notifies its nodes from its own destructor, which runs after wxEvtHandler's. By
// then the handler's bound functors are gone, so .NET hears about the object last, after every
// event it will ever get, including the window's own destroy event.
class Tracker final : public wxTrackerNode {
public:
	Tracker(wxk_Callback onDestroy, void* userdata) : _onDestroy(onDestroy), _userdata(userdata) {}

	void OnObjectDestroy() override {
		const wxk_Callback onDestroy = _onDestroy;
		void* const userdata = _userdata;
		delete this;
		onDestroy(userdata);
	}

private:
	wxk_Callback _onDestroy;
	void* _userdata;
};

} // namespace

WXK_FUNCTION(uint8_t) wxk_EvtHandler_Track(wxk_EvtHandler* handler, wxk_Callback onDestroy, void* userdata) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t {
		wxk::Unwrap(handler)->AddNode(new Tracker(onDestroy, userdata));
		return 1;
	});
}

WXK_FUNCTION(const wxk_ClassInfo*) wxk_Object_GetClassInfo(wxk_Object* object) noexcept {
	return wxk::Guard<const wxk_ClassInfo*>(nullptr, [&] { return wxk::Wrap(wxk::Unwrap(object)->GetClassInfo()); });
}

WXK_FUNCTION(wxk_String*) wxk_ClassInfo_GetClassName(const wxk_ClassInfo* info) noexcept {
	return wxk::Guard<wxk_String*>(nullptr, [&] { return wxk::ToUtf16(wxk::Unwrap(info)->GetClassName()); });
}

WXK_FUNCTION(const wxk_ClassInfo*) wxk_ClassInfo_GetBaseClass(const wxk_ClassInfo* info) noexcept {
	return wxk::Guard<const wxk_ClassInfo*>(nullptr, [&] { return wxk::Wrap(wxk::Unwrap(info)->GetBaseClass1()); });
}
