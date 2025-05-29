using Godot;
using System.Collections.Generic;
using System.Linq;

public static partial class TerminalProcessor {
    static void Farm(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (CurrNode is not ScriptedNetworkNode) { 
            Say("-r", "This node does not have a GC miner installed.");
            return; 
        }
        bool showStatus = parsedArgs.ContainsKey("-s") || parsedArgs.ContainsKey("--status");

        bool doUpgrade = parsedArgs.ContainsKey("-u") || parsedArgs.ContainsKey("--upgrade");
        bool hasLevel = parsedArgs.ContainsKey("-l") || parsedArgs.ContainsKey("--level");

        // Disallow mixing upgrade with status/info
        if (showStatus && doUpgrade) { Say("-r", "Can not show status of a node and changing it at the same time."); return; }

        // Check node's ownership
        if (!CurrNode.OwnedByPlayer) { Say("-r", "You don't own this node's GC miner"); return; }
        // Status display
        ScriptedNetworkNode sNode = CurrNode as ScriptedNetworkNode;
        if (showStatus) {
            int hackLvl = sNode.HackFarm.HackLvl, timeLvl = sNode.HackFarm.TimeLvl, growLvl = sNode.HackFarm.TimeLvl;
            Say(@$"[ GC Farm Status for {sNode.DisplayName} ]
Current GC in node: {Util.Format($"{sNode.HackFarm.CurrencyPile}", StrType.MONEY)}
Hack  : Lv.{Util.Format($"{hackLvl}", StrType.NUMBER)} -> {Util.Format(Util.Format($"{sNode.HackFarm.CurHack}", StrType.MONEY), StrType.UNIT, "/transfer")}
Time  : Lv.{Util.Format($"{timeLvl}", StrType.NUMBER)} -> {Util.Format($"{            sNode.HackFarm.CurTime}", StrType.UNIT, "s/tranfer")}
Grow  : Lv.{Util.Format($"{growLvl}", StrType.NUMBER)} -> {Util.Format(Util.Format($"{sNode.HackFarm.CurGrow}", StrType.MONEY), StrType.UNIT, "/s")}");

            Say(@$"[ Upgrade Info ]
Hack +{Util.Format($"1", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(hackLvl + 1, 1).Sum(i => HackFarm.GetHackCost(i))}", StrType.MONEY)} | +{Util.Format($"10", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(hackLvl + 1, 10).Sum(i => HackFarm.GetHackCost(i))}", StrType.MONEY)}
Time +{Util.Format($"1", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(timeLvl + 1, 1).Sum(i => HackFarm.GetTimeCost(i))}", StrType.MONEY)} | +{Util.Format($"10", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(timeLvl + 1, 10).Sum(i => HackFarm.GetTimeCost(i))}", StrType.MONEY)}
Grow +{Util.Format($"1", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(growLvl + 1, 1).Sum(i => HackFarm.GetGrowCost(i))}", StrType.MONEY)} | +{Util.Format($"10", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(growLvl + 1, 10).Sum(i => HackFarm.GetGrowCost(i))}", StrType.MONEY)}");
            return;
        }
        // Upgrade path
        if (doUpgrade) {
            string upgradeType = parsedArgs.GetValueOrDefault("-u") ?? parsedArgs.GetValueOrDefault("--upgrade") ?? "";
            int level = 1;
            if (!new[] { "hack", "time", "grow" }.Contains(upgradeType)) {
                Say("-r", $"Invalid upgrade type. Use: {Util.Format("hack", StrType.CMD_ARG)}, {Util.Format("time", StrType.CMD_ARG)}, or {Util.Format("grow", StrType.CMD_ARG)}");
                return;
            }
            if (!hasLevel) { Say("-r", $"Missing {Util.Format("--level", StrType.CMD_FLAG)}"); return; } else {
                if (!int.TryParse(parsedArgs.GetValueOrDefault("-l") ??
                    parsedArgs.GetValueOrDefault("--level"), out level) || level <= 0) { Say("-r", "Invalid level amount."); return; }
            }

            switch (upgradeType.ToLower()) {
                case "hack": {
                        for (int i = 0; i < level; i++) {
                            int result = sNode.HackFarm.UpgradeHackLevel();
                            switch (result) {
                                case 0: break;
                                case 1: Say("-r", $"Upgrade cost invalid. Please report this bug!!!"); return;
                                case 2: Say("-r", $"Not enough money."); return;
                                case 3: Say($"{Util.Format("Max level reached.", StrType.FULL_SUCCESS)}"); return;
                                default: Say("-r", $"Unknown error encountered. Error code: {result}"); return;
                            }
                        }
                        break;
                    }
                case "time": {
                        for (int i = 0; i < level; i++) {
                            int result = sNode.HackFarm.UpgradeTimeLevel();
                            switch (result) {
                                case 0: break;
                                case 1: Say("-r", $"Upgrade cost invalid. Please report this bug!!!"); return;
                                case 2: Say("-r", $"Not enough money."); return;
                                case 3: Say($"{Util.Format("Max level reached.", StrType.FULL_SUCCESS)}"); return;
                                default: Say("-r", $"Unknown error encountered. Error code: {result}"); return;
                            }
                        }
                        break;
                    }
                case "grow": {
                        for (int i = 0; i < level; i++) {
                            int result = sNode.HackFarm.UpgradeTimeLevel();
                            switch (result) {
                                case 0: break;
                                case 1: Say("-r", $"Upgrade cost invalid. Please report this bug!!!"); return;
                                case 2: Say("-r", $"Not enough money."); return;
                                case 3: Say($"{Util.Format("Max level reached.", StrType.FULL_SUCCESS)}"); return;
                                default: Say("-r", $"Unknown error encountered. Error code: {result}"); return;
                            }
                        }
                        break;
                    }
                default:
                    Say($"Unknown upgrade type. Use: {Util.Format("hack", StrType.CMD_ARG)}, {Util.Format("time", StrType.CMD_ARG)}, or {Util.Format("hack", StrType.CMD_ARG)}");
                    return;
            }
            Say("Upgrade complete.");
        } else {
            Say("-r", $"No action specified. Use either {Util.Format("--status", StrType.CMD_FLAG)} or {Util.Format("--upgrade", StrType.CMD_FLAG)}."); return;
        }
    }
    const double FLARE_TIME = 120.0;
    static double startEpoch = 0, endEpoch = 0, remainingTime = 0;
    static List<DriftSector> sectorQueuedForRemoval = [];
    static int Karaxe(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        NetworkNode targetNode = CurrNode;
        if (parsedArgs.ContainsKey("--axe") && parsedArgs.ContainsKey("--flare")) {
            Say("-r", "You can not use --axe and --flare at the same time.");
            return 4;
        }
        
        if (parsedArgs.ContainsKey("--flare")) {
            if (Time.GetUnixTimeFromSystem() < endEpoch) {
                Say("Karaxe already in effect.");
                return 1;
            }
            startEpoch = Time.GetUnixTimeFromSystem(); remainingTime = FLARE_TIME; endEpoch = startEpoch + remainingTime;
            crackDurationTimer.Start(FLARE_TIME);
            Say($"{Util.Format("Kraken activated", StrType.FULL_SUCCESS)}. All node [color={Util.CC(Cc.Y)}]lok[/color] system {Util.Format("~=EXP0SED=~", StrType.ERROR)}");
            return 2;
        }

        if (parsedArgs.ContainsKey("--axe")) {
            if (Time.GetUnixTimeFromSystem() > endEpoch) {
                Say("-r", "Karaxe already deactivated.");
                return 6;
            }
            Say($"{Util.Format("Kraken deactivated", StrType.FULL_SUCCESS)}. Flare sequence exited. All [color={Util.CC(Cc.Y)}]lock[/color] closed.");
            EndFlare();
            return 5;
        }
        if (Time.GetUnixTimeFromSystem() > endEpoch) {
            Say($"{Util.Format("Kraken inactive", StrType.ERROR)}. Run {Util.Format("karaxe --flare", StrType.CMD_FUL)} to activate.");
            EndFlare();
            return 3;
        }
        if (!parsedArgs.ContainsKey("--attack")) {
            Say("-r", $"Missing {Util.Format("--attack", StrType.CMD_FLAG)} flag. Use {Util.Format("--attack", StrType.CMD_FLAG)} to attack a node.");
            return 7;
        }
        int result = targetNode.AttempCrackNode(parsedArgs, endEpoch);
        if (targetNode.GetType() == typeof(DriftNode)) {
            sectorQueuedForRemoval.Add((targetNode as DriftNode).Sector);
        }
        if (result == 0 && !targetNode.OwnedByPlayer && targetNode.GetType() != typeof(DriftNode)) {
            targetNode.TransferOwnership();
            if (targetNode is ScriptedNetworkNode)
            NetworkManager.AddHackFarm((targetNode as ScriptedNetworkNode).HackFarm);
        }
        return 0;
        static void EndFlare() {
            endEpoch = 0;
            crackDurationTimer.Stop();
            if (sectorQueuedForRemoval.Count > 0) {
                foreach (DriftSector sector in sectorQueuedForRemoval) {
                    NetworkManager.DisconnectFromSector(sector);
                    NetworkManager.RemoveSector(sector);
                }
                sectorQueuedForRemoval.Clear();
            }
        }
    }
}