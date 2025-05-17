using Godot;

public partial class TextEditor : MarginContainer {
	[Export] public Overseer overseer;
	[Export] TabBar tabBar; [Export] TabContainer tabContainer; [Export] PackedScene editorTabScene;
	[Export] RichTextLabel noFileOpenedTab;
	[Export] ConfirmationDialog saveDialog;
	public override void _Ready() {
		InitializeOnReadyVar();
		noFileOpenedTab.Text =
@$"No file opened.
Run {Util.Format("mkf FILENAME", StrType.CMD)} to make a new file.
Run {Util.Format("edit FILENAME", StrType.CMD)} to open that file here.";
	}
	void InitializeOnReadyVar() {
	}
	public void OpenNewFile(string HostName, NodeFile nodeFile) {
		tabBar.AddTab($"{nodeFile.Name}", null);
		EditorTab newTab = editorTabScene.Instantiate<EditorTab>();
		tabContainer.AddChild(newTab, true);
		newTab.Begin(nodeFile, this);
	}
	public void SaveFile() {
		if (tabContainer.CurrentTab == -1) return; // No tab is selected
		EditorTab tab = tabContainer.GetChild<EditorTab>(tabContainer.CurrentTab);
		tab.Save();
	}
	public void CloseTab(bool forced) {
		if (tabContainer.CurrentTab == -1) return; // No tab is selected
		EditorTab tab = tabContainer.GetChild<EditorTab>(tabContainer.CurrentTab);
		if (tab.IsDirty() && !forced) { saveDialog.Show(); saveDialog.GrabFocus(); return; }
		tabBar.RemoveTab(tabContainer.CurrentTab);
		tabContainer.RemoveChild(tab);
		tabContainer.CurrentTab = tabBar.CurrentTab + 1;
	}
	public void ConfirmedClosing() {
		CloseTab(true);
	}
	public void ConfirmedCanceled() {
		saveDialog.Hide();
	}
	public void OnTabSelected(int tab) {
		tabContainer.CurrentTab = tab+1;
	}
}
