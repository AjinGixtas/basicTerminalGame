using Godot;

public partial class EditorTab : CodeEdit
{
    NodeFile file;
    public void Begin(NodeFile nodeFile) {
        file = nodeFile;
        Text = file.Content;
    }
    public bool IsDirty() {
        return Text != file.Content;
    }
    public void Save() {
        file.Content = Text;
    }
}
