namespace WxKit.Tests;

// Real input, sent through the system as a user's is, so these check that events really arrive,
// not only that a handler can be added. The input goes to the active window, so a test that
// cannot make its window active and focused, as on a machine without a desktop session, is skipped
// rather than failed. macOS only lets an application send input with a permission CI does not have.
public class InputTests {
	[Fact]
	public Task ClicksAButton() => Gui.WithFrame(async frame => {
		Panel panel = new(frame);
		Button button = new(panel, "&Press me", position: new Point(20, 20), size: new Size(120, 40));
		int clicks = 0;
		button.Click += (_, _) => clicks++;
		await Activate(frame, button);
		Rect bounds = new(0, 0, button.ClientSize.Width, button.ClientSize.Height);
		Assert.True(UIActionSimulator.MouseMove(button.ClientToScreen(new Point(bounds.Width / 2, bounds.Height / 2))));
		Assert.True(UIActionSimulator.MouseClick());
		await Gui.WaitUntil(() => clicks == 1);
	});

	[Fact]
	public Task TypesIntoATextControl() => Gui.WithFrame(async frame => {
		TextCtrl text = new(frame);
		List<KeyCode> keys = [];
		text.KeyDown += (_, e) => {
			keys.Add(e.KeyCode);
			e.Skip();
		};
		await Activate(frame, text);
		Assert.True(UIActionSimulator.Text("hello"));
		await Gui.WaitUntil(() => text.Value == "hello");
		Assert.Equal((KeyCode)'H', keys[0]);
	});

	[Fact]
	public Task ChoosesAMenuItemByItsShortcut() => Gui.WithFrame(async frame => {
		Menu menu = new();
		int chosen = 0;
		menu.Append(6100, "&Mark\tCtrl+M").Click += (_, _) => chosen++;
		MenuBar menuBar = new();
		menuBar.Append(menu, "&Edit");
		frame.MenuBar = menuBar;
		TextCtrl text = new(frame);
		await Activate(frame, text);
		Assert.True(UIActionSimulator.Char((KeyCode)'M', KeyModifiers.Control));
		await Gui.WaitUntil(() => chosen == 1);
	});

	// Shows the frame and focuses the window the input is for, or skips the test when the system
	// will not make the frame active, as it only lets the foreground application take it.
	private static async Task Activate(Frame frame, Window focus) {
		Assert.SkipWhen(OperatingSystem.IsMacOS(), "macOS only lets an application send input with the accessibility permission, which the test run does not have.");
		frame.Show();
		frame.Raise();
		focus.SetFocus();
		try {
			await Gui.WaitUntil(() => frame.IsActive && Window.FindFocus() == focus);
		} catch (TimeoutException) {
			Assert.Skip("The system did not make the test's window active, so input would go elsewhere.");
		}
	}
}
