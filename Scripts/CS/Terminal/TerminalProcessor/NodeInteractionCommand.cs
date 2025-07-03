using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class ShellCore {
	static void MiFarm(Dictionary<string, string> farg, string[] parg) {
		bool needHelp = Util.ContainKeys(farg, "-h", "--help");
		bool listAll = Util.ContainKeys(farg, "-a", "--all");
        bool analyzeOne = Util.ContainKeys(farg, "-m", "--miner"); string minerName = Util.GetArg(farg, "-m", "--miner");

		if (listAll) {
            if (NetworkManager.BotNet.Count == 0) { Say(Util.Format("No miners in the botnet.", StrSty.WARNING)); return; }
            Say("All bots:");
			StringBuilder sb = new("");
			foreach (BotFarm bot in NetworkManager.BotNet) {
				sb.Append($"[color={Util.ColorMapperSecLvl(bot.DefLvl)}]{bot.HostName}[/color]".PadRight(44));
			}
			Say(sb.ToString());
            return;
        }

		if (string.IsNullOrEmpty(minerName)) { Say("-r", "No miner name provided."); return; }
		BotFarm hackfarm = NetworkManager.GetBotByName(minerName);
		if (hackfarm == null) { Say("-r", "No miner with such name."); return; }
		Say(
$@"{Util.Format("Botnet:", StrSty.DECOR)} {Util.Format(minerName, StrSty.HOSTNAME)}
{Util.GenerateStringByProportions([.. hackfarm.mineralDistribution.Select(x => x.Item2)], [.. hackfarm.mineralDistribution.Select(x => ItemCrafter.MINERALS[x.Item1].ColorCode)], 50)}
{string.Join(" | ", hackfarm.mineralDistribution.Select(x => $"[color={Util.CC(ItemCrafter.MINERALS[x.Item1].ColorCode)}]{ItemCrafter.MINERALS[x.Item1].Shorthand}[/color]: {Util.Format($"{x.Item2 * 100.0:0.00}", StrSty.NUMBER)}%"))}
================================================================
{Util.Format("Batch size", StrSty.DECOR)}    | Lvl.{Util.Format($"{hackfarm.BatchSizeLVL}", StrSty.NUMBER, "0"), 26} | {Util.Format($"{hackfarm.BatchSize}", StrSty.NUMBER):3}
{Util.Format("Mining speed", StrSty.DECOR)}  | Lvl.{Util.Format($"{hackfarm.MineSpeedLVL}", StrSty.NUMBER, "0"), 26} | {Util.Format($"{hackfarm.MineSpeed}", StrSty.NUMBER):3}
{Util.Format("Transfer time", StrSty.DECOR)} | Lvl.{Util.Format($"{hackfarm.XferDelayLVL}", StrSty.NUMBER, "0"), 26} | {Util.Format($"{hackfarm.XferDelay}", StrSty.NUMBER):3}
================================================================
{Util.Format("Aprox. Time To Live:", StrSty.DECOR)} {Util.TimeDifferenceFriendly(hackfarm.LifeTime)}");
	}
	const double FLARE_TIME = 120.0;
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
	static void Karaxe(Dictionary<string, string> farg, string[] parg) {
		bool help = Util.ContainKeys(farg, "--help"); if (help) {
            Say(
$@"Welcome to {Util.Format("Karaxe", StrSty.CMD_ACT)}
This program allows you to attack nodes and bypass their security systems.

{Util.Format("Usage:", StrSty.CMD_FUL)}
	{Util.Format("karaxe --flr", StrSty.CMD_FUL)}
		Activate Karaxe. Exposes all node locks for a limited time.

	{Util.Format("karaxe --atk <flag_key_pairs>", StrSty.CMD_FUL)}
		Attack a node. Bypasses the node’s security system using the specified flag-key pairs.

		Example:
			{Util.Format("karaxe --atk --i4 1 --2xtractor fl1p", StrSty.CMD_FUL)}
			Attempts to unlock {Util.Format("I4X", StrSty.NODE_LOCK)} lock with
				{Util.Format("--i4", StrSty.CMD_FLAG)}={Util.Format("1", StrSty.CMD_ARG)},
				{Util.Format("--2xtractor", StrSty.CMD_FLAG)}={Util.Format("fl1p", StrSty.CMD_ARG)}

	{Util.Format("karaxe --axe", StrSty.CMD_FUL)}
		Deactivate Karaxe. Closes all node locks and stops the flare.

	{Util.Format("karaxe --help", StrSty.CMD_FUL)}
		Show this help message.

{Util.Format("NOTES:", StrSty.HEADER)}
	- {Util.Format("--flr", StrSty.CMD_FLAG)}, {Util.Format("--atk", StrSty.CMD_FLAG)}, and {Util.Format("--axe", StrSty.CMD_FLAG)}
		are mutually exclusive. You can only use one at a time.
	- To attack, you must run {Util.Format("--flr", StrSty.CMD_FLAG)} first to expose the node locks.
	- After a successful attack, run {Util.Format("analyze", StrSty.CMD_FUL)} to retrieve new information.
	- When the Karaxe duration ends, normal sectors will ban you (the sector will disappear).
	  Exceptions: tutorial, mission, or training sectors will remain accessible.

{Util.Format("Examples:", StrSty.HEADER)}
	{Util.Format("karaxe --flr", StrSty.CMD_FUL)}
	{Util.Format("karaxe --atk --i4 1 --2xtractor fl1p", StrSty.CMD_FUL)}
	{Util.Format("karaxe --axe", StrSty.CMD_FUL)}
"); return;
        }

        bool startKaraxe = Util.ContainKeys(farg, "--flr"), doKaraxe = Util.ContainKeys(farg, "--atk"), endKaraxe = Util.ContainKeys(farg, "--axe");
		if (!(startKaraxe || doKaraxe || endKaraxe)) {
			ShellCore.Say($"No action taken. Use {Util.Format("karaxe --help", StrSty.CMD_FUL)} for usage");
			return;
		}
		
		if (startKaraxe && endKaraxe) {
			Say("-r", $"You can not use {Util.Format("--axe", StrSty.CMD_FLAG)} and {Util.Format("--flare",StrSty.CMD_FLAG)} at the same time.");
			KARrunCMD?.Invoke([(CError.UNKNOWN, "", "", "--axe and --flare incompatible")]);
			return;
		}
        if (startKaraxe) {
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
		if (endKaraxe) {
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
		if (!doKaraxe) {
			Say("-r", $"Missing {Util.Format("--attack", StrSty.CMD_FLAG)} flag. Use {Util.Format("--attack", StrSty.CMD_FLAG)} to attack a node.");
            KARrunCMD?.Invoke([(CError.UNKNOWN, "", "", "Missing --attack")]);
            return;
		}
		
		(CError, string, string, string)[] result = Attack(farg);
		KARrunCMD?.Invoke(result);
		Dictionary<string, string> inputs = [];
		for (int i = 0; i < result.Length; ++i) {
			(CError, string, string, string) res = result[i];
            if (res.Item1 == CError.OK) {
                if (i != result.Length - 1) {
                    ShellCore.Say($"[{Util.Format("SUCCESS", StrSty.PART_SUCCESS)}] {Util.Format("Bypassed", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.NODE_LOCK)}");
                    continue;
                }
                ShellCore.Say($"{Util.Format("Node defense cracked.", StrSty.FULL_SUCCESS)} All security system [color={Util.CC(Cc.gR)}]destroyed[/color].");
                ShellCore.Say($"Run {Util.Format("analyze", StrSty.CMD_FUL)} for all new information.");
                continue;
            }
            if (res.Item1 == CError.MISSING) {
				ShellCore.Say($"[{Util.Format("N0VALUE", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.NODE_LOCK)}");
				ShellCore.Say("-r", $"Missing key for {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
				inputs[res.Item2] = res.Item4;
                continue;
			} 
			if (res.Item1 == CError.INCORRECT) {
				ShellCore.Say($"[{Util.Format("WRON6KY", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.NODE_LOCK)}");
				ShellCore.Say("-r", $"Incorrect key for {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
				inputs[res.Item2] = res.Item4;
                continue;
			} 
			if (res.Item1 == CError.MISSING) {
				ShellCore.Say($"[{Util.Format("MI55ING", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.NODE_LOCK)}");
				ShellCore.Say("-r", $"Missing flag {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
				continue;
			} 
			if (res.Item1 == CError.NO_PERMISSION) {
				ShellCore.Say($"[{Util.Format("N0PERMS", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.NODE_LOCK)}");
				ShellCore.Say("-r", $"You do not have permission to use {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
				continue;
			}
			ShellCore.Say($"[{Util.Format("UNK??WN", StrSty.ERROR)}] {Util.Format("Denied access by", StrSty.DECOR)} {Util.Format(res.Item2, StrSty.NODE_LOCK)}");
			ShellCore.Say("-r", $"Unknown error for {Util.Format(res.Item3, StrSty.CMD_FLAG)}");
		}
		foreach (string k in inputs.Keys) {
			ShellCore.Say($"[color={Util.CC(Cc.W)}]{inputs[k]}[/color]");
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
{Util.Format("Security Level:", StrSty.DECOR),padLength}{Util.Format($"{analyzeNode.SecLvl}", StrSty.SEC_TYPE)}
");
		ANALYZErunCMD?.Invoke(CError.OK, positionalArgs[0]);
	}
}
