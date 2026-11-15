namespace WxKit.Tests;

// The find and replace dialog, which keeps its data alive, and the font and color dialogs, which copy their data.
public class FindAndFontTests {
	[Fact]
	public Task RoundTripsFindData() => Gui.Run(() => {
		FindReplaceData data = new(FindReplaceFlags.Down | FindReplaceFlags.MatchCase) {
			FindString = "needle",
			ReplaceString = "thread",
		};
		Assert.Equal(FindReplaceFlags.Down | FindReplaceFlags.MatchCase, data.Flags);
		Assert.Equal("needle", data.FindString);
		Assert.Equal("thread", data.ReplaceString);
	});

	[Fact]
	public Task KeepsTheFindDataAliveForTheDialog() => Gui.WithFrame(async frame => {
		FindReplaceDialog dialog = CreateDialog(frame);
		for (int i = 0; i < 3; i++) {
			GC.Collect();
			GC.WaitForPendingFinalizers();
			await Task.Yield();
		}
		// The dialog reads its data as it shows, which would be freed if nothing kept it alive.
		dialog.Show();
		dialog.Destroy();
	});

	[Fact]
	public Task GivesTheFontDialogItsData() => Gui.WithFrame(frame => {
		using Font font = new(17, FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
		using FontData data = new() { InitialFont = font };
		using FontDialog dialog = new(frame, data);
		using FontData? given = dialog.FontData;
		Assert.NotNull(given);
		using Font? initial = given.InitialFont;
		Assert.Equal(17, initial?.PointSize);
	});

	[Fact]
	public Task GivesTheColorDialogItsData() => Gui.WithFrame(frame => {
		Color teal = new(0, 128, 128);
		Color plum = new(142, 69, 133);
		using ColorData data = new() { Color = teal, ChooseFull = true };
		data.SetCustomColor(3, plum);
		Assert.Equal(teal, data.Color);
		Assert.Equal(plum, data.GetCustomColor(3));
		using ColorDialog dialog = new(frame, data);
		using ColorData? given = dialog.ColorData;
		Assert.NotNull(given);
		Assert.Equal(teal, given.Color);
		Assert.True(given.ChooseFull);
	});

	// Only the dialog holds the data it is given here.
	private static FindReplaceDialog CreateDialog(Frame frame) => new(frame, new FindReplaceData { FindString = "needle" }, "Find", FindReplaceDialogStyle.ReplaceDialog);
}
