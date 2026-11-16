using System.Globalization;
using WxKit;

namespace Settings;

// What the sample remembers, as lines of key=value in a file of its own.
internal sealed class AppSettings {
	public static readonly string[] Themes = ["Match the system", "Light", "Dark"];

	public static readonly string[] Events = [
		"A file is saved", "A file is changed elsewhere", "An update is ready", "A backup finishes",
		"A backup fails", "A download finishes", "Someone shares a file", "A comment is added",
		"A reminder is due", "The disk is nearly full", "The battery is low", "A sync conflict is found",
	];

	public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WxKit Settings sample", "settings.txt");

	public string Name { get; set; } = Environment.UserName;

	// An index into Themes.
	public int Theme { get; set; }

	public Color Accent { get; set; } = new(0, 120, 215);

	// The font's description, as Font.NativeFontInfoDesc gives it, or empty for the default font.
	public string Font { get; set; } = string.Empty;

	public int RecentFiles { get; set; } = 10;

	public string DocumentsFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

	public bool StartWithSystem { get; set; }

	// The events in Events to be notified of.
	public HashSet<string> Notifications { get; } = [Events[0], Events[2], Events[4]];

	// Reads the settings, or gives the defaults when there are none yet or the file cannot be read.
	public static AppSettings Load(string path) {
		AppSettings settings = new();
		string[] lines;
		try {
			lines = File.ReadAllLines(path);
		} catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
			return settings;
		}
		Dictionary<string, string> values = new(StringComparer.Ordinal);
		foreach (string line in lines) {
			int equals = line.IndexOf('=');
			if (equals > 0) values[line[..equals]] = line[(equals + 1)..];
		}
		if (values.TryGetValue("name", out string? name)) settings.Name = name;
		if (values.TryGetValue("theme", out string? theme) && int.TryParse(theme, CultureInfo.InvariantCulture, out int themeIndex) && themeIndex >= 0 && themeIndex < Themes.Length) settings.Theme = themeIndex;
		if (values.TryGetValue("accent", out string? accent) && accent.Length == 7 && int.TryParse(accent.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb)) settings.Accent = new Color((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
		if (values.TryGetValue("font", out string? font)) settings.Font = font;
		if (values.TryGetValue("recentFiles", out string? recent) && int.TryParse(recent, CultureInfo.InvariantCulture, out int count)) settings.RecentFiles = count;
		if (values.TryGetValue("documents", out string? documents)) settings.DocumentsFolder = documents;
		if (values.TryGetValue("startWithSystem", out string? start)) settings.StartWithSystem = start == "true";
		if (values.TryGetValue("notifications", out string? notifications)) {
			settings.Notifications.Clear();
			settings.Notifications.UnionWith(notifications.Split('|', StringSplitOptions.RemoveEmptyEntries).Where(Events.Contains));
		}
		return settings;
	}

	public void Save(string path) {
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllLines(path, [
			$"name={Name}",
			$"theme={Theme.ToString(CultureInfo.InvariantCulture)}",
			$"accent=#{Accent.R:X2}{Accent.G:X2}{Accent.B:X2}",
			$"font={Font}",
			$"recentFiles={RecentFiles.ToString(CultureInfo.InvariantCulture)}",
			$"documents={DocumentsFolder}",
			$"startWithSystem={(StartWithSystem ? "true" : "false")}",
			$"notifications={string.Join('|', Events.Where(Notifications.Contains))}",
		]);
	}
}
