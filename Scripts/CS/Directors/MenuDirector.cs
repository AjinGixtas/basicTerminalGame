using Godot;

public partial class MenuDirector : HBoxContainer {
	[Export] TabContainer menuWindowContainer;
	[Export] OverlayWindow overlayWindow;
	int MenuWindowIndex {
		get => menuWindowContainer.CurrentTab;
		set {
			value = Mathf.Clamp(value, 0, menuWindowContainer.GetTabCount()-1);
			if (menuWindowContainer.CurrentTab == value) return;
			menuWindowContainer.CurrentTab = value;
		}
	}
	public override void _Ready() {
	}
	public override void _Process(double delta) {
	}
	readonly int TERMINAL_INDEX = 0, EDITOR_INDEX = 1;
	public void TerminalButtonPressed() {
		MenuWindowIndex = TERMINAL_INDEX;
	}
	public void EditorButtonPressed() {
		MenuWindowIndex = EDITOR_INDEX;
	}
}
