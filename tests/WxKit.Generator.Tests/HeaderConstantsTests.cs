using WxKit.Generator.Doxygen;

namespace WxKit.Generator.Tests;

public sealed class HeaderConstantsTests : IDisposable {
	// Shaped like the real headers: comments, an #if branch per platform, a value that follows the
	// one before it, and a scoped enum with an underlying type.
	private const string Header = """
		enum wxTextValidatorStyle
		{
			wxFILTER_NONE = 0x0, // No filtering.
			wxFILTER_EMPTY = 0x1,
			/* Non-ASCII characters. */
			wxFILTER_ASCII = 0x2
		};

		enum wxHatchStyle
		{
			wxHATCHSTYLE_FIRST = 111,
			wxHATCHSTYLE_BDIAGONAL = wxHATCHSTYLE_FIRST,
			wxHATCHSTYLE_CROSSDIAG
		};

		enum wxPlatformThing
		{
		#ifdef __WXMSW__
			wxPLATFORM_THING = 1,
		#else
			wxPLATFORM_THING = 2,
		#endif
			wxPLATFORM_OTHER = 3
		};

		enum class wxScoped : int { wxSCOPED_A = 4 };
		""";

	private readonly string _directory = Directory.CreateTempSubdirectory("wxkit-headers").FullName;

	public HeaderConstantsTests() => File.WriteAllText(Path.Combine(_directory, "test.h"), Header);

	public void Dispose() => Directory.Delete(_directory, recursive: true);

	[Fact]
	public void ReadsGivenValues() {
		Dictionary<string, string> values = HeaderConstants.ReadEnumValues(_directory);
		Assert.Equal("0x1", values["wxFILTER_EMPTY"]);
		Assert.Equal("0x2", values["wxFILTER_ASCII"]);
		Assert.Equal("4", values["wxSCOPED_A"]);
	}

	[Fact]
	public void FollowsTheValueBefore() => Assert.Equal("(wxHATCHSTYLE_BDIAGONAL) + 1", HeaderConstants.ReadEnumValues(_directory)["wxHATCHSTYLE_CROSSDIAG"]);

	[Fact]
	public void LeavesOutValuesThatDependOnThePlatform() {
		Dictionary<string, string> values = HeaderConstants.ReadEnumValues(_directory);
		Assert.DoesNotContain("wxPLATFORM_THING", values.Keys);
		Assert.Equal("3", values["wxPLATFORM_OTHER"]);
	}
}
