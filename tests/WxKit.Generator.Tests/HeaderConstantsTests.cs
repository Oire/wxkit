using WxKit.Generator.Doxygen;

namespace WxKit.Generator.Tests;

public sealed class HeaderConstantsTests : IDisposable {
	// Shaped like the real headers: comments, values that follow the one before them, a scoped enum
	// with an underlying type, a continued line, and values and macros that depend on the platform.
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

		enum wxStockCursor
		{
			wxCURSOR_NONE,
			wxCURSOR_BLANK,
		#ifdef __WXGTK__
			wxCURSOR_DEFAULT,
		#endif
			wxCURSOR_ARROWWAIT,
			wxCURSOR_EXPLICIT = 40
		};

		enum wxEventCategory
		{
			wxEVT_CATEGORY_UI = 1,
			wxEVT_CATEGORY_ALL =
				wxEVT_CATEGORY_UI| \
				2
		};

		enum class wxScoped : int { wxSCOPED_A = 4 };

		#if defined(__WXGTK__)
			#define wxTR_DEFAULT_STYLE (wxTR_HAS_BUTTONS | wxTR_NO_LINES)
		#else
			#define wxTR_DEFAULT_STYLE \
				(wxTR_HAS_BUTTONS | wxTR_LINES_AT_ROOT)
		#endif
		#define wxTR_HAS_BUTTONS 0x0001
		""";

	private readonly string _directory = Directory.CreateTempSubdirectory("wxkit-headers").FullName;

	public HeaderConstantsTests() => File.WriteAllText(Path.Combine(_directory, "test.h"), Header);

	private HeaderValues Values => HeaderConstants.ReadValues(_directory);

	public void Dispose() => Directory.Delete(_directory, recursive: true);

	[Fact]
	public void ReadsGivenValues() {
		IReadOnlyDictionary<string, string> values = Values.Values;
		Assert.Equal("0x1", values["wxFILTER_EMPTY"]);
		Assert.Equal("0x2", values["wxFILTER_ASCII"]);
		Assert.Equal("4", values["wxSCOPED_A"]);
	}

	[Fact]
	public void FollowsTheValueBefore() => Assert.Equal("(wxHATCHSTYLE_BDIAGONAL) + 1", Values.Values["wxHATCHSTYLE_CROSSDIAG"]);

	[Fact]
	public void JoinsContinuedLines() => Assert.Equal("wxEVT_CATEGORY_UI| 2", Values.Values["wxEVT_CATEGORY_ALL"]);

	[Fact]
	public void FindsValuesThatDependOnThePlatform() {
		HeaderValues values = Values;
		Assert.Contains("wxCURSOR_DEFAULT", values.Conditional);
		Assert.Contains("wxCURSOR_ARROWWAIT", values.Conditional);
		Assert.DoesNotContain("wxCURSOR_ARROWWAIT", values.Values.Keys);
		Assert.Equal("40", values.Values["wxCURSOR_EXPLICIT"]);
		Assert.Equal("(wxCURSOR_NONE) + 1", values.Values["wxCURSOR_BLANK"]);
	}

	[Fact]
	public void FindsMacrosWrittenTwoWays() {
		HeaderValues values = Values;
		Assert.Contains("wxTR_DEFAULT_STYLE", values.Conditional);
		Assert.DoesNotContain("wxTR_HAS_BUTTONS", values.Conditional);
	}
}
