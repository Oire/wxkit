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

	[Fact]
	public Task GivesATreeItsImages() => Gui.WithFrame(frame => {
		TreeCtrl tree = new(frame);
		using Bitmap folder = new(16, 16);
		using Bitmap file = new(16, 16);
		tree.SetImages([folder, file]);
		Assert.Equal(2, tree.ImageCount);
		TreeItemId root = tree.AddRoot("Library", image: 0);
		TreeItemId novel = tree.AppendItem(root, "A novel", image: 1);
		Assert.Equal(0, tree.GetItemImage(root));
		Assert.Equal(1, tree.GetItemImage(novel));
		Assert.Throws<ArgumentNullException>(() => tree.SetImages([folder, null!]));
	});

	[Fact]
	public Task GivesAListViewItsImages() => Gui.WithFrame(frame => {
		ListView list = new(frame);
		using Bitmap icon = new(16, 16);
		list.SetSmallImages([icon]);
		list.SetNormalImages([icon]);
		list.InsertColumn(0, "Name");
		Assert.Equal(0, list.InsertItem(0, "readme.md", 0));
		Assert.True(list.SetItemImage(0, 0));
	});
}
