using Godot;

public partial class TextEditor : MarginContainer {
	[Export] public Overseer overseer;
	[Export] TabBar tabBar; [Export] TabContainer tabContainer; [Export] PackedScene editorTabScene;
	public override void _Ready() {
		InitializeOnReadyVar();
	}
	void InitializeOnReadyVar() {
		tabBar = GetNode<TabBar>("VSplitContainer/TabBar");
		tabContainer = GetNode<TabContainer>("VSplitContainer/TabContainer");
		editorTabScene = GD.Load<PackedScene>("res://Scenes/EditorTab.tscn");
	}
	public void OpenNewFile(string HostName, NodeFile nodeFile) {
		tabBar.AddTab($"{HostName}:{nodeFile.Name}", null);
		EditorTab newTab = editorTabScene.Instantiate<EditorTab>();
		tabContainer.AddChild(newTab, true);
		newTab.Begin(nodeFile, this);
	}
	public void SaveFile() {
		if (tabContainer.CurrentTab == -1) return; // No tab is selected
		EditorTab tab = tabContainer.GetChild<EditorTab>(tabContainer.CurrentTab);
		tab.Save();
	}
	public void CloseTab() {
		if (tabContainer.CurrentTab == -1) return; // No tab is selected
		EditorTab tab = tabContainer.GetChild<EditorTab>(tabContainer.CurrentTab);
		if (tab.IsDirty()) { return; } // Disallow saving for now
		tabBar.RemoveTab(tabContainer.CurrentTab);
		tabContainer.RemoveChild(tab);
	}
}
