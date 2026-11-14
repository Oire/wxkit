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

	[GeneratedRegex(@"enum wxDeprecatedGUIConstants\s*\{(.*?)\};", RegexOptions.Singleline)]
	private static partial Regex EnumPattern();

	[GeneratedRegex(@"/\*.*?\*/|//[^\n]*", RegexOptions.Singleline)]
	private static partial Regex CommentPattern();
}
