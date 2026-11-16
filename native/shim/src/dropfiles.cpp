#include "shim.h"

WXK_FUNCTION(wxk_StringArray*) wxk_DropFilesEvent_GetFiles(wxk_Event* event) noexcept {
	return wxk::Guard<wxk_StringArray*>(nullptr, [&]() -> wxk_StringArray* {
		auto* dropped = static_cast<wxDropFilesEvent*>(wxk::Unwrap(event));
		auto* files = new wxArrayString;
		for (int index = 0; index < dropped->GetNumberOfFiles(); index++) files->push_back(dropped->GetFiles()[index]);
		return wxk::Wrap(files);
	});
}
