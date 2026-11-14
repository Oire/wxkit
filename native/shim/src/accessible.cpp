#include "shim.h"

#include <wx/access.h>

// The statuses .NET returns are wxAccStatus's values.
static_assert(wxACC_FAIL == 0 && wxACC_FALSE == 1 && wxACC_OK == 2 && wxACC_NOT_IMPLEMENTED == 3 && wxACC_NOT_SUPPORTED == 4 && wxACC_INVALID_ARG == 5);

namespace {

wxk_AccessibleCallbacks callbacks{};

wxAccessible* Unwrap(wxk_Accessible* accessible) {
	return reinterpret_cast<wxAccessible*>(accessible);
}

// An accessible object whose answers come from .NET. Before .NET sets the callbacks, and for
// anything .NET does not answer, it says it does not implement the question, so Windows answers
// it from the window's standard accessible object.
class ManagedAccessible final : public wxAccessible {
public:
	ManagedAccessible(wxWindow* window, void* userdata) : wxAccessible(window), _userdata(userdata) {}

	~ManagedAccessible() override {
		if (callbacks.onFree != nullptr) callbacks.onFree(_userdata);
	}

	wxAccStatus GetName(int childId, wxString* name) override {
		return Text(WXK_ACCESSIBLE_NAME, childId, name);
	}

	wxAccStatus GetDescription(int childId, wxString* description) override {
		return Text(WXK_ACCESSIBLE_DESCRIPTION, childId, description);
	}

	wxAccStatus GetHelpText(int childId, wxString* helpText) override {
		return Text(WXK_ACCESSIBLE_HELP_TEXT, childId, helpText);
	}

	wxAccStatus GetKeyboardShortcut(int childId, wxString* shortcut) override {
		return Text(WXK_ACCESSIBLE_KEYBOARD_SHORTCUT, childId, shortcut);
	}

	wxAccStatus GetDefaultAction(int childId, wxString* actionName) override {
		return Text(WXK_ACCESSIBLE_DEFAULT_ACTION, childId, actionName);
	}

	wxAccStatus GetValue(int childId, wxString* strValue) override {
		return Text(WXK_ACCESSIBLE_VALUE, childId, strValue);
	}

	wxAccStatus GetRole(int childId, wxAccRole* role) override {
		if (callbacks.getRole == nullptr) return wxACC_NOT_IMPLEMENTED;
		int32_t value = 0;
		auto status = static_cast<wxAccStatus>(callbacks.getRole(_userdata, childId, &value));
		if (status == wxACC_OK) *role = static_cast<wxAccRole>(value);
		return status;
	}

	wxAccStatus GetState(int childId, long* state) override {
		if (callbacks.getState == nullptr) return wxACC_NOT_IMPLEMENTED;
		int64_t value = 0;
		auto status = static_cast<wxAccStatus>(callbacks.getState(_userdata, childId, &value));
		if (status == wxACC_OK) *state = static_cast<long>(value);
		return status;
	}

	wxAccStatus GetChildCount(int* childCount) override {
		if (callbacks.getChildCount == nullptr) return wxACC_NOT_IMPLEMENTED;
		int32_t value = 0;
		auto status = static_cast<wxAccStatus>(callbacks.getChildCount(_userdata, &value));
		if (status == wxACC_OK) *childCount = value;
		return status;
	}

	wxAccStatus GetLocation(wxRect& rect, int elementId) override {
		if (callbacks.getLocation == nullptr) return wxACC_NOT_IMPLEMENTED;
		wxk_Rect value{};
		auto status = static_cast<wxAccStatus>(callbacks.getLocation(_userdata, elementId, &value));
		if (status == wxACC_OK) rect = wxk::FromRect(value);
		return status;
	}

	wxAccStatus GetChild(int childId, wxAccessible** child) override {
		int resultId = 0;
		return Element(WXK_ACCESSIBLE_CHILD, childId, 0, wxPoint(), &resultId, child);
	}

	wxAccStatus GetFocus(int* childId, wxAccessible** child) override {
		return Element(WXK_ACCESSIBLE_FOCUS, 0, 0, wxPoint(), childId, child);
	}

	wxAccStatus HitTest(const wxPoint& pt, int* childId, wxAccessible** childObject) override {
		return Element(WXK_ACCESSIBLE_HIT_TEST, 0, 0, pt, childId, childObject);
	}

	wxAccStatus Navigate(wxNavDir navDir, int fromId, int* toId, wxAccessible** toObject) override {
		return Element(WXK_ACCESSIBLE_NAVIGATE, fromId, navDir, wxPoint(), toId, toObject);
	}

	wxAccStatus DoDefaultAction(int childId) override {
		if (callbacks.act == nullptr) return wxACC_NOT_IMPLEMENTED;
		return static_cast<wxAccStatus>(callbacks.act(_userdata, WXK_ACCESSIBLE_DO_DEFAULT_ACTION, childId, 0));
	}

	wxAccStatus Select(int childId, wxAccSelectionFlags selectFlags) override {
		if (callbacks.act == nullptr) return wxACC_NOT_IMPLEMENTED;
		return static_cast<wxAccStatus>(callbacks.act(_userdata, WXK_ACCESSIBLE_SELECT, childId, selectFlags));
	}

private:
	wxAccStatus Text(wxk_AccessibleText which, int childId, wxString* text) {
		if (callbacks.getText == nullptr) return wxACC_NOT_IMPLEMENTED;
		wxk_String* value = nullptr;
		auto status = static_cast<wxAccStatus>(callbacks.getText(_userdata, which, childId, &value));
		wxString result = wxk::TakeString(value);
		if (status == wxACC_OK) *text = result;
		return status;
	}

	wxAccStatus Element(wxk_AccessibleElement which, int childId, int direction, const wxPoint& point, int* resultId, wxAccessible** resultObject) {
		if (callbacks.getElement == nullptr) return wxACC_NOT_IMPLEMENTED;
		int32_t id = 0;
		wxk_Accessible* object = nullptr;
		auto status = static_cast<wxAccStatus>(callbacks.getElement(_userdata, which, childId, direction, wxk::ToPoint(point), &id, &object));
		*resultId = id;
		*resultObject = Unwrap(object);
		return status;
	}

	void* _userdata;
};

} // namespace

WXK_FUNCTION(void) wxk_Accessible_SetCallbacks(const wxk_AccessibleCallbacks* set) noexcept {
	callbacks = *set;
}

WXK_FUNCTION(wxk_Accessible*) wxk_ManagedAccessible_new(wxk_Window* window, void* userdata) noexcept {
	return wxk::Guard<wxk_Accessible*>(nullptr, [&] {
		wxWindow* owner = wxk::Unwrap(window);
		auto* accessible = new ManagedAccessible(owner, userdata);
		// The window owns its accessible object, and deletes the one it had.
		owner->SetAccessible(accessible);
		return reinterpret_cast<wxk_Accessible*>(accessible);
	});
}

WXK_FUNCTION(void) wxk_Accessible_NotifyEvent(int32_t eventType, wxk_Window* window, int32_t objectType, int32_t objectId) noexcept {
	wxk::Guard([&] { wxAccessible::NotifyEvent(eventType, wxk::Unwrap(window), static_cast<wxAccObject>(objectType), objectId); });
}
