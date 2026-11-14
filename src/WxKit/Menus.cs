using WxKit.Interop;

namespace WxKit;

/// <summary>The bar of menus across the top of a frame. Wraps wxMenuBar. Set it as a frame's <see cref="Frame.MenuBar"/>.</summary>
public sealed unsafe class MenuBar : Window {
	public MenuBar() : base(WxKitNative.MenuBarNew(0)) {
	}

	internal MenuBar(WxkWindow* native) : base(native) {
	}

	/// <summary>Adds a menu, which the menu bar then owns. Put an ampersand before the letter that opens it, as in "&amp;File".</summary>
	public void Append(Menu menu, string title) {
		ArgumentNullException.ThrowIfNull(menu);
		ArgumentNullException.ThrowIfNull(title);
		fixed (char* text = title) {
			if (WxKitNative.MenuBarAppend(Native, menu.Native, text, Text.Length(title)) == 0) throw new InvalidOperationException("wxWidgets could not add the menu.");
		}
	}
}

/// <summary>
/// A menu, on a menu bar or inside another menu. Wraps wxMenu. Once appended to a menu bar, the
/// menu bar owns it. A menu that is never appended is not freed yet.
/// </summary>
public sealed unsafe class Menu : EvtHandler {
	public Menu() : base((WxkEvtHandler*)WxKitNative.MenuNew()) {
	}

	internal Menu(WxkMenu* native) : base((WxkEvtHandler*)native) {
	}

	internal WxkMenu* Native => (WxkMenu*)Handle;

	/// <summary>
	/// Adds an item. With a standard id, such as <see cref="StandardId.Exit"/>, and no text, the item
	/// gets the platform's standard text, and on macOS goes where the platform puts it. A tab in the
	/// text starts its keyboard shortcut, as in "&amp;Open\tCtrl+O".
	/// </summary>
	public MenuItem Append(int id, string text = "", string help = "", MenuItemKind kind = MenuItemKind.Normal) {
		ArgumentNullException.ThrowIfNull(text);
		ArgumentNullException.ThrowIfNull(help);
		fixed (char* itemText = text)
		fixed (char* helpText = help) {
			return MenuItem.Wrap(this, WxKitNative.MenuAppend(Native, id, itemText, Text.Length(text), helpText, Text.Length(help), (int)kind));
		}
	}

	public MenuItem AppendSeparator() => MenuItem.Wrap(this, WxKitNative.MenuAppendSeparator(Native));
}

/// <summary>An item on a menu. Wraps wxMenuItem. Its menu owns it.</summary>
public sealed unsafe class MenuItem {
	private MenuItem(Menu menu, int id) {
		Menu = menu;
		Id = id;
	}

	public Menu Menu { get; }

	/// <summary>The item's id, which wxWidgets picks when the item was added with <see cref="StandardId.Any"/>.</summary>
	public int Id { get; }

	/// <summary>Raised when the user chooses the item.</summary>
	public event EventHandler<CommandEvent>? Click {
		add => Menu.AddHandler(EventType.Menu, Id, Id, value, this);
		remove => Menu.RemoveHandler(EventType.Menu, Id, Id, value);
	}

	internal static MenuItem Wrap(Menu menu, WxkMenuItem* native) {
		if (native is null) throw new InvalidOperationException("wxWidgets could not add the menu item.");
		return new MenuItem(menu, WxKitNative.MenuItemGetId(native));
	}
}
