namespace WxKit.Tests;

// Pickers: a button, and optionally a text box, that choose a color, a file, a folder, or a font.
public class PickerTests {
	[Fact]
	public Task PicksAColor() => Gui.WithFrame(frame => {
		ColorPickerCtrl black = new(frame);
		Assert.Equal(new Color(0, 0, 0), black.Color);
		Color teal = new(0, 128, 128);
		ColorPickerCtrl picker = new(frame, teal, style: ColorPickerCtrlStyle.UseTextCtrl);
		Assert.Equal(teal, picker.Color);
		Assert.True(picker.HasTextCtrl);
		Assert.NotNull(picker.TextCtrl);
		Assert.False(black.HasTextCtrl);
		Color plum = new(142, 69, 133);
		picker.Color = plum;
		Assert.Equal(plum, picker.Color);
	});

	[Fact]
	public Task PicksAFileAndAFolder() => Gui.WithFrame(frame => {
		string folder = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
		string file = Path.Combine(folder, "notes.txt");
		FilePickerCtrl files = new(frame, file, wildcard: "Text files (*.txt)|*.txt", style: FilePickerCtrlStyle.Save | FilePickerCtrlStyle.UseTextCtrl);
		Assert.Equal(file, files.Path);
		files.SetInitialDirectory(folder);
		DirPickerCtrl folders = new(frame, folder);
		Assert.Equal(folder, folders.Path);
		folders.Path = Path.GetDirectoryName(folder)!;
		Assert.Equal(Path.GetDirectoryName(folder), folders.Path);
	});

	[Fact]
	public Task PicksAFont() => Gui.WithFrame(frame => {
		using Font font = new(15, FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
		FontPickerCtrl picker = new(frame, font);
		using Font? selected = picker.SelectedFont;
		Assert.Equal(15, selected?.PointSize);
		Assert.Equal(FontWeight.Bold, selected?.Weight);
		picker.MaxPointSize = 72;
		Assert.Equal(72, picker.MaxPointSize);
	});
}
