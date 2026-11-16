#include "shim.h"

#include <wx/uiaction.h>

namespace {

// One simulator serves every call, as it keeps no state between them that matters here.
wxUIActionSimulator& Simulator() {
	static wxUIActionSimulator simulator;
	return simulator;
}

uint8_t Send(bool sent) {
	return sent ? 1 : 0;
}

}

WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_MouseMove(wxk_Point point) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().MouseMove(wxk::FromPoint(point))); });
}

WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_MouseDown(int32_t button) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().MouseDown(button)); });
}

WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_MouseUp(int32_t button) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().MouseUp(button)); });
}

WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_MouseClick(int32_t button) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().MouseClick(button)); });
}

WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_MouseDoubleClick(int32_t button) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().MouseDblClick(button)); });
}

WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_KeyDown(int32_t key, int32_t modifiers) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().KeyDown(key, modifiers)); });
}

WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_KeyUp(int32_t key, int32_t modifiers) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().KeyUp(key, modifiers)); });
}

WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_Char(int32_t key, int32_t modifiers) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().Char(key, modifiers)); });
}

// wxUIActionSimulator::Text takes UTF-8, and types only what a US keyboard has keys for.
WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_Text(const char16_t* text, size_t textLength) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().Text(wxk::FromUtf16(text, textLength).utf8_str())); });
}

WXK_FUNCTION(uint8_t) wxk_UIActionSimulator_Select(const char16_t* text, size_t textLength) noexcept {
	return wxk::Guard<uint8_t>(0, [&]() -> uint8_t { return Send(Simulator().Select(wxk::FromUtf16(text, textLength))); });
}
