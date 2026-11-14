namespace WxKit;

/// <summary>A position in pixels. Wraps wxPoint, and crosses into the shim as it is.</summary>
public readonly record struct Point(int X, int Y) {
	/// <summary>Lets wxWidgets choose. Wraps wxDefaultPosition.</summary>
	public static Point Default { get; } = new(-1, -1);
}

/// <summary>A size in pixels. Wraps wxSize, and crosses into the shim as it is.</summary>
public readonly record struct Size(int Width, int Height) {
	/// <summary>Lets wxWidgets choose. Wraps wxDefaultSize.</summary>
	public static Size Default { get; } = new(-1, -1);
}
