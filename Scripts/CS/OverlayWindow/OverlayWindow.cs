using Godot;

public partial class OverlayWindow : Window {
	[Export] RichTextLabel moneyDisplay;
	public override void _Process(double delta) {
		moneyDisplay.Text = 
@$"Money 
{Util.Format($"{PlayerDataManager.GC_Cur}", StrType.MONEY)}
Production speed
{"pootis"}";
	}
}
