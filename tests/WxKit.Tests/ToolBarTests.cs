namespace WxKit.Tests;

public class ToolBarTests {
	[Fact]
	public Task AddsToolsToAFramesToolBar() => Gui.WithFrame(frame => {
		ToolBar? toolBar = frame.CreateToolBar();
		Assert.NotNull(toolBar);
		Assert.Same(toolBar, frame.ToolBar);
		using Bitmap? open = ArtProvider.GetBitmap(ArtId.FileOpen, ArtClient.Toolbar);
		Assert.NotNull(open);
		toolBar.AddTool(StandardId.Open, "Open", open, "Open a file");
		toolBar.AddSeparator();
		toolBar.AddTool(StandardId.Save, "Save", open);
		Assert.True(toolBar.Realize());
		Assert.Equal(3, toolBar.ToolsCount);
	});
}
