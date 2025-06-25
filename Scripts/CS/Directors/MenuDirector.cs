using Godot;

public partial class MenuDirector : HBoxContainer {
	[Export] TabContainer menuWindowContainer;
	[Export] OverlayWindow overlayWindow;
	public int MenuWindowIndex {
		get => menuWindowContainer.CurrentTab;
		private set {
			value = Mathf.Clamp(value, 0, menuWindowContainer.GetTabCount()-1);
			if (menuWindowContainer.CurrentTab == value) return;
			menuWindowContainer.CurrentTab = value;
			(menuWindowContainer.GetChild(value) as Control).GrabFocus();
		}
	}
	public override void _Ready() {
	}
	public override void _Process(double delta) {
	}
	public static int TERMINAL_INDEX = 0, EDITOR_INDEX = 1, BOTNET_INDEX = 2, CRAFTER_INDEX = 3;
	public void TerminalButtonPressed() {
		MenuWindowIndex = TERMINAL_INDEX;
	}
	public void EditorButtonPressed() {
		MenuWindowIndex = EDITOR_INDEX;
	}
	public void BotnetButtonPressed() {
		MenuWindowIndex = BOTNET_INDEX;
    }
	public void CrafterButtonPressed() {
        MenuWindowIndex = CRAFTER_INDEX;
    }
}
