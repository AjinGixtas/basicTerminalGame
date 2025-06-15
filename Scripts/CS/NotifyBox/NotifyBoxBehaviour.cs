using Godot;

public partial class NotifyBoxBehaviour : Control {
	[Export] MarginContainer NotificationBox;
	[Export] RichTextLabel NotificationLabel;
	[Export] Button CloseButton;
	[Export] ColorRect Background;
	[Export] AnimationPlayer AnimationPlayer;
	public void ShowNotification(string message, StrType boxStyle, string bgC = "0xDEADBEEF") {
		NotificationLabel.Text = message;
		if (bgC != "0xDEADBEEF") Background.Color = new Color(bgC);
		AnimationPlayer.Play("Popup");
    }
	public void HideNotification() {
		this.Visible = false;
	}
}
