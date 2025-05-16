using Godot;

public partial class OverlayWindow : Window {
	[Export] RichTextLabel moneyDisplay;
	public override void _Process(double delta) {
		moneyDisplay.Text = $"Money: {Util.Format(PlayerData.GC_Amount.ToString(), StrType.MONEY)}";
	}
}
