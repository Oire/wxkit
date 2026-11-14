using WxKit.Generator.Doxygen;

namespace WxKit.Generator.Binding;

// What the generator decided to bind, and how, which the emitters turn into code.

internal sealed record BindingResult(
	IReadOnlyList<BoundClass> Classes,
	IReadOnlyList<BoundEventType> EventTypes,
	IReadOnlyList<BoundEnum> Enums,
	IReadOnlyList<string> Skipped);

/// <summary>A C++ enum the managed API shows as a C# one, such as wxShowEffect as ShowEffect.</summary>
internal sealed record BoundEnum(string Name, string CppName, IReadOnlyList<BoundStyle> Members);

/// <summary>How objects of a class cross the boundary: the handle type and what it points at.</summary>
/// <param name="CType">The shim's handle type, such as wxk_Window.</param>
/// <param name="CppBase">The wxWidgets class the handle points at, such as wxWindow.</param>
/// <param name="InteropType">The handle's C# struct, such as WxkWindow.</param>
/// <param name="Accessor">The managed member that gives a wrapper's handle.</param>
internal sealed record HandleKind(string CType, string CppBase, string InteropType, string Accessor) {
	public static HandleKind Window { get; } = new("wxk_Window", "wxWindow", "WxkWindow", "Native");

	public static HandleKind Sizer { get; } = new("wxk_Sizer", "wxSizer", "WxkSizer", "Native");

	public static HandleKind Menu { get; } = new("wxk_Menu", "wxMenu", "WxkMenu", "Native");

	public static HandleKind Event { get; } = new("wxk_Event", "wxEvent", "WxkEvent", "Handle");

	public static HandleKind EvtHandler { get; } = new("wxk_EvtHandler", "wxEvtHandler", "WxkEvtHandler", "NativeHandler");

	public bool IsEvtHandler => this == Window || this == Menu || this == EvtHandler;
}

internal sealed class BoundClass(DoxygenClass source, ClassTweak tweak, HandleKind handle) {
	public DoxygenClass Source { get; } = source;

	public ClassTweak Tweak { get; } = tweak;

	public HandleKind Handle { get; } = handle;

	public string CppName => Source.Name;

	public string Name => Tweak.ManagedName ?? Naming.Type(Source.Name);

	/// <summary>The nearest ancestor WxKit also binds.</summary>
	public BoundClass? Base { get; set; }

	/// <summary>Whether the class derives from wxTopLevelWindow, whose parent can be null.</summary>
	public bool IsTopLevel { get; set; }

	public List<BoundFunction> Constructors { get; } = [];

	public List<BoundMethod> Methods { get; } = [];

	public List<BoundProperty> Properties { get; } = [];

	public List<BoundEvent> Events { get; } = [];

	public BoundStyles? Styles { get; set; }

	public IEnumerable<BoundFunction> Functions => Constructors.Concat(Methods.Select(static method => method.Function)).Concat(Properties.SelectMany(static property => property.Setter is null ? [property.Getter] : new[] { property.Getter, property.Setter }));
}

internal sealed record BoundParameter(string CppName, string Name, Conversion Conversion, bool Nullable, string? Default);

internal sealed class BoundFunction(BoundClass owner, DoxygenMember source, Conversion returns, IReadOnlyList<BoundParameter> parameters, bool isConstructor) {
	public BoundClass Owner { get; } = owner;

	public DoxygenMember Source { get; } = source;

	public Conversion Returns { get; } = returns;

	/// <summary>In the order the C++ function takes them.</summary>
	public IReadOnlyList<BoundParameter> Parameters { get; } = parameters;

	/// <summary>In the order the managed API takes them, which can differ, such as an id moved after the label.</summary>
	public IReadOnlyList<BoundParameter> ManagedParameters { get; set; } = parameters;

	public bool IsConstructor { get; } = isConstructor;

	/// <summary>Told apart from overloads with the same name, such as "SetSize_Int4".</summary>
	public string ExportName { get; set; } = isConstructor ? "new" : source.Name;

	public string ShimName => $"wxk_{Naming.Type(Owner.CppName)}_{ExportName}";

	public string InteropName => Naming.Type(Owner.CppName) + char.ToUpperInvariant(ExportName[0]) + ExportName[1..].Replace("_", string.Empty);

	public IReadOnlyList<string> OnlyFor => Source.OnlyFor;
}

internal sealed record BoundMethod(string Name, BoundFunction Function);

internal sealed record BoundProperty(string Name, BoundFunction Getter, BoundFunction? Setter);

internal sealed record BoundEvent(string Name, BoundEventType Type, string Description);

/// <param name="Header">The header that declares the constant, such as wx/textctrl.h.</param>
internal sealed record BoundEventType(string Constant, string Name, BoundClass EventClass, string? Header);

internal sealed record BoundStyles(string Name, IReadOnlyList<BoundStyle> Members);

internal sealed record BoundStyle(string Name, string Constant, long Value, string Description);
