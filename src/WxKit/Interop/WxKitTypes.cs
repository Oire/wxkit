namespace WxKit.Interop;

// Each handle type is only ever used through a pointer. As in the shim, every handle points at the
// start of the wxWidgets object, so a pointer to a derived class's handle can be cast to a base's.

/// <summary>Any wxObject.</summary>
public struct WxkObject;

/// <summary>A wxEvtHandler, or any class derived from it.</summary>
public struct WxkEvtHandler;

/// <summary>A wxWindow, or any class derived from it.</summary>
public struct WxkWindow;

/// <summary>A wxSizer, or any class derived from it.</summary>
public struct WxkSizer;

/// <summary>A wxDataViewColumn.</summary>
public struct WxkDataViewColumn;

/// <summary>A wxMenu.</summary>
public struct WxkMenu;

/// <summary>A wxMenuItem.</summary>
public struct WxkMenuItem;

/// <summary>A wxTimer.</summary>
public struct WxkTimer;

/// <summary>A wxEvent, or any class derived from it. Only valid while its handlers run.</summary>
public struct WxkEvent;

/// <summary>A wxClassInfo, which lives as long as the program.</summary>
public struct WxkClassInfo;

/// <summary>Text the shim hands out, freed with <see cref="WxKitNative.StringFree"/>.</summary>
public struct WxkString;

/// <summary>A wxColour's channels, and whether it is set, as the shim passes them.</summary>
public readonly record struct WxkColor(byte Red, byte Green, byte Blue, byte Alpha, byte Valid);

/// <summary>A wxArrayString, freed with <see cref="WxKitNative.StringArrayFree"/>.</summary>
public struct WxkStringArray;

/// <summary>A list of windows, freed with <see cref="WxKitNative.WindowListFree"/>.</summary>
public struct WxkWindowList;

/// <summary>A wxVariant, freed with <see cref="WxKitNative.VariantFree"/>.</summary>
public struct WxkVariant;

/// <summary>A wxVector of wxVariant, freed with <see cref="WxKitNative.VariantListFree"/>.</summary>
public struct WxkVariantList;

/// <summary>A wxDataViewModel, which is reference counted.</summary>
public struct WxkDataViewModel;

/// <summary>The wxDataViewItemArray a model fills with an item's children.</summary>
public struct WxkDataViewItems;

/// <summary>The types of value a wxVariant crosses with, as the shim numbers them.</summary>
public enum WxkVariantType {
	Null = 0,
	String = 1,
	Bool = 2,
	Long = 3,
	Double = 4,
	Bitmap = 5,
	Icon = 6,
	IconText = 7,
}

/// <summary>The callbacks of every model whose items come from .NET, as wxk_DataViewModelCallbacks lays them out.</summary>
public unsafe struct WxkDataViewModelCallbacks {
	public delegate* unmanaged[Cdecl]<void*, nint, uint, WxkVariant*, void> GetValue;
	public delegate* unmanaged[Cdecl]<void*, WxkVariant*, nint, uint, byte> SetValue;
	public delegate* unmanaged[Cdecl]<void*, nint, nint> GetParent;
	public delegate* unmanaged[Cdecl]<void*, nint, byte> IsContainer;
	public delegate* unmanaged[Cdecl]<void*, nint, WxkDataViewItems*, void> GetChildren;
	public delegate* unmanaged[Cdecl]<void*, nint, uint, byte> IsEnabled;
	public delegate* unmanaged[Cdecl]<void*, nint, byte> HasContainerColumns;
	public delegate* unmanaged[Cdecl]<void*, void> OnFree;
}

/// <summary>The callbacks of every virtual list model whose rows come from .NET, as wxk_VirtualListModelCallbacks lays them out.</summary>
public unsafe struct WxkVirtualListModelCallbacks {
	public delegate* unmanaged[Cdecl]<void*, uint, uint, WxkVariant*, void> GetValue;
	public delegate* unmanaged[Cdecl]<void*, WxkVariant*, uint, uint, byte> SetValue;
	public delegate* unmanaged[Cdecl]<void*, uint, uint, byte> IsEnabled;
	public delegate* unmanaged[Cdecl]<void*, void> OnFree;
}

/// <summary>An accessible object, which its window owns.</summary>
public struct WxkAccessible;

/// <summary>The callbacks every managed accessible object calls, as the shim's wxk_AccessibleCallbacks lays them out.</summary>
public unsafe struct WxkAccessibleCallbacks {
	public delegate* unmanaged[Cdecl]<void*, int, int, WxkString**, int> GetText;
	public delegate* unmanaged[Cdecl]<void*, int, int*, int> GetRole;
	public delegate* unmanaged[Cdecl]<void*, int, long*, int> GetState;
	public delegate* unmanaged[Cdecl]<void*, int*, int> GetChildCount;
	public delegate* unmanaged[Cdecl]<void*, int, Rect*, int> GetLocation;
	public delegate* unmanaged[Cdecl]<void*, int, int, int, Point, int*, WxkAccessible**, int> GetElement;
	public delegate* unmanaged[Cdecl]<void*, int, int, int, int> Act;
	public delegate* unmanaged[Cdecl]<void*, void> OnFree;
}

/// <summary>Clipboard content in several formats, which <see cref="WxKitNative.ClipboardSetContent"/> takes ownership of.</summary>
public struct WxkClipboardContent;

/// <summary>The standard clipboard formats, as the shim numbers them.</summary>
public enum WxkClipboardFormat {
	Text = 0,
	Bitmap = 1,
	Files = 2,
	Html = 3,
}
