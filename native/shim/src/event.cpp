#include "shim.h"

#include <memory>

namespace {

// wxWidgets copies a bound functor, and deletes the copies when the binding goes away, so the
// callback's state is shared between them and released once, when the last copy is deleted.
struct Binding {
	wxk_EventCallback callback;
	void* userdata;
	wxk_Callback onFree;

	~Binding() {
		if (onFree != nullptr) onFree(userdata);
	}
};

class Functor {
public:
	explicit Functor(std::shared_ptr<Binding> binding) : _binding(std::move(binding)) {}

	void operator()(wxEvent& event) const {
		_binding->callback(_binding->userdata, wxk::Wrap(&event));
	}

private:
	std::shared_ptr<Binding> _binding;
};

} // namespace

WXK_FUNCTION(uint8_t) wxk_EvtHandler_Bind(wxk_EvtHandler* handler, int32_t eventType, int32_t id, int32_t lastId, wxk_EventCallback callback, void* userdata, wxk_Callback onFree) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t {
		auto binding = std::make_shared<Binding>(Binding{callback, userdata, nullptr});
		wxk::Unwrap(handler)->Bind(wxEventTypeTag<wxEvent>(eventType), Functor(binding), id, lastId);
		// Only once wxWidgets holds the functor does releasing it become its job.
		binding->onFree = onFree;
		return 1;
	});
}

WXK_FUNCTION(int32_t) wxk_EventType_Button(void) noexcept {
	return wxEVT_BUTTON;
}

WXK_FUNCTION(int32_t) wxk_EventType_Menu(void) noexcept {
	return wxEVT_MENU;
}

WXK_FUNCTION(int32_t) wxk_EventType_Text(void) noexcept {
	return wxEVT_TEXT;
}

WXK_FUNCTION(int32_t) wxk_EventType_CloseWindow(void) noexcept {
	return wxEVT_CLOSE_WINDOW;
}

WXK_FUNCTION(int32_t) wxk_EventType_Timer(void) noexcept {
	return wxEVT_TIMER;
}

WXK_FUNCTION(int32_t) wxk_Event_GetEventType(wxk_Event* event) noexcept {
	return wxk::Unwrap(event)->GetEventType();
}

WXK_FUNCTION(int32_t) wxk_Event_GetId(wxk_Event* event) noexcept {
	return wxk::Unwrap(event)->GetId();
}

WXK_FUNCTION(wxk_Object*) wxk_Event_GetEventObject(wxk_Event* event) noexcept {
	return wxk::Wrap(wxk::Unwrap(event)->GetEventObject());
}

WXK_FUNCTION(void) wxk_Event_Skip(wxk_Event* event, uint8_t skip) noexcept {
	wxk::Unwrap(event)->Skip(skip != 0);
}

WXK_FUNCTION(uint8_t) wxk_Event_GetSkipped(wxk_Event* event) noexcept {
	return wxk::Unwrap(event)->GetSkipped() ? 1 : 0;
}

WXK_FUNCTION(wxk_String*) wxk_CommandEvent_GetString(wxk_Event* event) noexcept {
	return wxk::Guard<wxk_String*>(nullptr, [&] { return wxk::ToUtf16(wxk::As<wxCommandEvent>(event)->GetString()); });
}

WXK_FUNCTION(int32_t) wxk_CommandEvent_GetInt(wxk_Event* event) noexcept {
	return wxk::As<wxCommandEvent>(event)->GetInt();
}

WXK_FUNCTION(int32_t) wxk_CommandEvent_GetSelection(wxk_Event* event) noexcept {
	return wxk::As<wxCommandEvent>(event)->GetSelection();
}

WXK_FUNCTION(uint8_t) wxk_CommandEvent_IsChecked(wxk_Event* event) noexcept {
	return wxk::As<wxCommandEvent>(event)->IsChecked() ? 1 : 0;
}

WXK_FUNCTION(uint8_t) wxk_CloseEvent_CanVeto(wxk_Event* event) noexcept {
	return wxk::As<wxCloseEvent>(event)->CanVeto() ? 1 : 0;
}

WXK_FUNCTION(void) wxk_CloseEvent_Veto(wxk_Event* event, uint8_t veto) noexcept {
	wxk::Guard([&] { wxk::As<wxCloseEvent>(event)->Veto(veto != 0); });
}

WXK_FUNCTION(uint8_t) wxk_CloseEvent_GetVeto(wxk_Event* event) noexcept {
	return wxk::As<wxCloseEvent>(event)->GetVeto() ? 1 : 0;
}
