using WxKit.Interop;

namespace WxKit;

// A wxVariant, which crosses as an object: null, a string, a bool, a long, a double, a Bitmap, an
// Icon, or a DataViewIconText. Other numbers and enums go in as a long or a double, and any other
// type wxWidgets hands back reads as its text.
internal readonly unsafe ref struct NativeVariant {
	public NativeVariant(object? value) {
		Handle = WxKitNative.VariantNew();
		try {
			Set(Handle, value);
		} catch {
			WxKitNative.VariantFree(Handle);
			throw;
		}
	}

	public WxkVariant* Handle { get; }

	public void Dispose() => WxKitNative.VariantFree(Handle);

	public static void Set(WxkVariant* variant, object? value) {
		switch (value) {
			case null:
				WxKitNative.VariantSetNull(variant);
				break;
			case string text:
				fixed (char* data = text) WxKitNative.VariantSetString(variant, data, NativeString.Length(text));
				break;
			case bool flag:
				WxKitNative.VariantSetBool(variant, flag ? (byte)1 : (byte)0);
				break;
			case int or long or short or sbyte or byte or ushort or uint or Enum:
				WxKitNative.VariantSetLong(variant, Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture));
				break;
			case double or float or decimal:
				WxKitNative.VariantSetDouble(variant, Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture));
				break;
			case Bitmap or Icon or DataViewIconText:
				WxKitNative.VariantSetObject(variant, ((NativeObject)value).Native);
				break;
			default:
				throw new ArgumentException($"WxKit cannot hand wxWidgets a {value.GetType()} as a value. Use null, a string, a bool, a number, a Bitmap, an Icon, or a DataViewIconText.", nameof(value));
		}
	}

	public static object? Get(WxkVariant* variant) => (WxkVariantType)WxKitNative.VariantGetType(variant) switch {
		WxkVariantType.Null => null,
		WxkVariantType.Bool => WxKitNative.VariantGetBool(variant) != 0,
		WxkVariantType.Long => WxKitNative.VariantGetLong(variant),
		WxkVariantType.Double => WxKitNative.VariantGetDouble(variant),
		WxkVariantType.Bitmap => Bitmap.Take(WxKitNative.VariantGetObject(variant)),
		WxkVariantType.Icon => Icon.Take(WxKitNative.VariantGetObject(variant)),
		WxkVariantType.IconText => DataViewIconText.Take(WxKitNative.VariantGetObject(variant)),
		_ => NativeString.Take(WxKitNative.VariantGetString(variant)),
	};

	// Takes ownership of a variant the shim hands out, reads it, and frees it.
	public static object? Take(WxkVariant* variant) {
		if (variant is null) return null;
		try {
			return Get(variant);
		} finally {
			WxKitNative.VariantFree(variant);
		}
	}
}

// A wxVector<wxVariant>, such as a data view list row's values, built for the length of one call.
internal readonly unsafe ref struct NativeVariantList {
	public NativeVariantList(IEnumerable<object?> values) {
		ArgumentNullException.ThrowIfNull(values);
		Handle = WxKitNative.VariantListNew();
		try {
			foreach (object? value in values) {
				using NativeVariant variant = new(value);
				WxKitNative.VariantListAdd(Handle, variant.Handle);
			}
		} catch {
			WxKitNative.VariantListFree(Handle);
			throw;
		}
	}

	public WxkVariantList* Handle { get; }

	public void Dispose() => WxKitNative.VariantListFree(Handle);
}
