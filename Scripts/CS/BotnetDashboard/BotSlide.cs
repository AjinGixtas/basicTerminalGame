using Godot;

public partial class BotSlide : Control {
	[Export] RichTextLabel botName, hackTimeBar, lifeTimeDisplay;
	BotnetDashboard _director;
	public BotnetDashboard Director { get => _director; set => _director ??= value; }
	public int ID = 0;
	public void Update(BotFarm farm) {
		botName.Text = Util.Format(farm.IP, StrSty.IP).PadLeft(38) + $" {Util.Format("|", StrSty.DECOR)} " + Util.Format(farm.HostName, StrSty.HOSTNAME);
		int barAmount = Mathf.CeilToInt(farm.CycleTimeRemain / farm.XferDelay * 50);
		hackTimeBar.Text = "     [" + new string('|', Mathf.Max(50-barAmount,0)) + new string('-', barAmount) + "]";
		lifeTimeDisplay.Text = Util.Format($"{farm.LifeTime}", StrSty.NUMBER, "3") + "s TTL";
	}
	public void OnPressed() {
		_director.OnBotSlidePressed(ID);
	}
}
