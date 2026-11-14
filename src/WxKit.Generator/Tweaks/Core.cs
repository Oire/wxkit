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
			// WxKit tracks the sizer a window owns, and gives the native handle as NativeHandle.
			.Ignore("SetSizer", "SetSizerAndFit", "GetSizer", "GetHandle")
			.Property("IsEnabled", "IsEnabled", "Enable")
			.ParameterEnum("Centre", "direction", "Orientation"));
		tweaks.Class("wxTopLevelWindow", window => window
			.NoConstructors()
			// Only wxUniversal has these, though the documentation does not mark them for one port.
			.Ignore("UseNativeDecorations", "UseNativeDecorationsByDefault", "IsUsingNativeDecorations")
			.AddEvent("EVT_CLOSE", "wxEVT_CLOSE_WINDOW", "wxCloseEvent", "Closing"));
		// WxKit keeps the menu bar a frame owns.
		tweaks.Class("wxFrame", frame => frame.Ignore("SetMenuBar", "GetMenuBar"));
		tweaks.Class("wxDialog");
		tweaks.Class("wxMessageDialog");
		tweaks.Class("wxPanel");
		tweaks.Class("wxButton", button => button.Event("EVT_BUTTON", "Click"));
		tweaks.Class("wxStaticText");
		tweaks.Class("wxTextCtrl", text => text.Event("EVT_TEXT", "TextChanged"));
		// wxMenuBar's Enable and IsEnabled take an item's id, so they would hide Window.IsEnabled.
		tweaks.Class("wxMenuBar", menuBar => menuBar.Rename("IsEnabled", "IsItemEnabled").Rename("Enable", "EnableItem"));
		tweaks.Class("wxBoxSizer", sizer => sizer.ParameterEnum("wxBoxSizer", "orient", "Orientation"));

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
		]) {
			tweaks.Class(eventClass);
		}

		// Bound by hand-written classes: menu items' clicks and timers' ticks.
		tweaks.EventType("wxEVT_MENU", "wxCommandEvent");
		tweaks.EventType("wxEVT_TIMER", "wxTimerEvent");
	}
}
