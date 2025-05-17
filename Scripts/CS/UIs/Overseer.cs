using Godot;
public partial class Overseer : Control {
	[Export] public Terminal terminal;
	[Export] public TextEditor textEditor;
	[Export] public SaveManager saveManager;
	public override void _Ready() {
	}
}
