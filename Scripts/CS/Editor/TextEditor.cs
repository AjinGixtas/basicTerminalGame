using Godot;

public partial class TextEditor : MarginContainer {
	[Export] public RuntimeDirector overseer;
	[Export] TabBar tabBar; [Export] TabContainer tabContainer; [Export] PackedScene editorTabScene;
	[Export] RichTextLabel noFileOpenedTab;
	[Export] ConfirmationDialog saveDialog;
	public override void _Ready() {
		InitializeOnReadyVar();
		noFileOpenedTab.Text =
@$"No file opened.
Run {Util.Format("mkf FILENAME", StrType.CMD_FUL)} to make a new file.
Run {Util.Format("edit FILENAME", StrType.CMD_FUL)} to open that file here.";
	}
	void InitializeOnReadyVar() {
	}
	public void OpenNewFile(string HostName, NodeFile nodeFile) {
		tabBar.AddTab($"{nodeFile.Name}", null);
		EditorTab newTab = editorTabScene.Instantiate<EditorTab>();
		tabContainer.AddChild(newTab, true);
		newTab.Begin(nodeFile, this);
		if (tabContainer.CurrentTab == 0) tabContainer.CurrentTab = 1;
	}
	public void SaveFile() {
		if (tabBar.CurrentTab == -1) return; // No tab is selected
		EditorTab tab = tabContainer.GetChild<EditorTab>(tabContainer.CurrentTab);
		tab.Save();
	}
	public void CloseTab(bool forced) {
		if (tabBar.CurrentTab == -1) return; // No tab is selected
		EditorTab tab = tabContainer.GetChild<EditorTab>(tabContainer.CurrentTab);
		if (tab.IsDirty() && !forced) { saveDialog.Show(); saveDialog.GrabFocus(); return; }
		tabBar.RemoveTab(tabContainer.CurrentTab-1);
		tabContainer.RemoveChild(tab);
		if (tabBar.TabCount > 0) { 
			tabBar.CurrentTab = tabBar.TabCount - 1; 
			tabContainer.CurrentTab = tabContainer.GetTabCount()  - 1;
		}
	}
	public void ConfirmedClosing() {
		CloseTab(true);
	}
	public void ConfirmedCanceled() {
		saveDialog.Hide();
	}
	public void OnTabSelected(int tab) {
		tabContainer.CurrentTab = Mathf.Clamp(tab+1, 0, tabContainer.GetTabCount()-1);
	}
	public void OnFocusEntered() {
		(tabContainer.GetChild(Mathf.Max(0, tabBar.CurrentTab)) as Control).GrabFocus();
	}
}
