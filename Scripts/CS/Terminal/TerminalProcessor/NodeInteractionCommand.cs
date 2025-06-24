using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static partial class ShellCore {
	static void Farm(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		bool listAll = parsedArgs.ContainsKey("-a") || parsedArgs.ContainsKey("--all");
		string[] botnetNames = NetworkManager.GetBotsName();
		if (listAll) {
			for (int i = 0; i < botnetNames.Length; i++) Say(botnetNames[i]);
			return;
		}

		parsedArgs.TryGetValue("-h", out string minerName);
		if (string.IsNullOrEmpty(minerName)) parsedArgs.TryGetValue("--host", out minerName);
		if (string.IsNullOrEmpty(minerName)) { Say("-r", "No miner name provided."); return; }
		BotFarm hackfarm = NetworkManager.GetBotByName(minerName);
		if (hackfarm == null) { Say("-r", "No miner with such name."); return; }
		Say(
$@"[color={Util.CC(Cc.w)}]Botnet:[/color] {Util.Format(minerName, StrSty.HOSTNAME)}
{Util.GenerateStringByProportions([.. hackfarm.mineralDistribution.Select(x => x.Item2)], [.. hackfarm.mineralDistribution.Select(x => ItemCrafter.MINERALS[x.Item1].ColorCode)], 50)}
{string.Join(" | ", hackfarm.mineralDistribution.Select(x => $"[color={Util.CC(ItemCrafter.MINERALS[x.Item1].ColorCode)}]{ItemCrafter.MINERALS[x.Item1].Shorthand}[/color]: {Util.Format($"{x.Item2 * 100.0:0.00}", StrSty.NUMBER)}%"))}
================================================================
[color={Util.CC(Cc.w)}]Batch size[/color]    | Lvl.{Util.Format($"{hackfarm.BatchSizeLVL}", StrSty.NUMBER, "0"), 26} | {Util.Format($"{hackfarm.BatchSize}", StrSty.NUMBER):3}
[color={Util.CC(Cc.w)}]Mining speed[/color]  | Lvl.{Util.Format($"{hackfarm.MineSpeedLVL}", StrSty.NUMBER, "0"), 26} | {Util.Format($"{hackfarm.MineSpeed}", StrSty.NUMBER):3}
[color={Util.CC(Cc.w)}]Transfer time[/color] | Lvl.{Util.Format($"{hackfarm.XferDelayLVL}", StrSty.NUMBER, "0"), 26} | {Util.Format($"{hackfarm.XferDelay}", StrSty.NUMBER):3}
================================================================
[color={Util.CC(Cc.w)}]Aprox. Time To Live:[/color] {Util.TimeDifferenceFriendly(hackfarm.LifeTime)}");
	}
	const double FLARE_TIME = 20.0;
	static double startEpoch = 0, endEpoch = 0, remainingTime = 0;
	public static double StartEpoch { get => startEpoch; private set => startEpoch = value; }
    public static double EndEpoch { get => endEpoch; private set => endEpoch = value; }
    public static double RemainingTime { get => remainingTime; private set => remainingTime = value; }
    /// <summary>
    /// Weak reference sectors that have been attacked during the Karaxe flare.
    /// All sectors will be disconnected and removed when Karaxe is deactivated.
	/// </summary>
    static List<WeakReference<DriftSector>> sectorAttacked = [];
    /// <summary>
    /// Event that is invoked when the karaxe command is run from the UI.
	/// <code>result[i] = (CrackStatus, LockName, LockFlag, LockInput)</code>
	/// For non-attack commands, the first item in the tuple will be <see cref="CError.UNKNOWN"/>.
	/// and the other items will be empty strings. With <c>LockInput</c> being the command details.
    /// </summary>
    public static event Action<(CError, string, string, string)[]> KARrunCMD;
	static void Karaxe(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		if (parsedArgs.ContainsKey("--axe") && parsedArgs.ContainsKey("--flare")) {
			Say("-r", "You can not use --axe and --flare at the same time.");
			KARrunCMD?.Invoke([(CError.UNKNOWN, "", "", "--flare and --axe incompatible")]);
			return;
		}
        if (parsedArgs.ContainsKey("--flare")) {
			int statusCode = BeginFlare();
			switch (statusCode) {
				case 0: {
						Say($"{Util.Format("Kraken activated", StrSty.FULL_SUCCESS)}. All node [color={Util.CC(Cc.Y)}]lock[/color] system {Util.Format("~=EXP0SED=~", StrSty.ERROR)}");
						KARrunCMD?.Invoke([(CError.UNKNOWN, "", "", "Flare start")]);
						break;
					}
				case 1: {
						Say("-r", "Karaxe already in effect."); 
						KARrunCMD?.Invoke([(CError.UNKNOWN, "", "", "Flare start|FAILED")]);
						break;
                    }
            }
			return;
		}
		if (parsedArgs.ContainsKey("--axe")) {
			if (Time.GetUnixTimeFromSystem() > endEpoch) { 
				Say("-r", "Karaxe already deactivated."); 
				KARrunCMD?.Invoke([(CError.UNKNOWN, "", "", "Flare stop|FAILED")]);
                return;
			}
			Say($"{Util.Format("Kraken deactivated", StrSty.FULL_SUCCESS)}. Flare sequence exited. All [color={Util.CC(Cc.Y)}]lock[/color] closed.");
            KARrunCMD?.Invoke([(CError.UNKNOWN, "", "", "Flare stop")]);
            EndFlare(); return;
		}

		if (Time.GetUnixTimeFromSystem() > endEpoch) {
			Say($"{Util.Format("Kraken inactive", StrSty.ERROR)}. Run {Util.Format("karaxe --flare", StrSty.CMD_FUL)} to activate.");
            KARrunCMD?.Invoke([(CError.UNKNOWN, "", "", "Flare inactive")]);
            return;
		}

		if (!parsedArgs.ContainsKey("--attack")) {
			Say("-r", $"Missing {Util.Format("--attack", StrSty.CMD_FLAG)} flag. Use {Util.Format("--attack", StrSty.CMD_FLAG)} to attack a node.");
            KARrunCMD?.Invoke([(CError.UNKNOWN, "", "", "Missing --attack")]);
            return;
		}
		(CError, string, string, string)[] result = Attack(parsedArgs);
		KARrunCMD?.Invoke(result);
		for (int i = 0; i < result.Length; ++i) {
			(CError, string, string, string) res = result[i];
            if (res.Item1 == CError.OK) {
                if (i != result.Length - 1) {
                    ShellCore.Say($"[{Util.Format("SUCCESS", StrSty.PART_SUCCESS)}] {Util.Format("Bypassed", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.CMD_FLAG)}");
                    continue;
                }
                ShellCore.Say($"{Util.Format("Node defense cracked.", StrSty.FULL_SUCCESS)} All security system [color={Util.CC(Cc.gR)}]destroyed[/color].");
                ShellCore.Say($"Run {Util.Format("analyze", StrSty.CMD_FUL)} for all new information.");
                continue;
            }
            if (res.Item1 == CError.MISSING) {
				ShellCore.Say($"[{Util.Format("N0VALUE", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.CMD_FLAG)}");
				ShellCore.Say("-r", $"Missing key for {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
				ShellCore.Say($"[color={Util.CC(Cc.W)}]{res.Item4}[/color]");
				continue;
			} 
			if (res.Item1 == CError.INCORRECT) {
				ShellCore.Say($"[{Util.Format("WRON6KY", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.CMD_FLAG)}");
				ShellCore.Say("-r", $"Incorrect key for {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
				ShellCore.Say($"[color={Util.CC(Cc.W)}]{res.Item4}[/color]");
				continue;
			} 
			if (res.Item1 == CError.MISSING) {
				ShellCore.Say($"[{Util.Format("MI55ING", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.CMD_FLAG)}");
				ShellCore.Say("-r", $"Missing flag {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
				continue;
			} 
			if (res.Item1 == CError.NO_PERMISSION) {
				ShellCore.Say($"[{Util.Format("N0P3RMS", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.CMD_FLAG)}");
				ShellCore.Say("-r", $"You do not have permission to use {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
				continue;
			}
			ShellCore.Say($"[{Util.Format("UNK??WN", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.CMD_FLAG)}");
			ShellCore.Say("-r", $"Unknown error for {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
		}
	}
	public static event Action KaraxeEnd;
    public static void EndFlare() {
		endEpoch = 0;
		crackDurationTimer.Stop();
		foreach (WeakReference<DriftSector> sectorRef in sectorAttacked) {
			if (!sectorRef.TryGetTarget(out DriftSector sector)) continue;
			NetworkManager.DisconnectFromSector(sector);
			NetworkManager.RemoveSector(sector);
		}
		sectorAttacked.Clear();
		KaraxeEnd?.Invoke();
    }
	public static event Action<(CError, string, string, string)[]> KaraxeAttack;
    /// <summary>
    /// Attempt to crack the node with the provided answers.
    /// </summary>
    /// <param name="flagKeyPairs"></param>
    /// <returns><code>result[i] = (CrackStatus, LockName, LockFlag, LockInput)</code></returns>
    public static (CError, string, string, string)[] Attack(Dictionary<string, string> flagKeyPairs) {
		(CError, string, string, string)[] result = CurrNode.AttemptCrackNode(flagKeyPairs, endEpoch);
		if (CurrNode.GetType() == typeof(DriftNode)) {
			// No idea why, but hard reference to DriftSector is not working here.
			sectorAttacked.Add(new WeakReference<DriftSector>((CurrNode as DriftNode).Sector));
		}
		if (result[^1].Item1 == CError.OK && !CurrNode.OwnedByPlayer) {
			CurrNode.TransferOwnership();
			if (CurrNode.GetType() == typeof(DriftNode)) NetworkManager.QueueAddHackFarm((CurrNode as DriftNode).HackFarm);
		}
        KaraxeAttack?.Invoke(result);
        return result;
	}
	public static event Action KaraxeBegin;
    public static int BeginFlare() {
		if (Time.GetUnixTimeFromSystem() < endEpoch) { return 1; }
		startEpoch = Time.GetUnixTimeFromSystem(); remainingTime = FLARE_TIME; endEpoch = startEpoch + remainingTime;
		crackDurationTimer.Start(FLARE_TIME);
		KaraxeBegin?.Invoke();
		return 0;
	}
	
	public static event Action<CError, string> ANALYZErunCMD;
    static void Analyze(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
		if (positionalArgs.Length == 0) { positionalArgs = [CurrNode.HostName]; }
		if (CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]) == null
			&& (CurrNode.ParentNode != null && CurrNode.ParentNode.HostName != positionalArgs[0])
			&& CurrNode.HostName != positionalArgs[0]) {
            Say("-r", $"Host not found: {positionalArgs[0]}");
            ANALYZErunCMD?.Invoke(CError.NOT_FOUND, positionalArgs[0]);
            return;
		}
		NetworkNode analyzeNode = null;
		if (CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]) != null) analyzeNode = CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]);
		else if (CurrNode.ParentNode != null && CurrNode.ParentNode.HostName == positionalArgs[0]) analyzeNode = CurrNode.ParentNode;
		else if (CurrNode.HostName == positionalArgs[0]) analyzeNode = CurrNode;
		const int padLength = -40;
		Say("-tl", $@"
{Util.Format("▶ Node Info", StrSty.HEADER)}
{Util.Format("Host name:", StrSty.DECOR),padLength}{Util.Format(analyzeNode.HostName, StrSty.HOSTNAME)}
{Util.Format("IP address:", StrSty.DECOR),padLength}{Util.Format(analyzeNode.IP, StrSty.IP)}
{Util.Format("Display name:", StrSty.DECOR),padLength}{Util.Format(analyzeNode.DisplayName, StrSty.DISPLAY_NAME)}
{Util.Format("▶ Classification", StrSty.HEADER)}
{Util.Format("Firewall rating:", StrSty.DECOR),padLength}{Util.Format($"{analyzeNode.DefLvl}", StrSty.DEF_LVL)}
{Util.Format("Security Level:", StrSty.DECOR),padLength}{Util.Format($"{analyzeNode.SecType}", StrSty.SEC_TYPE)}
");
		ANALYZErunCMD?.Invoke(CError.OK, positionalArgs[0]);
	}
}
