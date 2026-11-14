using System.Globalization;
using System.Text;

namespace WxKit.Generator.Binding;

// The rules that turn wxWidgets names into .NET ones.
internal static class Naming {
	// British spellings wxWidgets uses, and the .NET ones WxKit uses instead, by whole word.
	private static readonly Dictionary<string, string> Respellings = new(StringComparer.Ordinal) {
		["Centre"] = "Center",
		["Colour"] = "Color",
		["Colours"] = "Colors",
		["Grey"] = "Gray",
		["Behaviour"] = "Behavior",
		["Initialise"] = "Initialize",
		["Normalise"] = "Normalize",
		["Customise"] = "Customize",
		["Favourite"] = "Favorite",
		["Neighbour"] = "Neighbor",
	};

	// Words that wxWidgets writes run together in upper case, so they cannot be split on their own.
	private static readonly Dictionary<string, string> CompoundWords = new(StringComparer.Ordinal) {
		["MULTILINE"] = "MultiLine",
		["READONLY"] = "ReadOnly",
		["NOHIDESEL"] = "NoHideSelection",
		["DONTWRAP"] = "DontWrap",
		["CHARWRAP"] = "CharWrap",
		["WORDWRAP"] = "WordWrap",
		["BESTWRAP"] = "BestWrap",
		["EXACTFIT"] = "ExactFit",
		["NOTEXT"] = "NoText",
		["ICONIZE"] = "Iconize",
	};

	private static readonly Dictionary<string, string> ParameterNames = new(StringComparer.Ordinal) {
		["pos"] = "position",
		["orient"] = "orientation",
		["winid"] = "id",
		["dir"] = "direction",
		["sz"] = "size",
		["pt"] = "point",
		["win"] = "window",
		["string"] = "text",
		["str"] = "text",
	};

	// What the common events are called, in the way .NET names events. An event the table leaves
	// out is named after its macro, as EVT_TEXT_ENTER becomes TextEnter.
	private static readonly Dictionary<string, string> EventNames = new(StringComparer.Ordinal) {
		["EVT_ACTIVATE"] = "Activated",
		["EVT_CHILD_FOCUS"] = "ChildFocused",
		["EVT_CLOSE"] = "Closing",
		["EVT_DPI_CHANGED"] = "DpiChanged",
		["EVT_ENTER_WINDOW"] = "MouseEnter",
		["EVT_FULLSCREEN"] = "FullScreenChanged",
		["EVT_HELP"] = "HelpRequested",
		["EVT_ICONIZE"] = "Iconized",
		["EVT_KILL_FOCUS"] = "LostFocus",
		["EVT_LEAVE_WINDOW"] = "MouseLeave",
		["EVT_MAXIMIZE"] = "Maximized",
		["EVT_MENU_CLOSE"] = "MenuClosed",
		["EVT_MENU_HIGHLIGHT"] = "MenuHighlighted",
		["EVT_MENU_OPEN"] = "MenuOpened",
		["EVT_MOUSEWHEEL"] = "MouseWheel",
		["EVT_MOVE"] = "Moved",
		["EVT_MOVE_END"] = "MoveEnded",
		["EVT_MOVE_START"] = "MoveStarted",
		["EVT_SET_FOCUS"] = "GotFocus",
		["EVT_SHOW"] = "VisibilityChanged",
		["EVT_SIZE"] = "SizeChanged",
		["EVT_SYS_COLOUR_CHANGED"] = "SystemColorsChanged",
		["EVT_TEXT_MAXLEN"] = "MaxLengthReached",
		["EVT_TEXT_URL"] = "UrlClicked",
	};

	private static readonly HashSet<string> Keywords = ["base", "checked", "class", "default", "event", "fixed", "lock", "object", "operator", "out", "params", "ref", "string", "this"];

	/// <summary>wxButton to Button.</summary>
	public static string Type(string name) => Respell(name.StartsWith("wx", StringComparison.Ordinal) ? name[2..] : name);

	/// <summary>A method or property name, respelled. A leading digit is spelled out, as Get3StateValue gives ThreeStateValue.</summary>
	public static string Member(string name) => Respell(name.Length > 0 && char.IsDigit(name[0]) ? Digits[name[0] - '0'] + name[1..] : name);

	private static readonly string[] Digits = ["Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"];

	public static string Parameter(string name) {
		string renamed = ParameterNames.GetValueOrDefault(name, name);
		string camel = char.ToLowerInvariant(renamed[0]) + Respell(char.ToUpperInvariant(renamed[0]) + renamed[1..])[1..];
		return Keywords.Contains(camel) ? "@" + camel : camel;
	}

	/// <summary>The name of the event a macro such as EVT_SIZE handles, such as SizeChanged.</summary>
	public static string Event(string macro) => EventNames.TryGetValue(macro, out string? name) ? name : Words(macro["EVT_".Length..]);

	/// <summary>wxEVT_CLOSE_WINDOW to CloseWindow.</summary>
	public static string EventType(string constant) => Words(constant["wxEVT_".Length..]);

	/// <summary>
	/// A style constant's name inside its class's enum. wxTE_MULTILINE becomes MultiLine: the prefix
	/// most of the class's styles share is dropped, as is one naming the class, and
	/// wxDEFAULT_FRAME_STYLE becomes Default.
	/// </summary>
	public static string Style(string constant, string? sharedPrefix, string className) {
		string[] words = constant[2..].Split('_', StringSplitOptions.RemoveEmptyEntries);
		string classWord = Type(className).ToUpperInvariant();
		if (words is ["DEFAULT", .., "STYLE"]) return "Default";
		if (words.Length > 1 && (words[0] == sharedPrefix || words[0] == classWord)) words = words[1..];
		return Words(string.Join('_', words));
	}

	/// <summary>The prefix most of a class's style names share, such as TE for wxTE_MULTILINE.</summary>
	public static string? SharedStylePrefix(IEnumerable<string> constants) {
		List<string> prefixes = [.. constants.Select(static name => name[2..].Split('_')).Where(static words => words.Length > 1).Select(static words => words[0])];
		if (prefixes.Count == 0) return null;
		IGrouping<string, string> common = prefixes.GroupBy(static prefix => prefix).OrderByDescending(static group => group.Count()).First();
		return common.Key.Length <= 3 && common.Count() * 2 > prefixes.Count ? common.Key : null;
	}

	/// <summary>UPPER_CASE_WORDS to PascalCase, respelled.</summary>
	public static string Words(string upper) {
		StringBuilder result = new();
		foreach (string word in upper.Split('_', StringSplitOptions.RemoveEmptyEntries)) {
			if (CompoundWords.TryGetValue(word, out string? compound)) {
				result.Append(compound);
				continue;
			}
			// A word already in mixed case, such as RightToLeft in wxLayout_RightToLeft, keeps its own casing.
			string rest = word.Any(char.IsLower) ? word[1..] : word[1..].ToLower(CultureInfo.InvariantCulture);
			result.Append(char.ToUpperInvariant(word[0])).Append(rest);
		}
		return Respell(result.ToString());
	}

	private static string Respell(string identifier) {
		StringBuilder result = new();
		int start = 0;
		for (int index = 1; index <= identifier.Length; index++) {
			if (index < identifier.Length && !char.IsUpper(identifier[index])) continue;
			string word = identifier[start..index];
			result.Append(Respellings.GetValueOrDefault(word, word));
			start = index;
		}
		return result.ToString();
	}
}
