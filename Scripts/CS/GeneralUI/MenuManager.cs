using Godot;

public partial class MenuManager : HBoxContainer {
    TabContainer tabContainer;
    public override void _Ready() {
        IntializeOnReadyVar();
    }
    void IntializeOnReadyVar() {
        tabContainer = GetNode<TabContainer>("TabContainer");
    }
    public override void _Process(double delta) {
    }
    readonly int TERMINAL_INDEX = 0, EDITOR_INDEX = 1;
    public void TerminalButtonPressed() {
        tabContainer.CurrentTab = TERMINAL_INDEX;
    }
    public void EditorButtonPressed() {
        tabContainer.CurrentTab = EDITOR_INDEX;
    }
}
