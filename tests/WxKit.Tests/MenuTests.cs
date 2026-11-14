namespace WxKit.Tests;

public class MenuTests {
	[Fact]
	public Task KeepsStandardIdsAndPicksOthers() => Gui.WithFrame(frame => {
		Menu menu = new();
		MenuItem exit = menu.Append(StandardId.Exit);
		MenuItem custom = menu.Append(StandardId.Any, "&Custom", "Does something");
		MenuItem check = menu.Append(StandardId.Any, "&Check", kind: MenuItemKind.Check);
		menu.AppendSeparator();
		Assert.Equal(StandardId.Exit, exit.Id);
		Assert.NotEqual(StandardId.Any, custom.Id);
		Assert.NotEqual(custom.Id, check.Id);
		Assert.Same(menu, exit.Menu);
		MenuBar bar = new();
		bar.Append(menu, "&File");
		frame.MenuBar = bar;
	});

	[Fact]
	public Task GivesTheMenuBarToItsFrame() => Gui.Run(async () => {
		Frame frame = new(null, "Menus");
		Menu menu = new();
		MenuBar bar = new();
		bar.Append(menu, "&File");
		frame.MenuBar = bar;
		Assert.Same(bar, frame.MenuBar);
		Assert.Same(frame, bar.Parent);
		frame.Destroy();
		await Gui.WaitUntil(() => frame.IsDestroyed);
		Assert.True(bar.IsDestroyed);
		Assert.True(menu.IsDestroyed);
	});
}

public class TimerTests {
	[Fact]
	public Task TicksOnTheGuiThread() => Gui.Run(async () => {
		using Timer timer = new();
		TaskCompletionSource<bool> ticked = new();
		timer.Tick += (_, _) => ticked.TrySetResult(Application.IsGuiThread);
		Assert.True(timer.Start(TimeSpan.FromMilliseconds(10), oneShot: true));
		Assert.True(await ticked.Task.WaitAsync(Gui.Timeout));
	});

	[Fact]
	public Task TicksRepeatedlyUntilStopped() => Gui.Run(async () => {
		using Timer timer = new();
		int ticks = 0;
		timer.Tick += (_, _) => ticks++;
		timer.Start(TimeSpan.FromMilliseconds(10));
		Assert.True(timer.IsRunning);
		await Gui.WaitUntil(() => ticks >= 3);
		timer.Stop();
		Assert.False(timer.IsRunning);
	});

	[Fact]
	public Task IsDestroyedOnceDisposed() => Gui.Run(() => {
		Timer timer = new();
		timer.Dispose();
		Assert.True(timer.IsDestroyed);
		Assert.Throws<ObjectDisposedException>(() => timer.Start(TimeSpan.FromSeconds(1)));
		timer.Dispose();
	});

	[Fact]
	public Task RefusesAnIntervalOfZero() => Gui.Run(() => {
		using Timer timer = new();
		Assert.Throws<ArgumentOutOfRangeException>(() => timer.Start(TimeSpan.Zero));
	});
}

public class DialogTests {
	[Fact]
	public Task CreatesAndDisposesAMessageDialog() => Gui.WithFrame(async frame => {
		MessageDialog dialog = new(frame, "Never shown", "Caption", MessageDialogStyle.YesNo | MessageDialogStyle.IconQuestion);
		Assert.False(dialog.IsDestroyed);
		dialog.Dispose();
		await Gui.WaitUntil(() => dialog.IsDestroyed);
		dialog.Dispose();
	});
}
