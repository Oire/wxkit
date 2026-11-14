using WxKit.Generator.Binding;

namespace WxKit.Generator.Tests;

public class NamingTests {
	[Theory]
	[InlineData("wxButton", "Button")]
	[InlineData("wxColourDialog", "ColorDialog")]
	[InlineData("wxCentreDialog", "CenterDialog")]
	public void DropsThePrefixAndRespellsTypes(string cpp, string managed) => Assert.Equal(managed, Naming.Type(cpp));

	[Theory]
	[InlineData("Centre", "Center")]
	[InlineData("SetBackgroundColour", "SetBackgroundColor")]
	[InlineData("GetLabel", "GetLabel")]
	public void RespellsMembersByWholeWord(string cpp, string managed) => Assert.Equal(managed, Naming.Member(cpp));

	[Theory]
	[InlineData("pos", "position")]
	[InlineData("orient", "orientation")]
	[InlineData("label", "label")]
	[InlineData("event", "@event")]
	public void RenamesParameters(string cpp, string managed) => Assert.Equal(managed, Naming.Parameter(cpp));

	[Theory]
	[InlineData("wxEVT_BUTTON", "Button")]
	[InlineData("wxEVT_CLOSE_WINDOW", "CloseWindow")]
	public void NamesEventTypes(string constant, string name) => Assert.Equal(name, Naming.EventType(constant));

	[Theory]
	[InlineData("CLOSE_WINDOW", "CloseWindow")]
	[InlineData("Layout_RightToLeft", "LayoutRightToLeft")]
	[InlineData("SHOW_EFFECT_NONE", "ShowEffectNone")]
	public void JoinsWordsInPascalCase(string upper, string pascal) => Assert.Equal(pascal, Naming.Words(upper));

	[Fact]
	public void FindsThePrefixMostStylesShare() {
		Assert.Equal("TE", Naming.SharedStylePrefix(["wxTE_MULTILINE", "wxTE_READONLY", "wxHSCROLL"]));
		Assert.Null(Naming.SharedStylePrefix(["wxOK", "wxCANCEL", "wxICON_ERROR", "wxICON_WARNING", "wxYES_NO", "wxCENTRE"]));
	}

	[Theory]
	[InlineData("wxTE_MULTILINE", "TE", "wxTextCtrl", "MultiLine")]
	[InlineData("wxDEFAULT_FRAME_STYLE", null, "wxFrame", "Default")]
	[InlineData("wxFRAME_TOOL_WINDOW", null, "wxFrame", "ToolWindow")]
	[InlineData("wxICON_ERROR", null, "wxMessageDialog", "IconError")]
	[InlineData("wxCENTRE", null, "wxMessageDialog", "Center")]
	public void NamesStyles(string constant, string? prefix, string className, string name) => Assert.Equal(name, Naming.Style(constant, prefix, className));
}
