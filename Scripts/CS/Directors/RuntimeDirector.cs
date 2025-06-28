using Godot;
public partial class RuntimeDirector : Control {
	[Export] public Terminal terminal;
	[Export] public TextEditor textEditor;
	[Export] public OverlayWindow overlayWindow;
	[Export] public PackedScene notificationScene;
	public static void MakeNotification(string msg) {
		NotifyBoxBehaviour notifBox = ShellCore.Overseer.notificationScene.Instantiate<NotifyBoxBehaviour>();
		notifBox.ShowNotification(msg);
        ShellCore.Overseer.AddChild(notifBox);
	}
}
