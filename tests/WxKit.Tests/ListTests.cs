namespace WxKit.Tests;

// List and tree controls, which most applications show their data in.
public class ListTests {
	[Fact]
	public Task FillsAReportListView() => Gui.WithFrame(frame => {
		ListView list = new(frame);
		list.InsertColumn(0, "Name");
		list.InsertColumn(1, "Size", ListColumnFormat.Right);
		Assert.Equal(2, list.ColumnCount);
		long first = list.InsertItem(0, "readme.md");
		list.SetItem(first, 1, "4 KB");
		list.InsertItem(1, "license");
		Assert.Equal(2, list.ItemCount);
		Assert.Equal("4 KB", list.GetItemText(first, 1));
		list.Select(1);
		Assert.True(list.IsSelected(1));
		Assert.Equal(1, list.FirstSelected);
	});

	[Fact]
	public Task BuildsATree() => Gui.WithFrame(frame => {
		TreeCtrl tree = new(frame);
		TreeItemId root = tree.AddRoot("Library");
		TreeItemId fiction = tree.AppendItem(root, "Fiction");
		TreeItemId novel = tree.AppendItem(fiction, "A novel");
		tree.AppendItem(root, "Poetry");
		Assert.True(root.IsOk);
		Assert.Equal(root, tree.RootItem);
		Assert.Equal(3, tree.GetChildrenCount(root));
		Assert.Equal(2, tree.GetChildrenCount(root, recursively: false));
		Assert.Equal(fiction, tree.GetItemParent(novel));
		Assert.Equal("A novel", tree.GetItemText(novel));
		Assert.False(default(TreeItemId).IsOk);
	});
}
