namespace WxKit.Tests;

// Taskbar icons, which the tests make, give a menu, and free, since a test machine may have no
// taskbar or tray to show one in.
public class TaskBarIconTests {
	[Fact]
	public Task MakesAndFreesATaskBarIcon() => Gui.WithFrame(frame => {
		TaskBarIcon icon = new();
		Assert.True(icon.IsOk);
		Menu menu = new();
		menu.Append(StandardId.Exit);
		icon.Menu = menu;
		Assert.Same(menu, icon.Menu);
		icon.RightUp += (_, _) => { };
		if (TaskBarIcon.IsAvailable) {
			using Bitmap picture = new(16, 16);
			icon.SetIcon(picture, "Tester");
			icon.RemoveIcon();
		}
		icon.Dispose();
		icon.Dispose();
	});
}
