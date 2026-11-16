#include "shim.h"

// wxWidgets's own helpers for the parts of its generic controls, which it implements for macOS
// and GTK 3, and which do nothing elsewhere.
#include <wx/private/access.h>

WXK_FUNCTION(void) wxk_Window_SetAccessibleParts(wxk_Window* window, const wxk_StringArray* labels, const wxk_Rect* rects) noexcept {
	wxk::Guard([&] {
		const wxArrayString& texts = *wxk::Unwrap(const_cast<wxk_StringArray*>(labels));
		wxPrivate::AccessibleElements elements;
		elements.reserve(texts.size());
		for (size_t index = 0; index < texts.size(); index++) elements.emplace_back(texts[index], wxk::FromRect(rects[index]));
		wxPrivate::SetAccessibleElements(wxk::Unwrap(window), elements);
	});
}

WXK_FUNCTION(void) wxk_Window_SetAccessibleRows(wxk_Window* window, const wxk_AccessibleRow* rows, size_t count, const wxk_StringArray* cellLabels, const wxk_Rect* cellRects, int64_t total) noexcept {
	wxk::Guard([&] {
		const wxArrayString& labels = *wxk::Unwrap(const_cast<wxk_StringArray*>(cellLabels));
		wxPrivate::AccessibleRows table;
		table.reserve(count);
		for (size_t index = 0; index < count; index++) {
			const wxk_AccessibleRow& row = rows[index];
			wxPrivate::AccessibleRow& added = table.emplace_back(static_cast<long>(row.index), wxk::FromRect(row.rect));
			added.selected = row.selected != 0;
			for (int32_t cell = row.firstCell; cell < row.firstCell + row.cellCount; cell++) added.cells.emplace_back(labels[cell], wxk::FromRect(cellRects[cell]));
		}
		wxPrivate::SetAccessibleTable(wxk::Unwrap(window), table, static_cast<long>(total));
	});
}

WXK_FUNCTION(void) wxk_Window_SetAccessibleCurrentRow(wxk_Window* window, int64_t row) noexcept {
	wxk::Guard([&] { wxPrivate::SetAccessibleCurrentRow(wxk::Unwrap(window), static_cast<long>(row)); });
}
