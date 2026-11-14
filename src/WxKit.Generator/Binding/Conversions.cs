namespace WxKit.Generator.Binding;

// How a value crosses from managed code through the shim to wxWidgets and back. Each kind of
// type has one conversion, which knows its form in all three places and the code that converts
// between them: the C parameters and return type in the shim, the C# types in the interop layer,
// and the C# type in the managed API.
internal abstract class Conversion {
	/// <summary>A short name used to tell overloads apart, such as String or Int.</summary>
	public abstract string Suffix { get; }

	// The shim's side.
	public abstract IEnumerable<string> CParameters(string name);

	public abstract string ToCpp(string name);

	public abstract string CReturnType { get; }

	public abstract string FromCpp(string expression);

	/// <summary>What the shim returns when wxWidgets throws.</summary>
	public abstract string Fallback { get; }

	// The interop layer's side.
	public abstract IEnumerable<string> InteropParameters(string name);

	public abstract string InteropReturnType { get; }

	// The managed side.
	public abstract string ManagedType(bool nullable);

	/// <summary>A null check, for parameters that cannot be null.</summary>
	public virtual string? Check(string name, bool nullable) => null;

	/// <summary>A fixed statement the call needs, such as pinning a string.</summary>
	public virtual string? Fixed(string name) => null;

	public abstract IEnumerable<string> InteropArguments(string name, bool nullable);

	public abstract string ManagedReturn(string expression);

	/// <summary>Whether an object of this type can be null, so a default of nullptr can be written as null.</summary>
	public virtual bool AcceptsNull => false;
}

internal sealed class VoidConversion : Conversion {
	public static VoidConversion Instance { get; } = new();

	public override string Suffix => "Void";

	public override string CReturnType => "void";

	public override string Fallback => string.Empty;

	public override string InteropReturnType => "void";

	public override IEnumerable<string> CParameters(string name) => throw new InvalidOperationException("void is not a parameter type.");

	public override string ToCpp(string name) => throw new InvalidOperationException("void is not a parameter type.");

	public override string FromCpp(string expression) => expression;

	public override IEnumerable<string> InteropParameters(string name) => throw new InvalidOperationException("void is not a parameter type.");

	public override string ManagedType(bool nullable) => "void";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => throw new InvalidOperationException("void is not a parameter type.");

	public override string ManagedReturn(string expression) => expression;
}

// A C bool's size is not guaranteed to match what .NET expects, so booleans cross as one byte.
internal sealed class BoolConversion : Conversion {
	public static BoolConversion Instance { get; } = new();

	public override string Suffix => "Bool";

	public override string CReturnType => "uint8_t";

	public override string Fallback => "0";

	public override string InteropReturnType => "byte";

	public override IEnumerable<string> CParameters(string name) => [$"uint8_t {name}"];

	public override string ToCpp(string name) => $"{name} != 0";

	public override string FromCpp(string expression) => $"({expression}) ? 1 : 0";

	public override IEnumerable<string> InteropParameters(string name) => [$"byte {name}"];

	public override string ManagedType(bool nullable) => "bool";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [$"{name} ? (byte)1 : (byte)0"];

	public override string ManagedReturn(string expression) => $"{expression} != 0";
}

/// <summary>A number with the same form in C and C#, such as int32_t and int.</summary>
internal sealed class NumberConversion(string suffix, string cType, string cppType, string csharpType) : Conversion {
	public static NumberConversion Int { get; } = new("Int", "int32_t", "int", "int");

	public static NumberConversion Long { get; } = new("Long", "int64_t", "long", "long");

	public static NumberConversion Double { get; } = new("Double", "double", "double", "double");

	public static NumberConversion Float { get; } = new("Float", "float", "float", "float");

	public override string Suffix => suffix;

	public override string CReturnType => cType;

	public override string Fallback => "0";

	public override string InteropReturnType => csharpType;

	public override IEnumerable<string> CParameters(string name) => [$"{cType} {name}"];

	public override string ToCpp(string name) => $"static_cast<{cppType}>({name})";

	public override string FromCpp(string expression) => $"static_cast<{cType}>({expression})";

	public override IEnumerable<string> InteropParameters(string name) => [$"{csharpType} {name}"];

	public override string ManagedType(bool nullable) => csharpType;

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [name];

	public override string ManagedReturn(string expression) => expression;
}

/// <summary>A number the managed API shows as an enum, such as a style or an orientation.</summary>
internal sealed class EnumConversion(string enumName, NumberConversion number) : Conversion {
	public string EnumName { get; } = enumName;

	public override string Suffix => EnumName;

	public override string CReturnType => number.CReturnType;

	public override string Fallback => "0";

	public override string InteropReturnType => number.InteropReturnType;

	public override IEnumerable<string> CParameters(string name) => number.CParameters(name);

	public override string ToCpp(string name) => number.ToCpp(name);

	public override string FromCpp(string expression) => number.FromCpp(expression);

	public override IEnumerable<string> InteropParameters(string name) => number.InteropParameters(name);

	public override string ManagedType(bool nullable) => EnumName;

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [$"({number.ManagedType(false)}){name}"];

	public override string ManagedReturn(string expression) => $"({EnumName}){expression}";
}

