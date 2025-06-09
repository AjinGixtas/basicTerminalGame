using Godot;

public partial class EditorTab : CodeEdit
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
			if (Input.IsActionJustPressed("closeTab")) { textEditor.CloseTab(false); }
			if (Input.IsActionJustPressed("saveFile")) { textEditor.SaveFile(); }
		}
	}
	public bool IsDirty() {
		return Text != file.Content;
	}
	public void Save() {
		file.Content = Text;
		LifeCycleDirector.SaveFileSystem();
	}
	public void OnCodeCompletionRequested() {
		AddCodeCompletionOption(CodeCompletionKind.Function, "ax:Say", "ax:Say");
		UpdateCodeCompletionOptions(false);
	}
	public void OnTextChanged() {
		RequestCodeCompletion();
		FoldAllLines();
	}
}
