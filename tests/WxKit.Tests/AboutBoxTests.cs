namespace WxKit.Tests;

// The About box, which only Windows shows as a modal dialog a test can close.
public class AboutBoxTests {
	[Fact]
	public Task ShowsAnAboutBox() => Gui.WithFrame(frame => {
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Only Windows shows the About box as a modal dialog, which the test can close.");
		AboutDialogInfo info = new() {
			Name = "Tester",
			Version = "1.2",
			Description = "Checks the About box.",
			Copyright = "(c) 2026 WxKit",
			License = "MIT",
			WebSite = "https://github.com/trypsynth/wxkit",
			Developers = { "Ada", "Grace" },
		};
		string? title = null;
		// The posted action runs inside the About box's modal loop, and closes it. wxWidgets
		// destroys the box once it is closed, so its title is read here.
		Application.Post(() => {
			for (Window? window = Window.FindFocus(); window is not null; window = window.Parent) {
				if (window is Dialog dialog && dialog.IsModal) {
					title = dialog.Title;
					dialog.EndModal(StandardId.Ok);
					return;
				}
			}
		});
		AboutBox.Show(info, frame);
		Assert.NotNull(title);
		Assert.Contains("Tester", title);
	});

	[Fact]
	public void NeedsInfo() => Assert.Throws<ArgumentNullException>(() => AboutBox.Show(null!));
}
