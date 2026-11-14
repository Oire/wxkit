using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using WxKit.Interop;

namespace WxKit;

/// <summary>
/// The application: starts wxWidgets, runs the event loop, and ends when the last top-level window
/// closes. Derive from it to override its startup, or call <see cref="Run(Action)"/>.
/// </summary>
public unsafe class Application {
	private ExceptionDispatchInfo? _failure;

	/// <summary>The running application, or null when none is running.</summary>
	public static Application? Current { get; private set; }

	/// <summary>The version of wxWidgets that WxKit is built with, such as "wxWidgets 3.3.4".</summary>
	public static string WxWidgetsVersion => Text.Take(WxKitNative.VersionGetString());

	/// <summary>Runs an application whose startup is <paramref name="onInit"/>, and returns its exit code.</summary>
	public static int Run(Action onInit) {
		ArgumentNullException.ThrowIfNull(onInit);
		return new ActionApplication(onInit).Run();
	}

	/// <summary>
	/// Runs <paramref name="action"/> on the GUI thread once the event loop gets to it. Safe to call
	/// from any thread while the application is running.
	/// </summary>
	public static void Post(Action action) {
		ArgumentNullException.ThrowIfNull(action);
		GCHandle handle = GCHandle.Alloc(action);
		if (WxKitNative.AppCallAfter(&RunPosted, (void*)GCHandle.ToIntPtr(handle)) == 0) {
			handle.Free();
			throw new InvalidOperationException("No application is running.");
		}
	}

	/// <summary>
	/// Starts wxWidgets, calls <see cref="OnInit"/>, and runs the event loop. Returns the exit code.
	/// On Windows, the GUI needs a single-threaded apartment. Mark Main with <see cref="STAThreadAttribute"/>
	/// to run it on the calling thread; otherwise it runs on a thread of its own, and this call waits for it.
	/// </summary>
	public int Run() {
		if (OperatingSystem.IsWindows() && Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) return RunOnStaThread();
		return RunHere();
	}

	/// <summary>Creates the first windows. Returning false ends the application without running the event loop.</summary>
	protected virtual bool OnInit() => true;

	// OLE, which the clipboard, drag and drop and accessibility use, only works in a single-threaded
	// apartment. .NET starts Main in the multithreaded one unless it is marked [STAThread], which top-level
	// statements cannot be, and a thread's apartment cannot change once it is set. wxWidgets would then fail
	// to start OLE without saying so, so the GUI gets a thread that is set up right instead.
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private int RunOnStaThread() {
		int exitCode = 0;
		ExceptionDispatchInfo? failure = null;
		Thread thread = new(() => {
			try {
				exitCode = RunHere();
			} catch (Exception error) {
				failure = ExceptionDispatchInfo.Capture(error);
			}
		}) {
			Name = "WxKit GUI"
		};
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();
		failure?.Throw();
		return exitCode;
	}

	private int RunHere() {
		if (Current is not null) throw new InvalidOperationException("An application is already running.");
		Current = this;
		GCHandle self = GCHandle.Alloc(this);
		try {
			int exitCode = WxKitNative.AppRun(&OnInitCore, (void*)GCHandle.ToIntPtr(self));
			_failure?.Throw();
			return exitCode;
		} finally {
			self.Free();
			Current = null;
		}
	}

	// An exception cannot unwind back through wxWidgets, so the first one ends the event loop and is
	// rethrown from Run on the thread that called it.
	internal void Fail(Exception error) {
		if (_failure is not null) return;
		_failure = ExceptionDispatchInfo.Capture(error);
		WxKitNative.AppExitMainLoop();
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static byte OnInitCore(void* userdata) {
		Application application = (Application)GCHandle.FromIntPtr((nint)userdata).Target!;
		try {
			return application.OnInit() ? (byte)1 : (byte)0;
		} catch (Exception error) {
			application._failure = ExceptionDispatchInfo.Capture(error);
			return 0;
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void RunPosted(void* userdata) {
		GCHandle handle = GCHandle.FromIntPtr((nint)userdata);
		Action action = (Action)handle.Target!;
		handle.Free();
		try {
			action();
		} catch (Exception error) {
			Current?.Fail(error);
		}
	}

	private sealed class ActionApplication(Action onInit) : Application {
		protected override bool OnInit() {
			onInit();
			return true;
		}
	}
}
