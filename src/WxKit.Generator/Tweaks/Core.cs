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
			.ParameterEnum("Move", "flags", "SizeFlags")
			.ParameterEnum("SetSize", "sizeFlags", "SizeFlags")
			.ParameterEnum("SendSizeEvent", "flags", "SendSizeEventFlags")
			.ParameterEnum("SendSizeEventToParent", "flags", "SendSizeEventFlags")
			.ParameterEnum("UpdateWindowUI", "flags", "wxUpdateUI")
			// wxGTK has no system-wide hot keys.
			.Guard("wxUSE_HOTKEY", "RegisterHotKey", "UnregisterHotKey"));
		tweaks.Class("wxTopLevelWindow", window => window
			.NoConstructors()
			// Only wxUniversal has these, though the documentation does not mark them for one port.
			.Ignore("UseNativeDecorations", "UseNativeDecorationsByDefault", "IsUsingNativeDecorations")
			.Guard("defined(__WXMSW__)", "MSWGetSystemMenu")
			.ParameterEnum("ShowFullScreen", "style", "FullScreenStyle")
			.ParameterEnum("EnableFullScreenView", "style", "FullScreenStyle")
			.ParameterEnum("RequestUserAttention", "flags", "UserAttention")
			.AddEvent("EVT_CLOSE", "wxEVT_CLOSE_WINDOW", "wxCloseEvent", "Closing"));
		// WxKit keeps the menu bar a frame owns.
		// The flags these take, which wxWidgets declares as constants rather than a named enum.
		tweaks.ConstantEnum("SizeFlags", "wxSIZE_", isFlags: true, excludePrefixes: ["wxSIZE_CONV_", "wxSIZE_SET_"]);
		tweaks.ConstantEnum("SendSizeEventFlags", "wxSEND_EVENT_", isFlags: true);
		tweaks.ConstantEnum("FullScreenStyle", "wxFULLSCREEN_", isFlags: true);
		tweaks.ConstantEnum("UserAttention", "wxUSER_ATTENTION_");
		tweaks.Enum("wxUpdateUI", "UpdateUIFlags", isFlags: true);
		// WxKit keeps the menu bar a frame owns. OnCreateStatusBar and OnCreateToolBar are what
		// CreateStatusBar and CreateToolBar call, for a derived frame to override, not to call.
		tweaks.Class("wxFrame", frame => frame
			.ParameterName("SetStatusWidths", "widths_field", "widths")
			.Ignore("SetMenuBar", "GetMenuBar", "OnCreateStatusBar", "OnCreateToolBar")
			.ParameterEnum("CreateToolBar", "style", "ToolBarStyle")
			.ParameterEnum("CreateStatusBar", "style", "StatusBarStyle"));
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
		// The color dialog, and the event wxMSW sends as the color in it changes. Events are only
		// received, so the event's color is only read.
		// ToString and FromString write and read the custom colors, which would hide object.ToString.
		tweaks.Class("wxColourData", data => data.ValueObject().Ignore("ToString", "FromString"));
		tweaks.Class("wxColourDialog", dialog => dialog.AddEvent("EVT_COLOUR_CHANGED", "wxEVT_COLOUR_CHANGED", "wxColourDialogEvent", "ColorChanged"));
		tweaks.Class("wxColourDialogEvent", e => e.Ignore("SetColour"));
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
			"wxStaticBitmap", "wxBitmapButton", "wxBitmapToggleButton", "wxCommandLinkButton",
			"wxListbook", "wxChoicebook", "wxTreebook", "wxToolbook", "wxSimplebook",
			"wxInfoBar", "wxActivityIndicator", "wxBitmapComboBox", "wxRearrangeList", "wxRearrangeCtrl",
			"wxBannerWindow", "wxMiniFrame", "wxRearrangeDialog",
		]) {
			tweaks.Class(control);
		}
		// An item's data is an object, which the control deletes with the item. Detaching it would
		// leave no one to free it.
		foreach (string container in (string[])["wxControlWithItems", "wxChoice", "wxComboBox", "wxListBox"]) tweaks.Class(container).Ignore("DetachClientObject");
		tweaks.Class("wxStatusBar", bar => bar.ParameterName("SetStatusWidths", "widths_field", "widths"));
		// Pickers: a button, and optionally a text box, that choose a color, a file, a folder, or a
		// font. Events are only received, so the events' values are only read.
		// CreateBase is for a picker's own Create.
		tweaks.Class("wxPickerBase", picker => picker.Ignore("CreateBase"));
		tweaks.Class("wxColourPickerCtrl", picker => picker
			.StylesPrefixed("CLRP")
			.Event("EVT_COLOURPICKER_CHANGED", "ColorChanged")
			.Event("EVT_COLOURPICKER_CURRENT_CHANGED", "CurrentColorChanged")
			.Event("EVT_COLOURPICKER_DIALOG_CANCELLED", "DialogCancelled"));
		tweaks.Class("wxFilePickerCtrl", picker => picker
			.Event("EVT_FILEPICKER_CHANGED", "PathChanged")
			.ParameterName("SetInitialDirectory", "dir", "directory"));
		tweaks.Class("wxDirPickerCtrl", picker => picker
			.StylesPrefixed("DIRP")
			.Event("EVT_DIRPICKER_CHANGED", "PathChanged")
			.ParameterName("SetInitialDirectory", "dir", "directory"));
		tweaks.Class("wxFontPickerCtrl", picker => picker
			.StylesPrefixed("FNTP")
			.Event("EVT_FONTPICKER_CHANGED", "FontChanged"));
		tweaks.Class("wxColourPickerEvent", e => e.Ignore("SetColour"));
		tweaks.Class("wxFileDirPickerEvent", e => e.Ignore("SetPath"));
		tweaks.Class("wxFontPickerEvent", e => e.Ignore("SetFont"));
		// A panel that scrolls its contents, such as a long page of settings.
		tweaks.TemplateInstance("wxScrolled", "wxScrolledWindow", "wxPanel");
		// OnDraw is for a derived class to override, and DoPrepareReadOnlyDC takes the wxReadOnlyDC
		// wxWidgets 3.3 added, which is not bound.
		tweaks.Class("wxScrolledWindow", scrolled => scrolled
			.Ignore("OnDraw", "DoPrepareReadOnlyDC")
			.ParameterEnum("GetScrollLines", "orient", "Orientation")
			.ParameterEnum("GetScrollPageSize", "orient", "Orientation")
			.ParameterEnum("SetScrollPageSize", "orient", "Orientation")
			.ParameterName("SetScrollRate", "xstep", "xStep")
			.ParameterName("SetScrollRate", "ystep", "yStep"));
		// A bitmap button raises its base's Click. CreateCloseButton is the second step of a two-step creation.
		tweaks.Class("wxBitmapButton", button => button.Event("EVT_BUTTON", "Click").Ignore("CreateCloseButton"));
		tweaks.Class("wxStaticBitmap", bitmap => bitmap.ParameterName("wxStaticBitmap", "label", "bitmap"));
		tweaks.Class("wxTreebook", book => book.ParameterName("AddSubPage", "bSelect", "select").ParameterName("InsertSubPage", "bSelect", "select"));
		tweaks.Class("wxInfoBar", bar => bar
			.ParameterName("AddButton", "btnid", "buttonId")
			.ParameterName("HasButtonId", "btnid", "buttonId")
			.ParameterName("RemoveButton", "btnid", "buttonId")
			.ParameterName("ShowMessage", "msg", "message")
			.ParameterName("ShowMessage", "flags", "icon")
			.ParameterEnum("ShowMessage", "flags", "MessageIcon"));
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
		// Toolbars and their tools. A tool is only made by adding it, so the members that make or remove
		// a tool nobody owns are left out. Deleting tools is written by hand, since the toolbar marks
		// their objects destroyed. A tool's setters only change what the tool records, not what the
		// toolbar shows, so its state is written by hand, through the toolbar.
		tweaks.Class("wxToolBar").Ignore("CreateTool", "CreateSeparator", "RemoveTool", "DeleteTool", "DeleteToolByPos", "ClearTools", "GetToolClientData", "SetToolClientData");
		tweaks.Class("wxToolBarToolBase", tool => tool
			.Named("ToolBarTool")
			.NoConstructors()
			.Only("GetId", "GetControl", "GetKind", "GetLabel", "GetNormalBitmap", "GetDisabledBitmap", "IsButton", "IsControl", "IsSeparator", "IsStretchableSpace", "CanBeToggled"));
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
			.ParameterEnum("SetSingleStyle", "style", "ListCtrlStyle")
			.ParameterEnum("SetWindowStyleFlag", "style", "ListCtrlStyle")
			.Director("OnGetItemText", "OnGetItemImage", "OnGetItemColumnImage", "OnGetItemIsChecked"));
		tweaks.Class("wxListView");
		// SetItemData is written by hand, since wxTreeCtrl does not delete the data it replaces.
		tweaks.Class("wxTreeCtrl", tree => tree.Ignore("SetItemData").ParameterName("GetSelections", "selection", "selections"));
		// Data views show a model, whose classes are hand-written, in DataViewModel. A column is made
		// by adding one, since renderers are not bound, so the members that add a column object are
		// left out. Deleting columns is written by hand, since the control marks their objects destroyed.
		tweaks.Enum("wxDataViewColumnFlags", "DataViewColumnFlags", isFlags: true);
		tweaks.Class("wxDataViewColumn", column => column
			.InheritUnboundBases()
			.NoConstructors()
			.Ignore("GetRenderer", "SetFlag", "ClearFlag", "ToggleFlag", "ChangeFlag", "HasFlag")
			.ParameterEnum("SetFlags", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("GetFlags", "return", "wxDataViewColumnFlags"));
		tweaks.Class("wxDataViewCtrl", view => view
			.ParameterName("GetSelections", "sel", "selections")
			.ParameterName("SetSelections", "sel", "selections")
			.DiscardUnboundReturns()
			.Ignore("AssociateModel", "GetModel", "AppendColumn", "PrependColumn", "InsertColumn", "DeleteColumn", "ClearColumns", "GetSortingColumns")
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
			.Ignore("GetStore", "AppendColumn", "PrependColumn", "InsertColumn")
			.ParameterEnum("AppendTextColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendToggleColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendProgressColumn", "flags", "wxDataViewColumnFlags")
			.ParameterEnum("AppendIconTextColumn", "flags", "wxDataViewColumnFlags"));
		// Its images are an image list, which is not bound.
		tweaks.Class("wxDataViewTreeCtrl", view => view.DiscardUnboundReturns().Ignore("GetStore", "GetImageList"));
		// The interface headers document SetItem, which the real wxDataViewEvent does not have. Events
		// are only received, so the column is only read.
		tweaks.Class("wxDataViewEvent", dataViewEvent => dataViewEvent.Ignore("SetItem", "SetDataViewColumn"));
		tweaks.Class("wxDataViewIconText").ValueObject();
		tweaks.Class("wxListItem").ValueObject();
		tweaks.Class("wxListEvent");
		tweaks.Class("wxTreeEvent");
		// Dates and times, which are DateTimes in local time. Events are only received, so their
		// dates are only read.
		tweaks.Class("wxDatePickerCtrl", picker => picker
			.ParameterName("wxDatePickerCtrl", "dt", "date")
			.ParameterName("SetRange", "dt1", "earliest")
			.ParameterName("SetRange", "dt2", "latest"));
		tweaks.Class("wxTimePickerCtrl", picker => picker
			.ParameterName("wxTimePickerCtrl", "dt", "time")
			.ParameterName("GetTime", "min", "minute")
			.ParameterName("GetTime", "sec", "second")
			.ParameterName("SetTime", "min", "minute")
			.ParameterName("SetTime", "sec", "second"));
		// The headers list the double click as EVT_CALENDAR, which sends wxEVT_CALENDAR_DOUBLECLICKED.
		tweaks.Class("wxCalendarCtrl", calendar => calendar
			.ParameterName("SetDateRange", "lowerdate", "earliest")
			.ParameterName("SetDateRange", "upperdate", "latest")
			.Event("EVT_CALENDAR_SEL_CHANGED", "SelectionChanged")
			.Event("EVT_CALENDAR_PAGE_CHANGED", "PageChanged")
			.Event("EVT_CALENDAR_WEEK_CLICKED", "WeekClicked")
			.Event("EVT_CALENDAR_WEEKDAY_CLICKED", "WeekDayClicked")
			.AddEvent("EVT_CALENDAR", "wxEVT_CALENDAR_DOUBLECLICKED", "wxCalendarEvent", "DateActivated"));
		tweaks.Class("wxDateEvent", e => e.Ignore("SetDate"));
		tweaks.Class("wxCalendarEvent", e => e.Ignore("SetWeekDay"));
		// Taskbar icons, which are made and freed by hand, since the shim's own class shows the menu
		// .NET gives it, and Destroy must only be called once, which Dispose sees to.
		tweaks.Enum("wxTaskBarIconType", "TaskBarIconType");
		tweaks.Class("wxTaskBarIcon", icon => icon
			.NoConstructors()
			.Ignore("Destroy")
			.Event("EVT_TASKBAR_MOVE", "MouseMoved")
			.Event("EVT_TASKBAR_LEFT_DOWN", "LeftDown")
			.Event("EVT_TASKBAR_LEFT_UP", "LeftUp")
			.Event("EVT_TASKBAR_RIGHT_DOWN", "RightDown")
			.Event("EVT_TASKBAR_RIGHT_UP", "RightUp")
			.Event("EVT_TASKBAR_LEFT_DCLICK", "LeftDoubleClick")
			.Event("EVT_TASKBAR_RIGHT_DCLICK", "RightDoubleClick")
			.Event("EVT_TASKBAR_CLICK", "Click"));
		tweaks.Class("wxTaskBarIconEvent");
		// Notifications, which the application frees with Dispose, as it does timers. Only wxMSW has toasts.
		// The icons a notification or an info bar shows, which are wxWidgets's message icons, without
		// the synonyms and the masks.
		tweaks.ConstantEnum("MessageIcon", "wxICON_", excludePrefixes: ["wxICON_EXCLAMATION", "wxICON_HAND", "wxICON_STOP", "wxICON_ASTERISK", "wxICON_MASK", "wxICON_AUTH_NEEDED", "wxICON_DEFAULT", "wxICON_SCREEN_"]);
		tweaks.Class("wxNotificationMessage", notification => notification
			.Guard("defined(__WXMSW__)", "MSWUseToasts")
			.ParameterEnum("wxNotificationMessage", "flags", "MessageIcon")
			.ParameterEnum("SetFlags", "flags", "MessageIcon")
			.ParameterName("wxNotificationMessage", "flags", "icon")
			.ParameterName("AddAction", "actionid", "actionId")
			.Event("EVT_NOTIFICATION_MESSAGE_CLICK", "Clicked")
			.Event("EVT_NOTIFICATION_MESSAGE_DISMISSED", "Dismissed")
			.Event("EVT_NOTIFICATION_MESSAGE_ACTION", "ActionSelected"));
		// A progress dialog's styles are constants, not a style table.
		tweaks.ConstantEnum("ProgressDialogStyle", "wxPD_", isFlags: true);
		tweaks.Class("wxProgressDialog").ParameterEnum("wxProgressDialog", "style", "ProgressDialogStyle");
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
		// An event does not delete the item data it is given, so only the item's data comes out.
		tweaks.Class("wxCommandEvent").Ignore("SetClientObject");
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
		// Files dropped on a window that accepts them. Files is written by hand, since GetFiles gives a C array.
		tweaks.Class("wxDropFilesEvent", e => e.Ignore("GetFiles"));
		// Bound by hand-written classes: menu items' clicks and timers' ticks.
		tweaks.EventType("wxEVT_MENU", "wxCommandEvent");
		tweaks.EventType("wxEVT_TIMER", "wxTimerEvent");
		// The headers give the color dialog's event the color picker's class, which it is not.
		tweaks.EventType("wxEVT_COLOUR_CHANGED", "wxColourDialogEvent");
	}
}