// Text goes in as UTF-16 with a length, pinned rather than copied, and comes out as a wxk_String
// the managed side reads and frees.
internal sealed class StringConversion : Conversion {
	public static StringConversion Instance { get; } = new();

	public override string Suffix => "String";

	public override string CReturnType => "wxk_String*";

	public override string Fallback => "nullptr";

	public override string InteropReturnType => "WxkString*";

	public override IEnumerable<string> CParameters(string name) => [$"const char16_t* {name}", $"size_t {name}Length"];

	public override string ToCpp(string name) => $"wxk::FromUtf16({name}, {name}Length)";

	public override string FromCpp(string expression) => $"wxk::ToUtf16({expression})";

	public override IEnumerable<string> InteropParameters(string name) => [$"char* {name}", $"nuint {name}Length"];

	public override string ManagedType(bool nullable) => "string";

	public override string? Check(string name, bool nullable) => $"ArgumentNullException.ThrowIfNull({name});";

	public override string? Fixed(string name) => $"fixed (char* {Pinned(name)} = {name})";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [Pinned(name), $"Text.Length({name})"];

	public override string ManagedReturn(string expression) => $"Text.Take({expression})";

	private static string Pinned(string name) => name.TrimStart('@') + "Text";
}

// wxPoint and wxSize cross as structs with the same layout on both sides. wxDefaultPosition and
// wxDefaultSize, which are (-1, -1), become null in the managed API.
internal sealed class GeometryConversion(string suffix, string cType, string cppType, string managedType) : Conversion {
	public static GeometryConversion Point { get; } = new("Point", "wxk_Point", "wxPoint", "Point");

	public static GeometryConversion Size { get; } = new("Size", "wxk_Size", "wxSize", "Size");

	public override string Suffix => suffix;

	public override string CReturnType => cType;

	public override string Fallback => $"{cType}{{-1, -1}}";

	public override string InteropReturnType => managedType;

	public override IEnumerable<string> CParameters(string name) => [$"{cType} {name}"];

	public override string ToCpp(string name) => $"wxk::From{suffix}({name})";

	public override string FromCpp(string expression) => $"wxk::To{suffix}({expression})";

	public override IEnumerable<string> InteropParameters(string name) => [$"{managedType} {name}"];

	public override string ManagedType(bool nullable) => nullable ? managedType + "?" : managedType;

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [nullable ? $"{name} ?? {managedType}.Default" : name];

	public override string ManagedReturn(string expression) => expression;

	public override bool AcceptsNull => true;

	public bool IsDefault(string cppDefault) => cppDefault == $"wxDefault{(suffix == "Point" ? "Position" : "Size")}";

	public string CppType => cppType;
}

// A pointer to a class WxKit wraps, which crosses as the class's handle. One coming back is wrapped
// in its managed object, which is made if WxKit has not seen the native one before.
internal sealed class ObjectConversion(BoundClass target) : Conversion {
	public BoundClass Target { get; } = target;

	public override string Suffix => Target.Name;

	public override string CReturnType => $"{Target.Handle.CType}*";

	public override string Fallback => "nullptr";

	public override string InteropReturnType => $"{Target.Handle.InteropType}*";

	public override IEnumerable<string> CParameters(string name) => [$"{Target.Handle.CType}* {name}"];

	public override string ToCpp(string name) => $"static_cast<{Target.CppName}*>(wxk::Unwrap({name}))";

	public override string FromCpp(string expression) => $"wxk::Wrap(static_cast<{Target.Handle.CppBase}*>({expression}))";

	public override IEnumerable<string> InteropParameters(string name) => [$"{Target.Handle.InteropType}* {name}"];

	public override string ManagedType(bool nullable) => nullable ? Target.Name + "?" : Target.Name;

	public override string? Check(string name, bool nullable) => nullable ? null : $"ArgumentNullException.ThrowIfNull({name});";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [nullable ? $"{name} is null ? null : ({Target.Handle.InteropType}*){name}.{Target.Handle.Accessor}" : $"({Target.Handle.InteropType}*){name}.{Target.Handle.Accessor}"];

	public override string ManagedReturn(string expression) => $"ObjectRegistry.Wrap<{Target.Name}>({expression})";

	public override bool AcceptsNull => true;
}

// A wxObject that may or may not be an event handler, such as an event's source. It crosses as
// itself, and comes back wrapped only when it is a class WxKit wraps.
internal sealed class AnyObjectConversion : Conversion {
	public static AnyObjectConversion Instance { get; } = new();

	public override string Suffix => "Object";

	public override string CReturnType => "wxk_Object*";

	public override string Fallback => "nullptr";

	public override string InteropReturnType => "WxkObject*";

	public override IEnumerable<string> CParameters(string name) => throw new InvalidOperationException("wxObject is only bound as a return type.");

	public override string ToCpp(string name) => throw new InvalidOperationException("wxObject is only bound as a return type.");

	public override string FromCpp(string expression) => $"wxk::Wrap({expression})";

	public override IEnumerable<string> InteropParameters(string name) => throw new InvalidOperationException("wxObject is only bound as a return type.");

	public override string ManagedType(bool nullable) => "EvtHandler?";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => throw new InvalidOperationException("wxObject is only bound as a return type.");

	public override string ManagedReturn(string expression) => $"ObjectRegistry.Wrap<EvtHandler>({expression})";
}
