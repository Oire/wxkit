namespace WxKit.Tests;

public class DrawingTests {
	[Fact]
	public Task DrawsIntoABitmap() => Gui.Run(() => {
		using Bitmap bitmap = new(20, 20);
		using (MemoryDC dc = new(bitmap)) {
			using Brush white = new(new Color(255, 255, 255));
			dc.Background = white;
			dc.Clear();
			using Brush red = new(new Color(255, 0, 0));
			using Pen none = new(new Color(255, 0, 0));
			dc.Brush = red;
			dc.Pen = none;
			dc.DrawRectangle(new Rect(0, 0, 10, 20));
		}
		using Image? image = bitmap.ConvertToImage();
		Assert.NotNull(image);
		Assert.Equal((byte)255, image.GetRed(5, 5));
		Assert.Equal((byte)0, image.GetGreen(5, 5));
		Assert.Equal((byte)255, image.GetGreen(15, 5));
	});

	[Fact]
	public Task PaintsAWindow() => Gui.WithFrame(frame => {
		Panel panel = new(frame, size: new Size(50, 50));
		int paints = 0;
		panel.Paint += (_, _) => {
			using PaintDC dc = new(panel);
			dc.DrawLine(0, 0, 50, 50);
			paints++;
		};
		frame.Show();
		panel.Refresh();
		panel.Update();
		Assert.True(paints > 0);
	});
}
