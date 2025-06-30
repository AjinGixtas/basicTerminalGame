using Godot;
using System.Linq;

public partial class BotStatusBoard : Control {
	System.WeakReference<BotFarm> botFarmRef = new(null);
	[Export] RichTextLabel leftLabel, rightLabel;
	public override void _Process(double delta) {
		botFarmRef.TryGetTarget(out BotFarm hackfarm);
		if (hackfarm == null) return;
		leftLabel.Text =
@$"[color={Util.CC(Cc.w)}]Botnet:[/color] {Util.Format(hackfarm.HostName, StrSty.HOSTNAME)}
================================

[color={Util.CC(Cc.w)}]Batch size[/color]    | Lvl.{Util.Format($"{hackfarm.BatchSizeLVL}", StrSty.NUMBER, "0"),-26} | {Util.Format($"{hackfarm.BatchSize}", StrSty.NUMBER):3}unit
[color={Util.CC(Cc.w)}]Mining speed[/color]  | Lvl.{Util.Format($"{hackfarm.MineSpeedLVL}", StrSty.NUMBER, "0"),-26} | {Util.Format($"{hackfarm.MineSpeed}", StrSty.NUMBER):3}u/s
[color={Util.CC(Cc.w)}]Transfer time[/color] | Lvl.{Util.Format($"{hackfarm.XferDelayLVL}", StrSty.NUMBER, "0"),-26} | {Util.Format($"{hackfarm.XferDelay}", StrSty.NUMBER):3}second

================================
[color={Util.CC(Cc.w)}]Time to live:[/color] {Util.Format($"{hackfarm.LifeTime}", StrSty.NUMBER)}s
";


		rightLabel.Text = 
@$"MINERAL BACKLOG
{CombineBox(hackfarm)}
Next batch in: {Util.Format($"{hackfarm.CycleTimeRemain}", StrSty.NUMBER, "2"), 6} 
{GenerateHackTimeBar(hackfarm)}
[color={Util.CC(Cc.w)}]Batch distribution:[/color] ({GenerateBatchDistVisual(hackfarm)} )";
		if (hackfarm.LifeTime <= 0) botFarmRef.SetTarget(null);
	}
	string CombineBox(BotFarm hackfarm) {
		string[][] boxs = new string[hackfarm.mineralDistribution.Length][];
		for (int i = 0; i < hackfarm.mineralDistribution.Length; ++i) {
			boxs[i] = GenerateAppendableBox(hackfarm, i);
		}
		string[] result = new string[5];
		result[0] = result[4] = $"[color={Util.CC(Cc.w)}]+[/color]";
		for (int i = 1; i < 4; ++i) result[i] = $"[color={Util.CC(Cc.w)}]|[/color]";
		for (int j = 0; j < hackfarm.mineralDistribution.Length; ++j) {
			for (int i = 0; i < 5; ++i) {
				result[i] += boxs[j][i];
			}
		}
		return result.Join("\n");
	}
	string[] GenerateAppendableBox(BotFarm hackfarm, int index) {
		string[] box = new string[5];
		string I = $"[color={Util.CC(Cc.w)}]|[/color]";
		box[0] = $"[color={Util.CC(Cc.w)}]------------+[/color]";
		box[1] = $"     {Util.Format($"{ItemCrafter.MINERALS[hackfarm.mineralDistribution[index].Item1].Shorthand}", StrSty.COLORED_ITEM_NAME, $"{hackfarm.mineralDistribution[index].Item1}")}    {I}";
		string content = (hackfarm.MBacklog * hackfarm.mineralDistribution[index].Item2).ToString("F2");
		int padLeft = (10 - content.Length) / 2; int padRight = 10 - content.Length - padLeft;
		box[2] = $" {new string(' ', padRight)}{Util.Format(content, StrSty.NUMBER)}{new string(' ', padLeft)} {I}";
		box[3] = $" {Util.Format($"{hackfarm.MineSpeed * hackfarm.mineralDistribution[index].Item2}", StrSty.NUMBER)+ "u/s", 33} {I}";
		box[4] = $"[color={Util.CC(Cc.w)}]------------+[/color]";
		return box;
	}
	string GenerateBatchDistVisual(BotFarm hackfarm) {
		string result = "";
		for (int i = 0; i < hackfarm.mineralDistribution.Length; ++i) {
			result += $" +{Util.Format($"{hackfarm.mineralDistribution[i].Item2 * hackfarm.BatchSize}", StrSty.NUMBER, "1")}[color={Util.CC(Cc.w)}]u[/color]{Util.Format($"{ItemCrafter.MINERALS[hackfarm.mineralDistribution[i].Item1].Shorthand}", StrSty.COLORED_ITEM_NAME, $"{hackfarm.mineralDistribution[i].Item1}")}";
		}
		return result;
	}

	string GenerateHackTimeBar(BotFarm hackfarm) {
		int barAmount = Mathf.CeilToInt(hackfarm.CycleTimeRemain / hackfarm.XferDelay * 50);
		return "[" + new string('|', Mathf.Max(50 - barAmount, 0)) + new string('-', barAmount) + "]";
	}
	public void ChangeFocusedBotFarm(BotFarm bot) {
		if (botFarmRef.TryGetTarget(out BotFarm cur)) {
			if (cur == null) return;
		}
		botFarmRef.SetTarget(bot);
	}
	public bool isRefNull() { return !botFarmRef.TryGetTarget(out BotFarm _); }
}
