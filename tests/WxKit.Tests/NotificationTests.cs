namespace WxKit.Tests;

// Notifications, which the tests make and free without showing, since a notification posted from a
// test run can stay on the desktop, and macOS only posts them from an app bundle.
public class NotificationTests {
	[Fact]
	public Task MakesAndFreesANotification() => Gui.WithFrame(frame => {
		NotificationMessage notification = new("Saved", "Your tasks are saved.", frame, NotificationIcon.Warning);
		notification.SetTitle("Backed up");
		notification.SetMessage("Your tasks are backed up.");
		notification.SetFlags(NotificationIcon.Error);
		notification.Clicked += (_, _) => { };
		Assert.False(notification.IsDestroyed);
		notification.Dispose();
		Assert.True(notification.IsDestroyed);
		notification.Dispose();
	});
}
