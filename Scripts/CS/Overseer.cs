using Godot;
public partial class Overseer : Control {
    public Terminal terminal;
    public TextEditor textEditor;
    public override void _Ready() {
        IntializeOnReadyVar();
    }
    void IntializeOnReadyVar() {
        terminal = GetNode<Terminal>("HSplitContainer/TabContainer/Terminal");
        textEditor = GetNode<TextEditor>("HSplitContainer/TabContainer/TextEditor");
        terminal.overseer = this;
        textEditor.overseer = this;
    }
}