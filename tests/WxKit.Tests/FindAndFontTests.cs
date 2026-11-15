namespace WxKit.Tests;

// The find and replace dialog, which keeps its data alive, and the font dialog, which copies its data.
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

	// Only the dialog holds the data it is given here.
	private static FindReplaceDialog CreateDialog(Frame frame) => new(frame, new FindReplaceData { FindString = "needle" }, "Find", FindReplaceDialogStyle.ReplaceDialog);
}
