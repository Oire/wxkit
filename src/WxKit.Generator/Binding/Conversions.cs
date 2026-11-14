namespace WxKit.Generator.Binding;

// How a value crosses from managed code through the shim to wxWidgets and back. Each kind of
// type has one conversion, which knows its form in all three places and the code that converts
// between them: the C parameters and return type in the shim, the C# types in the interop layer,
// and the C# type in the managed API. Managed code names WxKit's own types with global::WxKit,
// since a generated member can hide one, as Pen's Color property hides the Color type.
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

	/// <summary>The managed type of a value coming back, which can be more specific than the type going in.</summary>
	public virtual string ManagedReturnType(bool nullable) => ManagedType(nullable);

	/// <summary>A null check, for parameters that cannot be null.</summary>
	public virtual string? Check(string name, bool nullable) => null;

	/// <summary>A fixed statement the call needs, such as pinning a string.</summary>
	public virtual string? Fixed(string name) => null;

	/// <summary>A statement the call needs before it, such as building a native copy of a list.</summary>
	public virtual string? Prepare(string name) => null;

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

	public static NumberConversion Byte { get; } = new("Byte", "uint8_t", "unsigned char", "byte");

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

/// <summary>
/// A number the managed API shows as an enum, such as a style or an orientation. When the C++
/// parameter is an enum itself, <paramref name="cppType"/> names it, so the shim casts to it.
/// </summary>
internal sealed class EnumConversion(string enumName, NumberConversion number, string? cppType = null) : Conversion {
	public string EnumName { get; } = enumName;

	public override string Suffix => EnumName;

	public override string CReturnType => number.CReturnType;

	public override string Fallback => "0";

	public override string InteropReturnType => number.InteropReturnType;

	public override IEnumerable<string> CParameters(string name) => number.CParameters(name);

	public override string ToCpp(string name) => cppType is null ? number.ToCpp(name) : $"static_cast<{cppType}>({name})";

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

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [Pinned(name), $"NativeString.Length({name})"];

	public override string ManagedReturn(string expression) => $"NativeString.Take({expression})";

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

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [nullable ? $"{name} ?? global::WxKit.{managedType}.Default" : name];

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

// An unsigned C++ size or count, such as size_t, which the managed API shows as int, the way .NET
// shows counts. A negative value going in, or one too large for int coming back, throws.
internal sealed class CountConversion(string suffix, string cType, string interopType) : Conversion {
	public static CountConversion Size { get; } = new("SizeT", "size_t", "nuint");

	public static CountConversion Unsigned { get; } = new("UInt", "uint32_t", "uint");

	public override string Suffix => suffix;

	public override string CReturnType => cType;

	public override string Fallback => "0";

	public override string InteropReturnType => interopType;

	public override IEnumerable<string> CParameters(string name) => [$"{cType} {name}"];

	public override string ToCpp(string name) => name;

	public override string FromCpp(string expression) => $"static_cast<{cType}>({expression})";

	public override IEnumerable<string> InteropParameters(string name) => [$"{interopType} {name}"];

	public override string ManagedType(bool nullable) => "int";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [$"checked(({interopType}){name})"];

	public override string ManagedReturn(string expression) => $"checked((int){expression})";
}

// wxRect crosses as a struct with the same layout on both sides.
internal sealed class RectConversion : Conversion {
	public static RectConversion Instance { get; } = new();

	public override string Suffix => "Rect";

	public override string CReturnType => "wxk_Rect";

	public override string Fallback => "wxk_Rect{0, 0, 0, 0}";

	public override string InteropReturnType => "Rect";

	public override IEnumerable<string> CParameters(string name) => [$"wxk_Rect {name}"];

	public override string ToCpp(string name) => $"wxk::FromRect({name})";

	public override string FromCpp(string expression) => $"wxk::ToRect({expression})";

	public override IEnumerable<string> InteropParameters(string name) => [$"Rect {name}"];

	public override string ManagedType(bool nullable) => "Rect";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [name];

	public override string ManagedReturn(string expression) => expression;
}

// A wxColour crosses as its channels and whether it is set. An unset one, wxNullColour, is null in
// the managed API, so setting a window's color to null restores its default.
internal sealed class ColorConversion : Conversion {
	public static ColorConversion Instance { get; } = new();

	public override string Suffix => "Color";

	public override string CReturnType => "wxk_Colour";

	public override string Fallback => "wxk_Colour{0, 0, 0, 0, 0}";

	public override string InteropReturnType => "WxkColor";

	public override IEnumerable<string> CParameters(string name) => [$"wxk_Colour {name}"];

	public override string ToCpp(string name) => $"wxk::FromColour({name})";

	public override string FromCpp(string expression) => $"wxk::ToColour({expression})";

