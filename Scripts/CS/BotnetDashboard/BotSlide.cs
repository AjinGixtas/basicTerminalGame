using Godot;

public partial class BotSlide : Button {
	[Export] RichTextLabel botName, lifetimeBar, mineSpeed;
	public void Update(BotFarm farm) {
		botName.Text = Util.Format(farm.HostName, StrType.HOSTNAME);
		int barAmount = Mathf.CeilToInt(farm.LifeTime / farm.MAX_LIFE_TIME * 50);
		lifetimeBar.Text = "[" + new string('|', barAmount) + new string('-', Mathf.Max(50 - barAmount, 0)) + "]";
		mineSpeed.Text = Util.Format($"{Mathf.Min(farm.GrowVal, farm.HackVal/farm.TimeVal)}", StrType.UNIT, "unit/s");
	}
}
