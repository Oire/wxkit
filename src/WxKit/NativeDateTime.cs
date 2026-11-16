using WxKit.Interop;

namespace WxKit;

// A wxDateTime is a DateTime in local time, and wxInvalidDateTime is null. A UTC time goes in as
// the local time it is.
internal static class NativeDateTime {
	internal static WxkDateTime ToNative(DateTime? value) {
		if (value is not DateTime date) return default;
		if (date.Kind == DateTimeKind.Utc) date = date.ToLocalTime();
		return new WxkDateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, 1);
	}

	internal static DateTime? FromNative(WxkDateTime value) => value.Valid != 0 ? new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, DateTimeKind.Local) : null;
}
