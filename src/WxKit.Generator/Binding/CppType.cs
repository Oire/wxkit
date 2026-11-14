namespace WxKit.Generator.Binding;

// A C++ type as the interface headers write it, such as "const wxString &" or "wxWindow *".
internal sealed record CppType(string Name, bool IsConst, int Pointers, bool IsReference) {
	public static CppType Parse(string text) {
		string spaced = text.Replace("*", " * ").Replace("&", " & ");
		List<string> words = [.. spaced.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
		bool isConst = words.Remove("const");
		int pointers = words.RemoveAll(static word => word == "*");
		bool isReference = words.RemoveAll(static word => word == "&") > 0;
		words.RemoveAll(static word => word is "virtual" or "static" or "inline" or "explicit");
		return new CppType(string.Join(' ', words), isConst, pointers, isReference);
	}

	public bool IsVoid => Name == "void" && Pointers == 0;

	/// <summary>The type as passed by value, such as wxString for const wxString &.</summary>
	public bool IsValueOrConstReference => Pointers == 0 && (!IsReference || IsConst);

	public override string ToString() => $"{(IsConst ? "const " : string.Empty)}{Name}{new string('*', Pointers)}{(IsReference ? "&" : string.Empty)}";
}
