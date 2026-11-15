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
			.ParameterEnum("Centre", "direction", "Orientation")
			// wxGTK has no system-wide hot keys.
			.Guard("wxUSE_HOTKEY", "RegisterHotKey", "UnregisterHotKey"));
		tweaks.Class("wxTopLevelWindow", window => window
			.NoConstructors()
			// Only wxUniversal has these, though the documentation does not mark them for one port.
			.Ignore("UseNativeDecorations", "UseNativeDecorationsByDefault", "IsUsingNativeDecorations")
			.Guard("defined(__WXMSW__)", "MSWGetSystemMenu")
			.AddEvent("EVT_CLOSE", "wxEVT_CLOSE_WINDOW", "wxCloseEvent", "Closing"));
		// WxKit keeps the menu bar a frame owns.
		tweaks.Class("wxFrame", frame => frame.Ignore("SetMenuBar", "GetMenuBar").ParameterEnum("CreateToolBar", "style", "ToolBarStyle"));
		// wxMSW's wxDialog has no GetToolBar, though the documentation gives it one.
		tweaks.Class("wxDialog").Ignore("GetToolBar");
		tweaks.Class("wxMessageDialog");
		// The find dialog reads and writes its data for as long as it exists, so the data is
		// WxKit's, and the dialog keeps it alive. GetData would wrap the same data a second time.
		// The dialog sends its events to itself, and on to its parent.
		tweaks.Enum("wxFindReplaceFlags", "FindReplaceFlags", isFlags: true);
		tweaks.Enum("wxFindReplaceDialogStyles", "FindReplaceDialogStyle", isFlags: true);
		tweaks.Class("wxFindReplaceData", data => data
			.Owned()
			.ParameterEnum("wxFindReplaceData", "flags", "wxFindReplaceFlags")
			.ParameterEnum("SetFlags", "flags", "wxFindReplaceFlags")
			.ParameterEnum("GetFlags", "return", "wxFindReplaceFlags"));
		tweaks.Class("wxFindReplaceDialog", dialog => dialog
			.KeepAlive("data")
			.Ignore("GetData")
			.ParameterEnum("wxFindReplaceDialog", "style", "wxFindReplaceDialogStyles")
			.AddEvent("EVT_FIND", "wxEVT_FIND", "wxFindDialogEvent", "Find")
			.AddEvent("EVT_FIND_NEXT", "wxEVT_FIND_NEXT", "wxFindDialogEvent", "FindNext")
			.AddEvent("EVT_FIND_REPLACE", "wxEVT_FIND_REPLACE", "wxFindDialogEvent", "Replace")
			.AddEvent("EVT_FIND_REPLACE_ALL", "wxEVT_FIND_REPLACE_ALL", "wxFindDialogEvent", "ReplaceAll")
			.AddEvent("EVT_FIND_CLOSE", "wxEVT_FIND_CLOSE", "wxFindDialogEvent", "FindClose"));
		tweaks.Class("wxFindDialogEvent", findEvent => findEvent.ParameterEnum("GetFlags", "return", "wxFindReplaceFlags"));
		// The font dialog copies the data it is given, and gives a copy back.
		tweaks.Class("wxFontData", data => data.ValueObject().Property("EnableEffects", "GetEnableEffects", "EnableEffects").Property("RestrictSelection", "GetRestrictSelection", "RestrictSelection"));
		tweaks.Class("wxFontDialog");
		tweaks.Class("wxPanel");
		tweaks.Class("wxButton", button => button.Event("EVT_BUTTON", "Click"));
		tweaks.Class("wxStaticText");
		// Spell checking needs gspell on Linux, which WxKit leaves out for now, so there it does nothing.
		tweaks.Class("wxTextCtrl", text => text.Event("EVT_TEXT", "TextChanged").Guard("wxUSE_SPELLCHECK", "EnableProofCheck"));
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
		// wxOSX has no static box with a window as its label.
		tweaks.Class("wxStaticBox").GuardOverload("defined(wxHAS_WINDOW_LABEL_IN_STATIC_BOX)", "wxStaticBox", "label", "wxWindow *");
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
		// The standard ids, which give a button or menu item the platform's label and shortcut. The
		// bounds of the ids wxWidgets assigns itself, and those of its own file controls, are its own.
		tweaks.ConstantClass("wxStandardID", "StandardId", "wxID_AUTO_LOWEST", "wxID_AUTO_HIGHEST", "wxID_LOWEST", "wxID_HIGHEST", "wxID_FILEDLGG", "wxID_FILECTRL");
		// Only wxMSW declares them, under wxUSE_ACCESSIBILITY.
		const string accessibility = "wxUSE_ACCESSIBILITY";
		tweaks.Enum("wxAccRole", "AccessibleRole", memberPrefix: "wxROLE_SYSTEM_", guard: accessibility);
		tweaks.Enum("wxAccObject", "AccessibleObject", guard: accessibility);
		tweaks.Enum("wxNavDir", "NavigationDirection", guard: accessibility);
		tweaks.Enum("wxAccSelectionFlags", "AccessibleSelection", isFlags: true, guard: accessibility);
		tweaks.ConstantEnum("AccessibleStates", "wxACC_STATE_SYSTEM_", isFlags: true, guard: accessibility);
		tweaks.ConstantEnum("AccessibleEvent", "wxACC_EVENT_", guard: accessibility);
		// A virtual list asks for each row as it shows it, through members C# overrides.
		tweaks.Class("wxListCtrl", list => list
			.ParameterEnum("InsertColumn", "format", "wxListColumnFormat")
			.Director("OnGetItemText", "OnGetItemImage", "OnGetItemColumnImage", "OnGetItemIsChecked"));
		tweaks.Class("wxListView");
		tweaks.Class("wxTreeCtrl");
		// Data views show a model, whose classes are hand-written, in DataViewModel. Adding a column
		// returns the column, which is not bound, so it gives nothing back; its model column names it.
		tweaks.Enum("wxDataViewColumnFlags", "DataViewColumnFlags", isFlags: true);
		tweaks.Class("wxDataViewCtrl", view => view
			.DiscardUnboundReturns()
			.Ignore("AssociateModel", "GetModel")
			.ParameterEnum("AppendTextColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendToggleColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendProgressColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendIconTextColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendBitmapColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendDateColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("PrependTextColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("PrependToggleColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("PrependProgressColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("PrependIconTextColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("PrependBitmapColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("PrependDateColumn", "flags", "wxDataViewColumnFlags"));
		tweaks.Class("wxDataViewListCtrl", view => view
			.DiscardUnboundReturns()
			.Ignore("GetStore")
			.ParameterEnum("AppendTextColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendToggleColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendProgressColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendIconTextColumn", "flags", "wxDataViewColumnFlags"));
		tweaks.Class("wxDataViewTreeCtrl", view => view.DiscardUnboundReturns().Ignore("GetStore"));
		// The interface headers document SetItem, which the real wxDataViewEvent does not have.
		tweaks.Class("wxDataViewEvent", dataViewEvent => dataViewEvent.Ignore("SetItem"));
		tweaks.Class("wxDataViewIconText").ValueObject();
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
		tweaks.Class("wxFont").Rename("Underlined", "WithUnderline").Rename("Strikethrough", "WithStrikethrough").Guard("wxUSE_PRIVATE_FONTS", "AddPrivateFont");
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
