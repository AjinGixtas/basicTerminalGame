using Godot;
using System;
using System.Linq;
using System.Text;

public partial class TerminalSidebar : MarginContainer {
	[Export] Timer statSwitchTimer;
	[Export] RichTextLabel artDiplayer;
	[Export] RichTextLabel playerStatDisplayer;
	[Export] TextEdit notePad;
	[Export] RichTextLabel helpCmdSection;
    string[] animalSymbol = [];
	Func<string>[] statFormater = [];
	public override void _Ready() {
		animalSymbol = new string[4];
		animalSymbol[0] = Util.LoadUnicodeArt("res://Utilities/TextFiles/UnicodeArt/bison.txt");
		animalSymbol[1] = Util.LoadUnicodeArt("res://Utilities/TextFiles/UnicodeArt/raven.txt");
		animalSymbol[2] = Util.LoadUnicodeArt("res://Utilities/TextFiles/UnicodeArt/spider.txt");
		animalSymbol[3] = Util.LoadUnicodeArt("res://Utilities/TextFiles/UnicodeArt/wolf.txt");
		statFormater = new Func<string>[4];
		statFormater[0] = GetBisonStatFormat; statFormater[1] = GetRavenStatFormat; 
		statFormater[2] = GetSpiderStatFormat; statFormater[3] = GetWolfStatFormat;
		OnStatSwitchTimerTimeout();
		helpCmdSection.Text = $"Control this with {Util.Format("sidebar --help", StrSty.CMD_FUL)}";
    }
	int counter = 0;
	public void OnStatSwitchTimerTimeout() {
		counter = (counter + 1) % 4;
		artDiplayer.Text = $"[color=cyan]{animalSymbol[counter % animalSymbol.Length]}[/color]";
	}
	public override void _Process(double delta) {
		if (!Visible) return;
        playerStatDisplayer.Text = $"[color=cyan]{statFormater[counter % animalSymbol.Length]()}[/color]";
	}
	string GetBisonStatFormat() {
		StringBuilder sb = new();
		for (int i = 0; i < ItemCrafter.CurThreads; ++i) {
			if (ItemCrafter.CraftThreads[i].Recipe == null) {
				sb.Append($"{Util.Format("null", StrSty.DECOR)} [{new string('-', 24)}] 0%\n");
				continue;
			}
			sb.Append($"{Util.Format($"{ItemCrafter.CraftThreads[i].Recipe.Shorthand,-4}", StrSty.COLORED_ITEM_NAME, $"{ItemCrafter.CraftThreads[i].Recipe.ID}")} [{Util.GenerateSimpleBar(ItemCrafter.CraftThreads[i].Recipe.CraftTime - ItemCrafter.CraftThreads[i].RemainTime, ItemCrafter.CraftThreads[i].Recipe.CraftTime, length:24)}] {(1.0 - ItemCrafter.CraftThreads[i].RemainTime / ItemCrafter.CraftThreads[i].Recipe.CraftTime) * 100.0:F2}%\n");
		}
		return 
$@"Total money: {Util.Format($"{PlayerDataManager.GC_Cur}", StrSty.MONEY)} 
ThreadAmount: {Util.Format($"{ItemCrafter.CurThreads}", StrSty.NUMBER, "0")}
ThreadProgress:
{sb.ToString().TrimEnd()}";
	}
	string GetRavenStatFormat() {
        long inventoryEvaluation = (long)ItemCrafter.ALL_ITEMS.Select((item, i) => item.Value * PlayerDataManager.MineInv[i]).Sum();
        StringBuilder sb = new();
		foreach (BotFarm bot in NetworkManager.BotNet) {
			sb.Append($"[color={Util.CC(ItemCrafter.ALL_RECIPES[bot.mineralDistribution[0].Item1].ColorCode)}{(Mathf.RoundToInt(Mathf.Pow(bot.CycleTimeRemain / bot.XferDelay * 1.189207115002721, 32))).ToString("X2")}]█[/color]");
		}
        return
$@"InventoryEvaluation: {Util.Format($"{inventoryEvaluation}", StrSty.MONEY)}
ActiveBot: {Util.Format($"{NetworkManager.BotNet.Count}", StrSty.NUMBER, "0")}
{sb}";
	}
	string GetSpiderStatFormat() {
        return // Placeholder for spider stats
$@"BrainSlot: {Util.Format($"{0}", StrSty.NUMBER, "0")}";
	}
	string GetWolfStatFormat() {
        return // Placeholder for wolf stats
$@"PrismKeyCount: {Util.Format($"{0}", StrSty.NUMBER, "0")}";
	}
}
