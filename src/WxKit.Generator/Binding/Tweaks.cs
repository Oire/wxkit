namespace WxKit.Generator.Binding;

// The decisions the XML cannot make on its own: which classes and members to bind, what to call
// events, which getters and setters form a property, and which types a number really has. Every
// name a tweak gives is checked against the XML, so a tweak cannot quietly go stale.
internal sealed class Tweaks {
	private readonly Dictionary<string, ClassTweak> _classes = new(StringComparer.Ordinal);

	public IReadOnlyDictionary<string, ClassTweak> Classes => _classes;

	/// <summary>Event types to bind even though no bound class lists them, and the event class each sends.</summary>
	public Dictionary<string, string> ExtraEventTypes { get; } = new(StringComparer.Ordinal);

	public ClassTweak Class(string name, Action<ClassTweak>? configure = null) {
		if (!_classes.TryGetValue(name, out ClassTweak? tweak)) _classes[name] = tweak = new ClassTweak(name);
		configure?.Invoke(tweak);
		return tweak;
	}

	public void EventType(string constant, string eventClass) => ExtraEventTypes[constant] = eventClass;
}

internal sealed class ClassTweak(string name) {
	public string CppName { get; } = name;

	/// <summary>Null binds every supported member. Otherwise only these, by their C++ names.</summary>
	public HashSet<string>? OnlyMembers { get; private set; }

	public HashSet<string> IgnoredMembers { get; } = new(StringComparer.Ordinal);

	/// <summary>The class is written by hand. The generator only uses it as a type, and lists it so it can be wrapped.</summary>
	public bool IsHandwritten { get; private set; }

	public bool HasConstructors { get; private set; } = true;

	/// <summary>Whether WxKit can wrap a native object of this class it did not create. Generated event handler classes always can.</summary>
	public bool IsWrappable { get; private set; }

	public string? ManagedName { get; private set; }

	public Dictionary<string, string> EventNames { get; } = new(StringComparer.Ordinal);

	public List<(string Macro, string Constant, string EventClass, string Name)> AddedEvents { get; } = [];

	public Dictionary<string, (string Getter, string? Setter)> PropertyPairs { get; } = new(StringComparer.Ordinal);

	public Dictionary<string, string> Renames { get; } = new(StringComparer.Ordinal);

	public Dictionary<(string Member, string Parameter), string> ParameterEnums { get; } = [];

	public ClassTweak Only(params string[] members) {
		OnlyMembers = new HashSet<string>(members, StringComparer.Ordinal);
		return this;
	}

	public ClassTweak Ignore(params string[] members) {
		IgnoredMembers.UnionWith(members);
		return this;
	}

	public ClassTweak Handwritten() {
		IsHandwritten = true;
		return this;
	}

	/// <summary>The class only has static members, such as wxSystemSettings, and becomes a static class.</summary>
	public bool IsStaticOnly { get; private set; }

	/// <summary>The class is copied like a value, though it is not one of the classes that are by their ancestry, such as wxListItem.</summary>
	public bool IsValueObject { get; private set; }

	public ClassTweak ValueObject() {
		IsValueObject = true;
		return this;
	}

	public ClassTweak StaticOnly() {
		IsStaticOnly = true;
		return this;
	}

	public ClassTweak Wrappable() {
		IsWrappable = true;
		return this;
	}

	public ClassTweak NoConstructors() {
		HasConstructors = false;
		return this;
	}

	public ClassTweak Named(string managedName) {
		ManagedName = managedName;
		return this;
	}

	/// <summary>Names the event a class's event table lists under <paramref name="macro"/>.</summary>
	public ClassTweak Event(string macro, string eventName) {
		EventNames[macro] = eventName;
		return this;
	}

	/// <summary>Adds an event the class's event table leaves out.</summary>
	public ClassTweak AddEvent(string macro, string constant, string eventClass, string eventName) {
		AddedEvents.Add((macro, constant, eventClass, eventName));
		EventNames[macro] = eventName;
		return this;
	}

	/// <summary>Makes a property from a getter and setter whose names do not follow Get and Set.</summary>
	public ClassTweak Property(string propertyName, string getter, string? setter = null) {
		PropertyPairs[propertyName] = (getter, setter);
		return this;
	}

	public ClassTweak Rename(string member, string managedName) {
		Renames[member] = managedName;
		return this;
	}

	/// <summary>Gives a number parameter an enum type, such as an orientation.</summary>
	public ClassTweak ParameterEnum(string member, string parameter, string enumName) {
		ParameterEnums[(member, parameter)] = enumName;
		return this;
	}
}
