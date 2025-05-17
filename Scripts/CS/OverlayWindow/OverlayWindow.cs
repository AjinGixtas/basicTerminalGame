using Godot;

public partial class OverlayWindow : Window {
	[Export] RichTextLabel moneyDisplay;
	public override void _Ready() {
		AddThemeIconOverride("close", new());
	}
	public override void _Process(double delta) {
		moneyDisplay.Text = 
@$"Money 
{Util.Format(PlayerData.GC_Amount.ToString(), StrType.MONEY)}
Production speed
{Util.Format(PlayerData.)}";
	}
}
