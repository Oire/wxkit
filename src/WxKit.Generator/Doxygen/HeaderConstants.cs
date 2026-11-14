using System.Text.RegularExpressions;

namespace WxKit.Generator.Doxygen;

// A few constants are only in wxWidgets's real headers, not its interface headers, though the
// interface headers use them: wxFONTFAMILY_DEFAULT is wxDEFAULT, for example, which only the
// deprecated enum in wx/defs.h defines. This reads that enum from the same pinned wxWidgets.
internal static partial class HeaderConstants {
	public static Dictionary<string, string> ReadDeprecatedGuiConstants(string defsHeader) {
		Dictionary<string, string> constants = new(StringComparer.Ordinal);
		Match block = EnumPattern().Match(File.ReadAllText(defsHeader));
		if (!block.Success) throw new InvalidOperationException($"{defsHeader} no longer has the wxDeprecatedGUIConstants enum.");
		string body = CommentPattern().Replace(block.Groups[1].Value, " ");
		string previous = "-1";
		foreach (string entry in body.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)) {
			string[] parts = entry.Split('=', 2, StringSplitOptions.TrimEntries);
			// A value without an initializer follows the one before, as in C.
			constants[parts[0]] = parts.Length == 2 ? parts[1] : $"({previous}) + 1";
			previous = parts[0];
		}
		return constants;
	}

	// The interface headers sometimes list an enum's values without their initializers, as for
	// wxTextValidatorStyle's flags, which would then count up from zero. This reads the values of
	// the enums in the real headers, following C for those without an initializer. A value given
	// differently in two places, such as under different platforms' #if branches, is left out, since
	// a regular expression cannot tell which branch applies.
	public static Dictionary<string, string> ReadEnumValues(string includeDirectory) {
		Dictionary<string, string> values = new(StringComparer.Ordinal);
		HashSet<string> ambiguous = new(StringComparer.Ordinal);
		foreach (string header in Directory.EnumerateFiles(includeDirectory, "*.h")) {
			string text = PreprocessorPattern().Replace(CommentPattern().Replace(File.ReadAllText(header), " "), " ");
			foreach (Match block in AnyEnumPattern().Matches(text)) {
				string previous = "-1";
				foreach (string entry in block.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)) {
					string[] parts = entry.Split('=', 2, StringSplitOptions.TrimEntries);
					if (!IdentifierPattern().IsMatch(parts[0])) break;
					string value = parts.Length == 2 ? WhitespacePattern().Replace(parts[1], " ") : $"({previous}) + 1";
					previous = parts[0];
					if (values.TryGetValue(parts[0], out string? existing) && existing != value) ambiguous.Add(parts[0]);
					values[parts[0]] = value;
				}
			}
		}
		foreach (string name in ambiguous) values.Remove(name);
		return values;
	}

	// The art provider's ids are macros that make strings, which the interface headers only declare
	// as variables: wxART_MAKE_ART_ID(X) is "X", and wxART_MAKE_CLIENT_ID(X) is "X_C".
	public static Dictionary<string, string> ReadArtIds(string artProviderHeader) {
		Dictionary<string, string> ids = new(StringComparer.Ordinal);
		foreach (Match match in ArtIdPattern().Matches(File.ReadAllText(artProviderHeader))) {
			ids[match.Groups[1].Value] = match.Groups[3].Value + (match.Groups[2].Value == "CLIENT" ? "_C" : string.Empty);
		}
		if (ids.Count == 0) throw new InvalidOperationException($"{artProviderHeader} no longer defines its ids the way the generator reads them.");
		return ids;
	}

	[GeneratedRegex(@"^#define\s+(wxART_\w+)\s+wxART_MAKE_(ART|CLIENT)_ID\((\w+)\)", RegexOptions.Multiline)]
	private static partial Regex ArtIdPattern();

	[GeneratedRegex(@"enum wxDeprecatedGUIConstants\s*\{(.*?)\};", RegexOptions.Singleline)]
	private static partial Regex EnumPattern();

	[GeneratedRegex(@"/\*.*?\*/|//[^\n]*", RegexOptions.Singleline)]
	private static partial Regex CommentPattern();

	[GeneratedRegex(@"\benum(?:\s+class)?(?:\s+\w+)?(?:\s*:\s*\w+)?\s*\{([^{}]*)\}", RegexOptions.Singleline)]
	private static partial Regex AnyEnumPattern();

	[GeneratedRegex(@"^\s*#.*$", RegexOptions.Multiline)]
	private static partial Regex PreprocessorPattern();

	[GeneratedRegex(@"^wx[A-Z0-9_]+$")]
	private static partial Regex IdentifierPattern();

	[GeneratedRegex(@"\s+")]
	private static partial Regex WhitespacePattern();
}
