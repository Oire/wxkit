using WxKit.Generator.Binding;

namespace WxKit.Generator.Tests;

public class ConstantsTests {
	private static readonly Constants Values = new(new Dictionary<string, string> {
		["wxSYSTEM_MENU"] = "0x0800",
		["wxCAPTION"] = "0x20000000",
		["wxDEFAULT_FRAME_STYLE"] = "(wxSYSTEM_MENU | \\ wxCAPTION)",
		["wxNO_DEFAULT"] = "0x00000080 /* only valid with wxYES_NO */",
		["wxLOOP"] = "wxLOOP | 1",
		["wxSHIFTED"] = "1 << 4",
		["wxNOT"] = "~0x1 & 0xF",
	});

	[Fact]
	public void EvaluatesHexAndDecimal() {
		Assert.Equal(0x800, Values.Value("wxSYSTEM_MENU"));
		Assert.Equal(42, Values.Evaluate("42"));
		Assert.Equal(7, Values.Evaluate("7L"));
	}

	[Fact]
	public void EvaluatesMacrosMadeOfOtherMacros() => Assert.Equal(0x20000800, Values.Value("wxDEFAULT_FRAME_STYLE"));

	[Fact]
	public void IgnoresComments() => Assert.Equal(0x80, Values.Value("wxNO_DEFAULT"));

	[Fact]
	public void EvaluatesOperators() {
		Assert.Equal(16, Values.Value("wxSHIFTED"));
		Assert.Equal(0xE, Values.Value("wxNOT"));
		Assert.Equal(-1, Values.Evaluate("-1"));
	}

	[Fact]
	public void GivesUpOnWhatItCannotEvaluate() {
		Assert.Null(Values.Value("wxUNKNOWN"));
		Assert.Null(Values.Value("wxLOOP"));
		Assert.Null(Values.Evaluate("sizeof(int)"));
	}
}
