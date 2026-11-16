using WxKit;

namespace Settings;

// The sample's window, which lists the settings and opens the dialog that changes them.
internal sealed class MainFrame : Frame {
	private readonly AppSettings _settings;
	private readonly TextCtrl _summary;

	public MainFrame(AppSettings settings) : base(null, "Settings sample", size: new Size(520, 420)) {
		_settings = settings;
		// On macOS, Preferences goes in the application menu, where Mac applications have it, and
		// Exit becomes Quit there.
		Menu file = new();
		file.Append(StandardId.Exit).Click += (_, _) => Close();
		Menu edit = new();
		edit.Append(StandardId.Preferences, "&Settings...\tCtrl+,").Click += (_, _) => EditSettings();
		MenuBar menuBar = new();
		menuBar.Append(file, "&File");
		menuBar.Append(edit, "&Edit");
		MenuBar = menuBar;
		// The settings are read-only text, which a screen reader can read line by line.
		Panel panel = new(this);
		StaticText label = new(panel, "&Current settings:");
		_summary = new TextCtrl(panel, style: TextCtrlStyle.MultiLine | TextCtrlStyle.ReadOnly);
		Button change = new(panel, "&Change settings...");
		change.Click += (_, _) => EditSettings();
		BoxSizer sizer = new(Orientation.Vertical);
		sizer.Add(label, SizerFlags.Default.Border(Direction.Left | Direction.Right | Direction.Top));
		sizer.Add(_summary, new SizerFlags(1).Expand().Border());
		sizer.Add(change, SizerFlags.Default.Border(Direction.Left | Direction.Right | Direction.Bottom));
		panel.Sizer = sizer;
		ShowSummary();
		Center();
	}

	private void EditSettings() {
		using SettingsDialog dialog = new(this, _settings);
		if (dialog.ShowModal() != StandardId.Ok) return;
		dialog.ApplyTo(_settings);
		try {
			_settings.Save(AppSettings.DefaultPath);
		} catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
			MessageBox.Show($"The settings could not be saved. {error.Message}", "Settings sample", MessageDialogStyle.Ok | MessageDialogStyle.IconError, this);
		}
		ShowSummary();
	}

	// For the smoke test: opens the dialog, and cancels it once its modal loop runs.
	internal void OpenAndCancelSettings() {
		using SettingsDialog dialog = new(this, _settings);
		Application.Post(() => dialog.EndModal(StandardId.Cancel));
		dialog.ShowModal();
	}

	private void ShowSummary() {
		string font = _settings.Font.Length == 0 ? "the default" : DescribeFont(_settings.Font);
		string notifications = _settings.Notifications.Count == 0 ? "none" : string.Join(", ", AppSettings.Events.Where(_settings.Notifications.Contains));
		_summary.ChangeValue(string.Join('\n', [
			$"Name: {_settings.Name}",
			$"Theme: {AppSettings.Themes[_settings.Theme]}",
			$"Accent color: red {_settings.Accent.R}, green {_settings.Accent.G}, blue {_settings.Accent.B}",
			$"Font: {font}",
			$"Recent files remembered: {_settings.RecentFiles}",
			$"Documents folder: {_settings.DocumentsFolder}",
			$"Start when you sign in: {(_settings.StartWithSystem ? "yes" : "no")}",
			$"Notify when: {notifications}",
		]));
	}

	private static string DescribeFont(string description) {
		using Font font = new(description);
		return $"{font.FaceName}, {font.PointSize} points";
	}
}
