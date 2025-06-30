using Godot;

public partial class EditorTab : TextEdit
{
	NodeFile file;
	TextEditor textEditor;
	public void Begin(NodeFile nodeFile, TextEditor textEditor) {
		file = nodeFile;
		Text = file.Content;
		this.textEditor = textEditor;
	}
	public override void _Process(double delta) {
		if (HasFocus()) {
			if (Input.IsActionJustPressed("saveFile")) { Save(); }
			else if (Input.IsActionJustPressed("closeTab")) { textEditor.CloseTab(false); }
			else if (file.Content != Text) textEditor.tabBar.SetTabTitle(textEditor.tabBar.CurrentTab, file.Name + "*");
			else textEditor.tabBar.SetTabTitle(textEditor.tabBar.CurrentTab, file.Name);
        }
	}
	public bool IsDirty() {
		return Text != file.Content;
	}
	public void Save() {
		file.Content = Text;
	}
}