	public override IEnumerable<string> InteropParameters(string name) => [$"WxkColor {name}"];

	public override string ManagedType(bool nullable) => "Color?";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [$"global::WxKit.Color.ToNative({name})"];

	public override string ManagedReturn(string expression) => $"global::WxKit.Color.FromNative({expression})";

	public override bool AcceptsNull => true;
}

// A wxArrayString goes in as a native copy of any sequence of strings, made for the call, and
// comes out as a string array.
internal sealed class StringArrayConversion : Conversion {
	public static StringArrayConversion Instance { get; } = new();

	public override string Suffix => "Strings";

	public override string CReturnType => "wxk_StringArray*";

	public override string Fallback => "nullptr";

	public override string InteropReturnType => "WxkStringArray*";

	public override IEnumerable<string> CParameters(string name) => [$"const wxk_StringArray* {name}"];

	public override string ToCpp(string name) => $"*wxk::Unwrap({name})";

	public override string FromCpp(string expression) => $"wxk::Wrap(new wxArrayString({expression}))";

	public override IEnumerable<string> InteropParameters(string name) => [$"WxkStringArray* {name}"];

	public override string ManagedType(bool nullable) => "IEnumerable<string>";

	public override string ManagedReturnType(bool nullable) => "string[]";

	public override string? Check(string name, bool nullable) => $"ArgumentNullException.ThrowIfNull({name});";

	public override string? Prepare(string name) => $"using NativeStringArray {Native(name)} = new({name});";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [$"{Native(name)}.Handle"];

	public override string ManagedReturn(string expression) => $"NativeStringArray.Take({expression})";

	private static string Native(string name) => name.TrimStart('@') + "Native";
}

// A value object, such as a font, crosses as a handle to a heap copy the managed side owns. One
// going in can be null, for the class's empty object; one coming back empty is null. A bundle of
// bitmaps or icons is made from one, as C++ would convert it.
internal sealed class ValueConversion(BoundClass target, string? wrapper = null, bool mutable = false) : Conversion {
	public BoundClass Target { get; } = target;

	public override string Suffix => Target.Name;

	public override string CReturnType => "wxk_Object*";

	public override string Fallback => "nullptr";

	public override string InteropReturnType => "WxkObject*";

	public override IEnumerable<string> CParameters(string name) => [mutable ? $"wxk_Object* {name}" : $"const wxk_Object* {name}"];

	public override string ToCpp(string name) => mutable ? $"wxk::ValueRef<{Target.CppName}>({name})" : wrapper is null ? $"wxk::ValueOf<{Target.CppName}>({name})" : $"{wrapper}(wxk::ValueOf<{Target.CppName}>({name}))";

	public override string FromCpp(string expression) => $"wxk::CopyValue<{Target.CppName}>({expression})";

	public override IEnumerable<string> InteropParameters(string name) => [$"WxkObject* {name}"];

	public override string ManagedType(bool nullable) => nullable ? Target.Name + "?" : Target.Name;

	public override string ManagedReturnType(bool nullable) => Target.Name + "?";

	public override string? Check(string name, bool nullable) => nullable && !mutable ? null : $"ArgumentNullException.ThrowIfNull({name});";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [nullable && !mutable ? $"{name} is null ? null : {name}.Native" : $"{name}.Native"];

	public override string ManagedReturn(string expression) => $"global::WxKit.{Target.Name}.Take({expression})";

	public override bool AcceptsNull => true;
}

// An object the managed side owns, such as a drawing context, passed by reference or pointer.
internal sealed class OwnedConversion(BoundClass target, bool byReference) : Conversion {
	public override string Suffix => target.Name;

	public override string CReturnType => throw new InvalidOperationException("Owned objects are only bound as parameters.");

	public override string Fallback => "nullptr";

	public override string InteropReturnType => throw new InvalidOperationException("Owned objects are only bound as parameters.");

	public override IEnumerable<string> CParameters(string name) => [$"wxk_Object* {name}"];

	public override string ToCpp(string name) => byReference ? $"*static_cast<{target.CppName}*>(wxk::Unwrap({name}))" : $"static_cast<{target.CppName}*>(wxk::Unwrap({name}))";

	public override string FromCpp(string expression) => throw new InvalidOperationException("Owned objects are only bound as parameters.");

	public override IEnumerable<string> InteropParameters(string name) => [$"WxkObject* {name}"];

	public override string ManagedType(bool nullable) => nullable ? target.Name + "?" : target.Name;

	public override string? Check(string name, bool nullable) => nullable ? null : $"ArgumentNullException.ThrowIfNull({name});";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [nullable ? $"{name} is null ? null : {name}.Native" : $"{name}.Native"];

