using WxKit;

// With --smoke, the frame closes as soon as the event loop is running, so CI can check that a
// published executable starts and ends cleanly.
bool smoke = args is ["--smoke"];
return Application.Run(() => {
	Frame frame = new(null, $"Hello from WxKit and {Application.WxWidgetsVersion}");
	frame.Show();
	if (smoke) Application.Post(() => frame.Close(force: true));
});
