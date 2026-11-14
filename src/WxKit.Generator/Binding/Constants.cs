using System.Globalization;
using System.Text.RegularExpressions;

namespace WxKit.Generator.Binding;

// Works out the value of the defines and enum values that style flags and defaults are made of,
// such as wxDEFAULT_FRAME_STYLE, which is a | of other defines. Only integer expressions are
// supported, which is all the flags use.
internal sealed partial class Constants(IReadOnlyDictionary<string, string> expressions) {
	private readonly Dictionary<string, long?> _values = new(StringComparer.Ordinal);
	private readonly HashSet<string> _evaluating = new(StringComparer.Ordinal);

	public long? Value(string name) {
		if (_values.TryGetValue(name, out long? cached)) return cached;
		if (!expressions.TryGetValue(name, out string? expression) || !_evaluating.Add(name)) return null;
		long? value = Evaluate(expression);
		_evaluating.Remove(name);
		_values[name] = value;
		return value;
	}

	public long? Evaluate(string expression) {
		string code = CommentPattern().Replace(expression, " ").Replace("\\", " ");
		List<string> tokens = [.. TokenPattern().Matches(code).Select(static match => match.Value)];
		int position = 0;
		long? result = ParseOr(tokens, ref position);
		return position == tokens.Count ? result : null;
	}

	private long? ParseOr(List<string> tokens, ref int position) {
		long? left = ParseAnd(tokens, ref position);
		while (left is not null && Accept(tokens, ref position, "|")) left = Combine(left, ParseAnd(tokens, ref position), static (a, b) => a | b);
		return left;
	}

	private long? ParseAnd(List<string> tokens, ref int position) {
		long? left = ParseShift(tokens, ref position);
		while (left is not null && Accept(tokens, ref position, "&")) left = Combine(left, ParseShift(tokens, ref position), static (a, b) => a & b);
		return left;
	}

	private long? ParseShift(List<string> tokens, ref int position) {
		long? left = ParseSum(tokens, ref position);
		while (left is not null) {
			if (Accept(tokens, ref position, "<<")) left = Combine(left, ParseSum(tokens, ref position), static (a, b) => a << (int)b);
			else if (Accept(tokens, ref position, ">>")) left = Combine(left, ParseSum(tokens, ref position), static (a, b) => a >> (int)b);
			else break;
		}
		return left;
	}

	private long? ParseSum(List<string> tokens, ref int position) {
		long? left = ParseUnary(tokens, ref position);
		while (left is not null) {
			if (Accept(tokens, ref position, "+")) left = Combine(left, ParseUnary(tokens, ref position), static (a, b) => a + b);
			else if (Accept(tokens, ref position, "-")) left = Combine(left, ParseUnary(tokens, ref position), static (a, b) => a - b);
			else break;
		}
		return left;
	}

	private long? ParseUnary(List<string> tokens, ref int position) {
		if (Accept(tokens, ref position, "~")) return ParseUnary(tokens, ref position) is long value ? ~value : null;
		if (Accept(tokens, ref position, "-")) return ParseUnary(tokens, ref position) is long negated ? -negated : null;
		if (Accept(tokens, ref position, "(")) {
			long? inner = ParseOr(tokens, ref position);
			return Accept(tokens, ref position, ")") ? inner : null;
		}
		if (position >= tokens.Count) return null;
		string token = tokens[position++];
		string digits = token.TrimEnd('u', 'U', 'l', 'L');
		if (digits.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) return long.TryParse(digits[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long hex) ? hex : null;
		if (char.IsDigit(digits[0])) return long.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out long number) ? number : null;
		return Value(token);
	}

	private static bool Accept(List<string> tokens, ref int position, string token) {
		if (position >= tokens.Count || tokens[position] != token) return false;
		position++;
		return true;
	}

	private static long? Combine(long? left, long? right, Func<long, long, long> operation) => left is long a && right is long b ? operation(a, b) : null;

	[GeneratedRegex(@"/\*.*?\*/|//.*$")]
	private static partial Regex CommentPattern();

	[GeneratedRegex(@"0[xX][0-9a-fA-F]+[uUlL]*|\d+[uUlL]*|[A-Za-z_]\w*|<<|>>|[|&~()+\-]")]
	private static partial Regex TokenPattern();
}
