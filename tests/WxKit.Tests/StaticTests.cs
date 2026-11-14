namespace WxKit.Tests;

// Static members, and classes that only have them.
public class StaticTests {
	[Fact]
	public Task ReadsSystemSettings() => Gui.Run(() => {
		Assert.NotNull(SystemSettings.GetColor(SystemColor.Window));
		using Font? font = SystemSettings.GetFont(SystemFont.DefaultGuiFont);
		Assert.NotNull(font);
		Assert.True(SystemSettings.GetMetric(SystemMetric.ScreenX) > 0);
	});

	[Fact]
	public Task FindsTheFocusedWindow() => Gui.WithFrame(frame => {
		TextCtrl text = new(frame);
		frame.Show();
		text.SetFocus();
		Assert.Same(text, Window.FindFocus());
	});

	[Fact]
	public Task FindsAWindowById() => Gui.WithFrame(frame => {
		Button button = new(frame, "&Found", id: 4242);
		Assert.Same(button, Window.FindWindowById(4242, frame));
		Assert.Null(Window.FindWindowById(4343, frame));
	});

	[Fact]
	public Task GivesStandardArt() => Gui.Run(() => {
		using Bitmap? information = ArtProvider.GetBitmap(ArtId.Information, ArtClient.MessageBox);
		Assert.NotNull(information);
		Assert.True(information.Width > 0);
	});

	[Fact]
	public void ChecksTheThreadInStaticMembers() => Assert.Throws<InvalidOperationException>(() => SystemSettings.GetMetric(SystemMetric.ScreenX));
}
