using Godot;
public partial class RuntimeDirector : Control {
	[Export] public Terminal terminal;
	[Export] public TextEditor textEditor;
	[Export] public OverlayWindow overlayWindow;
	[Export] public PackedScene notificationScene;
	public void MakeNotification(string msg) {
		NotifyBoxBehaviour notifBox = notificationScene.Instantiate<NotifyBoxBehaviour>();
		notifBox.ShowNotification(msg);
		AddChild(notifBox);
	}
}
