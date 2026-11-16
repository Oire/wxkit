using Settings;
using WxKit;

// A settings dialog, as most applications have: a notebook of pages, a form laid out with a grid
// bag sizer, pickers for a color, a font, and a folder, a page that scrolls, and OK and Cancel,
// with a validator that keeps the dialog open until the name is filled in. The settings are saved
// to a file in the user's application data. With --smoke, the sample opens the dialog and cancels
// it as soon as the event loop runs, then closes, so CI can check that a published sample makes
// every page and ends cleanly.
bool smoke = args is ["--smoke"];
return Application.Run(() => {
	MainFrame frame = new(AppSettings.Load(AppSettings.DefaultPath));
	frame.Show();
	if (smoke) {
		Application.Post(() => {
			frame.OpenAndCancelSettings();
			frame.Close(force: true);
		});
	}
});
