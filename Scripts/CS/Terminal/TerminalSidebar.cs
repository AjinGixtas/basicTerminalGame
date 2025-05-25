using Godot;
using System;

public partial class TerminalSidebar : MarginContainer {
	[Export] Timer statSwitchTimer;
	[Export] RichTextLabel artDiplayer;
	[Export] RichTextLabel playerStatDisplayer;
	[Export] TextEdit notePad;
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
    }
	int counter = 0;
	public void OnStatSwitchTimerTimeout() {
		++counter;
        artDiplayer.Text = $"[color=cyan]{animalSymbol[counter % animalSymbol.Length]}[/color]";
		playerStatDisplayer.Text = $"[color=cyan]{statFormater[counter % animalSymbol.Length]()}[/color]";
    }
	string GetBisonStatFormat() {
		return 
$@"
Total money: {Util.Format($"{PlayerDataManager.GC_Cur}", StrType.MONEY)} 
";
	}
	string GetRavenStatFormat() {
		return
$@"
Not implemented yet!
";
	}
	string GetSpiderStatFormat() {
		return
$@"
Not implemented yet!
";
	}
	string GetWolfStatFormat() {
		return
$@"
Not implemented yet!
";
	}
}
