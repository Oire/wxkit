using WxKit.Generator.Binding;

namespace WxKit.Generator.Tweaks;

// The core classes: windows, the basic controls, sizers, menus, and the events they send. Every
// member the generator can convert is bound, except the ones listed here, which are written by hand.
internal static class Core {
	public static void Apply(Binding.Tweaks tweaks) {
		// Written by hand, because they track ownership or need the shim's own helpers.
		tweaks.Class("wxEvtHandler").Handwritten();
		tweaks.Class("wxSizer").Handwritten();
		tweaks.Class("wxMenu").Handwritten().Wrappable();
		tweaks.Class("wxWindow", window => window
			.NoConstructors()
			// WxKit tracks the sizer a window owns, gives the native handle as NativeHandle, keeps
			// the validator it was given, since the window holds a copy, and gives a window its
			// accessible object through the hand-written Accessible.
			.Ignore("SetSizer", "SetSizerAndFit", "GetSizer", "GetHandle", "SetValidator", "GetValidator", "GetAccessible", "SetAccessible", "CreateAccessible", "GetOrCreateAccessible")
			.Property("IsEnabled", "IsEnabled", "Enable")
			.ParameterEnum("Centre", "direction", "Orientation"));
		tweaks.Class("wxTopLevelWindow", window => window
			.NoConstructors()
			// Only wxUniversal has these, though the documentation does not mark them for one port.
			.Ignore("UseNativeDecorations", "UseNativeDecorationsByDefault", "IsUsingNativeDecorations")
			.AddEvent("EVT_CLOSE", "wxEVT_CLOSE_WINDOW", "wxCloseEvent", "Closing"));
		// WxKit keeps the menu bar a frame owns.
		tweaks.Class("wxFrame", frame => frame.Ignore("SetMenuBar", "GetMenuBar").ParameterEnum("CreateToolBar", "style", "ToolBarStyle"));
		// wxMSW's wxDialog has no GetToolBar, though the documentation gives it one.
		tweaks.Class("wxDialog").Ignore("GetToolBar");
		tweaks.Class("wxMessageDialog");
		tweaks.Class("wxPanel");
		tweaks.Class("wxButton", button => button.Event("EVT_BUTTON", "Click"));
		tweaks.Class("wxStaticText");
		tweaks.Class("wxTextCtrl", text => text.Event("EVT_TEXT", "TextChanged"));
		// wxMenuBar's Enable and IsEnabled take an item's id, so they would hide Window.IsEnabled.
		tweaks.Class("wxMenuBar", menuBar => menuBar.Rename("IsEnabled", "IsItemEnabled").Rename("Enable", "EnableItem"));
		tweaks.Class("wxBoxSizer", sizer => sizer.ParameterEnum("wxBoxSizer", "orient", "Orientation"));
		tweaks.Class("wxStaticBoxSizer", sizer => sizer.ParameterEnum("wxStaticBoxSizer", "orient", "Orientation"));
		tweaks.Class("wxGridSizer");
		tweaks.Class("wxFlexGridSizer");
		tweaks.Class("wxStdDialogButtonSizer");
		// The controls most applications use.
		foreach (string control in (string[])[
			"wxControl", "wxAnyButton", "wxControlWithItems", "wxCheckBox", "wxRadioButton", "wxRadioBox",
			"wxChoice", "wxComboBox", "wxListBox", "wxCheckListBox", "wxSpinCtrl", "wxSpinCtrlDouble",
			"wxSpinButton", "wxSlider", "wxGauge", "wxStaticBox", "wxStaticLine", "wxToggleButton",
			"wxHyperlinkCtrl", "wxScrollBar", "wxCollapsiblePane", "wxSearchCtrl", "wxStatusBar",
			"wxBookCtrlBase", "wxNotebook", "wxSplitterWindow",
		]) {
			tweaks.Class(control);
		}
		// Validators, which WxKit creates and windows copy. Custom ones are hand-written, in CustomValidator.
		tweaks.Class("wxValidator", validator => validator.Owned().NoConstructors().Ignore("SetWindow", "Clone"));
		tweaks.Class("wxTextValidator", validator => validator
			.Owned()
			.Ignore("Clone", "OnChar")
			.ParameterEnum("wxTextValidator", "style", "wxTextValidatorStyle")
			.ParameterEnum("SetStyle", "style", "wxTextValidatorStyle")
			.ParameterEnum("GetStyle", "return", "wxTextValidatorStyle"));
		// Toolbars. The tools they return are not bound, so adding one gives nothing back; its id names it.
		tweaks.Class("wxToolBar").DiscardUnboundReturns();
		// Lists and trees, and what they hold.
		// Accessibility, for the hand-written Accessible, which describes a custom control to screen readers.
		tweaks.Enum("wxAccRole", "AccessibleRole", memberPrefix: "wxROLE_SYSTEM_");
		tweaks.Enum("wxAccObject", "AccessibleObject");
		tweaks.Enum("wxNavDir", "NavigationDirection");
		tweaks.Enum("wxAccSelectionFlags", "AccessibleSelection", isFlags: true);
		tweaks.ConstantEnum("AccessibleStates", "wxACC_STATE_SYSTEM_", isFlags: true);
		tweaks.ConstantEnum("AccessibleEvent", "wxACC_EVENT_");
		// A virtual list asks for each row as it shows it, through members C# overrides.
		tweaks.Class("wxListCtrl", list => list
			.ParameterEnum("InsertColumn", "format", "wxListColumnFormat")
			.Director("OnGetItemText", "OnGetItemImage", "OnGetItemColumnImage", "OnGetItemIsChecked"));
		tweaks.Class("wxListView");
		tweaks.Class("wxTreeCtrl");
		tweaks.Class("wxListItem").ValueObject();
		tweaks.Class("wxListEvent");
		tweaks.Class("wxTreeEvent");
		// IsEmpty is ambiguous on purpose, between the list and the text. IsListEmpty and IsTextEmpty say which.
		tweaks.Class("wxComboBox").Ignore("IsEmpty");
		// Fonts, pens, brushes, bitmaps, icons, cursors, and images, which are copied like values.
		foreach (string value in (string[])["wxFont", "wxPen", "wxBrush", "wxBitmap", "wxIcon", "wxCursor", "wxImage"]) {
			tweaks.Class(value);
		}
		// Size returns a resized copy, beside the Size property, as Resize resizes the image itself.
		tweaks.Class("wxImage").Rename("Size", "Resized");
		// wxMSW's wxIcon has no ConvertToDisabled, though the documentation gives it one.
		tweaks.Class("wxIcon").Ignore("ConvertToDisabled");
		// Underlined and Strikethrough return changed copies, beside the properties of the same names.
		tweaks.Class("wxFont").Rename("Underlined", "WithUnderline").Rename("Strikethrough", "WithStrikethrough");
		// Drawing contexts.
		foreach (string dc in (string[])["wxDC", "wxWindowDC", "wxClientDC", "wxPaintDC", "wxMemoryDC", "wxScreenDC"]) {
			tweaks.Class(dc);
		}
		// The system's colors, fonts, and metrics, and the standard icons.
		tweaks.Class("wxSystemSettings").StaticOnly();
		tweaks.Class("wxArtProvider").StaticOnly();
		// The standard dialogs.
		foreach (string dialog in (string[])["wxTextEntryDialog", "wxPasswordEntryDialog", "wxFileDialog", "wxDirDialog", "wxProgressDialog", "wxSingleChoiceDialog", "wxMultiChoiceDialog"]) {
			tweaks.Class(dialog);
		}
		tweaks.Class("wxEvent", e => e.NoConstructors().Rename("GetSkipped", "IsSkipped"));
		tweaks.Class("wxCommandEvent");
		// Veto is written by hand, since it refuses when the close cannot be vetoed.
		tweaks.Class("wxCloseEvent", e => e.Ignore("Veto").Rename("GetVeto", "IsVetoed"));
		// Button(button) asks whether the event came from that button. GetButton is the Button
		// property, and IsButton is taken too.
		tweaks.Class("wxMouseEvent", e => e.Rename("Button", "IsFromButton"));
		foreach (string eventClass in (string[])[
			"wxNotifyEvent", "wxKeyEvent", "wxSizeEvent", "wxMoveEvent", "wxFocusEvent",
			"wxChildFocusEvent", "wxPaintEvent", "wxShowEvent", "wxIconizeEvent", "wxMaximizeEvent",
			"wxActivateEvent", "wxIdleEvent", "wxUpdateUIEvent", "wxContextMenuEvent", "wxHelpEvent",
			"wxSysColourChangedEvent", "wxDPIChangedEvent", "wxInitDialogEvent", "wxMenuEvent",
			"wxFullScreenEvent", "wxNavigationKeyEvent", "wxTimerEvent", "wxScrollWinEvent", "wxEraseEvent",
			"wxSpinEvent", "wxSpinDoubleEvent", "wxScrollEvent", "wxHyperlinkEvent", "wxBookCtrlEvent",
			"wxSplitterEvent", "wxCollapsiblePaneEvent",
		]) {
			tweaks.Class(eventClass);
		}
		// Bound by hand-written classes: menu items' clicks and timers' ticks.
		tweaks.EventType("wxEVT_MENU", "wxCommandEvent");
		tweaks.EventType("wxEVT_TIMER", "wxTimerEvent");
	}
}
