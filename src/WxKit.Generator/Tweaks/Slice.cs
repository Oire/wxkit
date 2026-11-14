using WxKit.Generator.Binding;

namespace WxKit.Generator.Tweaks;

// The first milestone's slice: the classes and members the hand-written bindings covered, which
// the generator now produces. Each class lists its members, so the output stays exactly the slice
// while the generator grows. Classes marked hand-written are used as types only.
internal static class Slice {
	public static void Apply(Binding.Tweaks tweaks) {
		// Written by hand, because they track ownership or need the shim's own helpers.
		tweaks.Class("wxEvtHandler").Handwritten();
		tweaks.Class("wxSizer").Handwritten();
		tweaks.Class("wxMenu").Handwritten().Wrappable();

		tweaks.Class("wxWindow", window => window
			.NoConstructors()
			.Only("GetId", "GetParent", "GetLabel", "SetLabel", "Show", "Hide", "IsShown", "Enable", "IsEnabled", "SetFocus", "Close", "Destroy", "Fit", "Layout", "Centre")
			.Property("IsEnabled", "IsEnabled", "Enable")
			.ParameterEnum("Centre", "direction", "Orientation"));
		tweaks.Class("wxTopLevelWindow", window => window
			.NoConstructors()
			.Only("GetTitle", "SetTitle")
			.AddEvent("EVT_CLOSE", "wxEVT_CLOSE_WINDOW", "wxCloseEvent", "Closing"));
		tweaks.Class("wxFrame", frame => frame.Only());
		tweaks.Class("wxDialog", dialog => dialog.Only("ShowModal"));
		tweaks.Class("wxMessageDialog", dialog => dialog.Only());
		tweaks.Class("wxPanel", panel => panel.Only());
		tweaks.Class("wxButton", button => button.Only().Event("EVT_BUTTON", "Click"));
		tweaks.Class("wxStaticText", text => text.Only());
		tweaks.Class("wxTextCtrl", text => text
			.Only("GetValue", "SetValue", "ChangeValue")
			.Event("EVT_TEXT", "TextChanged"));
		tweaks.Class("wxMenuBar", menuBar => menuBar.Only("Append"));
		tweaks.Class("wxBoxSizer", sizer => sizer.Only().ParameterEnum("wxBoxSizer", "orient", "Orientation"));

		tweaks.Class("wxEvent", e => e
			.NoConstructors()
			.Only("GetId", "GetEventObject", "Skip", "GetSkipped")
			.Rename("GetSkipped", "IsSkipped"));
		tweaks.Class("wxCommandEvent", e => e.Only("GetString", "GetInt", "GetSelection", "IsChecked"));
		// Veto is written by hand, since it refuses when the close cannot be vetoed.
		tweaks.Class("wxCloseEvent", e => e.Only("CanVeto", "GetVeto").Rename("GetVeto", "IsVetoed"));

		// Bound by hand-written classes: menu items' clicks and timers' ticks.
		tweaks.EventType("wxEVT_MENU", "wxCommandEvent");
		tweaks.EventType("wxEVT_TIMER", "wxTimerEvent");
	}
}
