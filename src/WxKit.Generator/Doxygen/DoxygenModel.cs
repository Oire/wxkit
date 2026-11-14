namespace WxKit.Generator.Doxygen;

// What Doxygen says about the wxWidgets interface headers, before any decision about binding it.

internal sealed record DoxygenApi(IReadOnlyDictionary<string, DoxygenClass> Classes, IReadOnlyDictionary<string, string> Constants);

internal sealed record DoxygenClass(
	string Name,
	IReadOnlyList<string> Bases,
	string? Header,
	string Brief,
	IReadOnlyList<DoxygenMember> Members,
	IReadOnlyList<DoxygenStyle> Styles,
	IReadOnlyList<DoxygenEvent> Events);

internal sealed record DoxygenMember(
	string Name,
	string Type,
	IReadOnlyList<DoxygenParameter> Parameters,
	bool IsConst,
	bool IsStatic,
	bool IsVirtual,
	bool IsPublic,
	bool IsDeprecated,
	IReadOnlyList<string> OnlyFor,
	string Brief);

internal sealed record DoxygenParameter(string Type, string Name, string? Default);

internal sealed record DoxygenStyle(string Name, string Description);

// An entry in a class's event table, such as EVT_BUTTON, which sends wxEVT_BUTTON as a wxCommandEvent.
internal sealed record DoxygenEvent(string Macro, string? EventType, string? EventClass, string Description);
