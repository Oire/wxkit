using System.Text;

namespace WxKit.Generator.Emitting;

// Writes indented code with tabs and LF line endings, as the repository's .editorconfig asks.
internal sealed class CodeWriter {
	private readonly StringBuilder _text = new();
	private int _depth;

	public CodeWriter Line(string line = "") {
		if (line.Length > 0) _text.Append('\t', _depth).Append(line);
		_text.Append('\n');
		return this;
	}

	/// <summary>Writes a line ending in an opening brace, and indents what follows until <see cref="Close"/>.</summary>
	public CodeWriter Open(string line) {
		Line(line + " {");
		_depth++;
		return this;
	}

	/// <summary>Ends a block and opens the next one on the same line, as in "} catch (Exception error) {".</summary>
	public CodeWriter Continue(string line) {
		_depth--;
		Line("} " + line + " {");
		_depth++;
		return this;
	}

	public CodeWriter Close(string closing = "}") {
		_depth--;
		return Line(closing);
	}

	public override string ToString() => _text.ToString();

	public static string Escape(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
