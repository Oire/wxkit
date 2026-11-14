using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WxKit.Interop;

namespace WxKit;

/// <summary>
/// A validator whose checks are written in C#. Derive from it and override the checks you need,
/// then set it as a control's <see cref="Window.Validator"/>. A dialog runs them when its OK button
/// is pressed, and <see cref="Window.Validate"/> runs them for a window's children.
/// </summary>
/// <remarks>
/// The control holds a copy of the validator, as wxWidgets does, and the copy calls this object,
/// so one validator can serve several controls. It stays alive for as long as any control holds a copy.
/// </remarks>
public abstract unsafe class CustomValidator : Validator {
	// The shim's handle is weak, so the validator keeps its holder alive itself.
	private readonly Target _target;

	protected CustomValidator() : this(new Target()) {
	}

	private CustomValidator(Target target) : base(CreateNative(target)) {
		_target = target;
		target.Validator = this;
	}

	/// <summary>Checks the control's value, and returns false when it is not valid, after telling the user why.</summary>
	/// <param name="window">The control the validator is set on.</param>
	/// <param name="parent">The window being validated, such as the dialog.</param>
	protected virtual bool OnValidate(Window window, Window? parent) => true;

	/// <summary>Puts the application's data into the control, such as when a dialog opens.</summary>
	protected virtual bool OnTransferToWindow(Window window) => true;

	/// <summary>Takes the control's value back into the application's data, such as when a dialog's OK button is pressed.</summary>
	protected virtual bool OnTransferFromWindow(Window window) => true;

	private static WxkObject* CreateNative(Target target) {
		Application.VerifyAccess();
		GCHandle weak = GCHandle.Alloc(target, GCHandleType.Weak);
		return WxKitNative.ManagedValidatorNew(&Validate, &TransferToWindow, &TransferFromWindow, &Retain, &Release, &Free, (void*)GCHandle.ToIntPtr(weak));
	}

	// The callbacks reach the managed validator through a weak handle, which the shim's copies make
	// strong while any control holds one. A validator that is gone, or a control WxKit cannot wrap,
	// passes, so a dialog is never stuck.
	private static Target? TargetOf(void* userdata) => (Target?)GCHandle.FromIntPtr((nint)userdata).Target;

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static byte Validate(void* userdata, WxkWindow* window, WxkWindow* parent) {
		try {
			if (TargetOf(userdata)?.Validator is not CustomValidator validator || ObjectRegistry.Wrap<Window>(window) is not Window control) return 1;
			return validator.OnValidate(control, ObjectRegistry.Wrap<Window>(parent)) ? (byte)1 : (byte)0;
		} catch (Exception error) {
			Application.ReportException(error);
			return 0;
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static byte TransferToWindow(void* userdata, WxkWindow* window) {
		try {
			if (TargetOf(userdata)?.Validator is not CustomValidator validator || ObjectRegistry.Wrap<Window>(window) is not Window control) return 1;
			return validator.OnTransferToWindow(control) ? (byte)1 : (byte)0;
		} catch (Exception error) {
			Application.ReportException(error);
			return 0;
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static byte TransferFromWindow(void* userdata, WxkWindow* window) {
		try {
			if (TargetOf(userdata)?.Validator is not CustomValidator validator || ObjectRegistry.Wrap<Window>(window) is not Window control) return 1;
			return validator.OnTransferFromWindow(control) ? (byte)1 : (byte)0;
		} catch (Exception error) {
			Application.ReportException(error);
			return 0;
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void Retain(void* userdata) {
		if (TargetOf(userdata) is not Target target) return;
		if (target.Copies++ == 0) target.Strong = GCHandle.Alloc(target);
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void Release(void* userdata) {
		if (TargetOf(userdata) is not Target target) return;
		if (--target.Copies == 0 && target.Strong.IsAllocated) target.Strong.Free();
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void Free(void* userdata) => GCHandle.FromIntPtr((nint)userdata).Free();

	// Holds the managed validator for the shim, and counts the controls' copies.
	private sealed class Target {
		public CustomValidator? Validator { get; set; }

		public int Copies { get; set; }

		public GCHandle Strong { get; set; }
	}
}

// A control's validator. The control holds its own copy, so a change to the validator after it is
// set does not reach the control; set it again instead.
public unsafe partial class Window {
	private Validator? _validator;

	/// <summary>
	/// The validator that checks the control's value, or null for none. The control holds a copy,
	/// as wxWidgets does, so set it again after changing it.
	/// </summary>
	public Validator? Validator {
		get {
			_ = Native;
			return _validator;
		}
		set {
			WxKitNative.WindowSetValidator(Native, value is null ? null : value.Native);
			_validator = value;
		}
	}
}
