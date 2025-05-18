using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public static class PlayerData {
	static List<HackFarm> GC_miners = new();
	static double GC_max = 16_000_000;
	static double _gc_total = 0;
	static bool needWarn = false, warned = false;
	static double GC_total {
		get => _gc_total;
		set {
			_gc_total = Math.Clamp(value, 0, GC_max);
			if (_gc_total < GC_max) { needWarn = false; warned = false; } else needWarn = true;
			if (needWarn && !warned) {
				TerminalProcessor.Say($"[color={Util.CC(Cc.gR)}]Warning:[/color] GC total is over the limit of {GC_max}. Remaining GC lost.");
				warned = true;
			}
		}
	}
	public static double GC_Amount {
		get { return GC_total; }
	}
	static string _username = "AjinGixtas";
	public static string Username { 
		get => _username; 
		set { _username = value; GD.Print(_username); }
	}
	public static int LoadPlayerData(string filePath) {
		if (!Godot.FileAccess.FileExists(filePath)) { return 1; }
		PlayerDataSaveResource data = GD.Load<PlayerDataSaveResource>(filePath);
		GC_max = data.GC_max;
		GC_total = data.GC_total;
		Username = data.username;
		return 0;
	}
	public static void SavePlayerData(string filePath) {
		PlayerDataSaveResource data = new() { GC_max = GC_max, GC_total = GC_total, username = Username };
		GD.Print(Username);
		Error error = ResourceSaver.Save(data, filePath);
		if (error != Error.Ok) {
			GD.PrintErr($"Failed to save player data: {error}");
		}
	}
	public static void AddHackFarm(HackFarm h) {  GC_miners.Add(h); }
	public static void RemoveHackFarm(HackFarm h) { GC_miners.Remove(h); }

	// 0-Successful withdraw; 1-Invalid amount; 2-Not enough money
	public static int WithDraw(double amount) {
		if (amount < 0) { return 1; }
		if (amount > GC_total) { return 2; }
		GC_total -= amount;
		return 0;
	}
	// 0-Successful deposit; 1-Invalid amount
	public static int Deposit(double amount) {
		if (amount <= 0) { return 1; }
		GC_total += amount;
		return 0;
	}
	public static void Process(double delta) {
		foreach (HackFarm h in GC_miners) {
			GC_total += h.Process(delta);
		}
	}
}
