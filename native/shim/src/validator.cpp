#include "shim.h"

#include <memory>

namespace {

// What a managed validator's native copies share: the callbacks into .NET, and a weak handle to
// the managed object. Only the clones a window holds call into .NET, so they keep it alive: the
// first one asks .NET for a strong handle, and the last one to go lets it go. The original, which
// the managed object owns, does not, so the managed object can still be collected when no window
// uses it.
struct ValidatorState {
	wxk_ValidateCallback validate;
	wxk_TransferCallback toWindow;
	wxk_TransferCallback fromWindow;
	wxk_Callback retain;
	wxk_Callback release;
	wxk_Callback onFree;
	void* userdata;

	ValidatorState(wxk_ValidateCallback validate, wxk_TransferCallback toWindow, wxk_TransferCallback fromWindow, wxk_Callback retain, wxk_Callback release, wxk_Callback onFree, void* userdata)
		: validate(validate), toWindow(toWindow), fromWindow(fromWindow), retain(retain), release(release), onFree(onFree), userdata(userdata) {}

	// Only the last copy's owner may tell .NET the state is gone, so the state itself is never copied.
	ValidatorState(const ValidatorState&) = delete;
	ValidatorState& operator=(const ValidatorState&) = delete;

	~ValidatorState() {
		onFree(userdata);
	}
};

class ManagedValidator final : public wxValidator {
public:
	explicit ManagedValidator(std::shared_ptr<ValidatorState> state) : _state(std::move(state)), _isClone(false) {}

	ManagedValidator(const ManagedValidator& other) : wxValidator(other), _state(other._state), _isClone(true) {
		_state->retain(_state->userdata);
	}

	~ManagedValidator() override {
		if (_isClone) _state->release(_state->userdata);
	}

	wxObject* Clone() const override {
		return new ManagedValidator(*this);
	}

	bool Validate(wxWindow* parent) override {
		return _state->validate(_state->userdata, wxk::Wrap(GetWindow()), wxk::Wrap(parent)) != 0;
	}

	bool TransferToWindow() override {
		return _state->toWindow(_state->userdata, wxk::Wrap(GetWindow())) != 0;
	}

	bool TransferFromWindow() override {
		return _state->fromWindow(_state->userdata, wxk::Wrap(GetWindow())) != 0;
	}

private:
	std::shared_ptr<ValidatorState> _state;
	bool _isClone;
};

} // namespace

WXK_FUNCTION(wxk_Object*) wxk_ManagedValidator_new(wxk_ValidateCallback validate, wxk_TransferCallback toWindow, wxk_TransferCallback fromWindow, wxk_Callback retain, wxk_Callback release, wxk_Callback onFree, void* userdata) noexcept {
	return wxk::Guard<wxk_Object*>(nullptr, [&] {
		auto state = std::make_shared<ValidatorState>(validate, toWindow, fromWindow, retain, release, onFree, userdata);
		wxObject* validator = new ManagedValidator(std::move(state));
		return wxk::Wrap(validator);
	});
}

WXK_FUNCTION(void) wxk_Window_SetValidator(wxk_Window* window, wxk_Object* validator) noexcept {
	// A window takes a clone of the validator. wxDefaultValidator clones to nothing, which removes it.
	wxk::Guard([&] { wxk::Unwrap(window)->SetValidator(validator != nullptr ? *static_cast<const wxValidator*>(wxk::Unwrap(validator)) : wxDefaultValidator); });
}