	public override string ManagedReturn(string expression) => throw new InvalidOperationException("Owned objects are only bound as parameters.");

	public override bool AcceptsNull => !byReference;
}

// A number a function writes through a pointer, such as GetSelection's from and to, which the
// managed API gives as an out parameter. A long goes through the shim's OutLong, since a C long is
// 32 bits on Windows.
internal sealed class OutConversion(string suffix, string cType, string csharpType, bool isLong) : Conversion {
	public static OutConversion Int { get; } = new("OutInt", "int32_t", "int", isLong: false);

	public static OutConversion Long { get; } = new("OutLong", "int64_t", "long", isLong: true);

	public static OutConversion Double { get; } = new("OutDouble", "double", "double", isLong: false);

	public override string Suffix => suffix;

	public override string CReturnType => throw new InvalidOperationException("Output pointers are only parameters.");

	public override string Fallback => "0";

	public override string InteropReturnType => throw new InvalidOperationException("Output pointers are only parameters.");

	public override IEnumerable<string> CParameters(string name) => [$"{cType}* {name}"];

	public override string ToCpp(string name) => isLong ? $"wxk::OutLong{{{name}}}" : name;

	public override string FromCpp(string expression) => throw new InvalidOperationException("Output pointers are only parameters.");

	public override IEnumerable<string> InteropParameters(string name) => [$"{csharpType}* {name}"];

	public override string ManagedType(bool nullable) => "out " + csharpType;

	// An out parameter has to be assigned before its address is taken.
	public override string? Prepare(string name) => $"{name} = 0;";

	public override string? Fixed(string name) => $"fixed ({csharpType}* {Pointer(name)} = &{name})";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [Pointer(name)];

	public override string ManagedReturn(string expression) => throw new InvalidOperationException("Output pointers are only parameters.");

	private static string Pointer(string name) => name.TrimStart('@') + "Pointer";
}

// A function that changes its object and returns it, such as wxFont::MakeBold, returns the same
// managed object, so calls chain.
internal sealed class SelfConversion(BoundClass owner) : Conversion {
	public override string Suffix => "Self";

	public override string CReturnType => "void";

	public override string Fallback => string.Empty;

	public override string InteropReturnType => "void";

	public override IEnumerable<string> CParameters(string name) => throw new InvalidOperationException("Self is only a return type.");

	public override string ToCpp(string name) => throw new InvalidOperationException("Self is only a return type.");

	public override string FromCpp(string expression) => expression;

	public override IEnumerable<string> InteropParameters(string name) => throw new InvalidOperationException("Self is only a return type.");

	public override string ManagedType(bool nullable) => owner.Name;

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => throw new InvalidOperationException("Self is only a return type.");

	public override string ManagedReturn(string expression) => expression;
}

// A list of windows, such as a window's children, which comes back as an array of their managed objects.
internal sealed class WindowListConversion : Conversion {
	public static WindowListConversion Instance { get; } = new();

	public override string Suffix => "Windows";

	public override string CReturnType => "wxk_WindowList*";

	public override string Fallback => "nullptr";

	public override string InteropReturnType => "WxkWindowList*";

	public override IEnumerable<string> CParameters(string name) => throw new InvalidOperationException("Window lists are only return types.");

	public override string ToCpp(string name) => throw new InvalidOperationException("Window lists are only return types.");

	public override string FromCpp(string expression) => $"wxk::ToWindowList({expression})";

	public override IEnumerable<string> InteropParameters(string name) => throw new InvalidOperationException("Window lists are only return types.");

	public override string ManagedType(bool nullable) => "Window[]";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => throw new InvalidOperationException("Window lists are only return types.");

	public override string ManagedReturn(string expression) => $"global::WxKit.NativeWindowList.Take({expression})";
}

// A wxTreeItemId wraps one pointer, so it crosses as that pointer and becomes the TreeItemId value type.
internal sealed class TreeItemIdConversion : Conversion {
	public static TreeItemIdConversion Instance { get; } = new();

	public override string Suffix => "TreeItemId";

	public override string CReturnType => "void*";

	public override string Fallback => "nullptr";

	public override string InteropReturnType => "nint";

	public override IEnumerable<string> CParameters(string name) => [$"void* {name}"];

	public override string ToCpp(string name) => $"wxTreeItemId({name})";

	public override string FromCpp(string expression) => $"({expression}).GetID()";

	public override IEnumerable<string> InteropParameters(string name) => [$"nint {name}"];

	public override string ManagedType(bool nullable) => "TreeItemId";

	public override IEnumerable<string> InteropArguments(string name, bool nullable) => [$"{name}.Value"];

	public override string ManagedReturn(string expression) => $"new global::WxKit.TreeItemId({expression})";
}
