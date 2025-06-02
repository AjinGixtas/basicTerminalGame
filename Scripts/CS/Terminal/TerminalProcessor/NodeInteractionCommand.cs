using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class TerminalProcessor {
    static void Farm(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        bool listAll = parsedArgs.ContainsKey("-a") || parsedArgs.ContainsKey("--all");
        string[] botnetNames = NetworkManager.GetBotnetNames();
        if (listAll) {
            for (int i = 0; i < botnetNames.Length; i++) Say(botnetNames[i]);
            return;
        }

        string minerName = "";
        parsedArgs.TryGetValue("-h", out minerName); parsedArgs.TryGetValue("--host", out minerName);
        if (string.IsNullOrEmpty(minerName)) { Say("-r", "No miner name provided."); return; }
        HackFarm hackfarm = NetworkManager.GetHackfarm(minerName);
        if (hackfarm == null) { Say("-r", "No miner with such name."); return; }
        GD.Print(Util.MINERAL_PROFILES.Length);
        GD.Print(hackfarm.mineralDistribution.Select(x=>x.Item1));
        Say(
$@"Botnet: {Util.Format(minerName, StrType.HOSTNAME)}
{GenerateStringByProportions([.. hackfarm.mineralDistribution.Select(x => x.Item2)], [.. hackfarm.mineralDistribution.Select(x => Util.MINERAL_PROFILES[x.Item1].ColorCode)], 50)}
================================================================
Load capacity  | Lvl.{Util.Format($"{hackfarm.HackLVL}", StrType.NUMBER):3} | {Util.Format($"{hackfarm.HackVal}", StrType.NUMBER):3}
Mining speed   | Lvl.{Util.Format($"{hackfarm.GrowLVL}", StrType.NUMBER):3} | {Util.Format($"{hackfarm.GrowVal}", StrType.NUMBER):3}
Transfer speed | Lvl.{Util.Format($"{hackfarm.TimeLVL}", StrType.NUMBER):3} | {Util.Format($"{hackfarm.TimeVal}", StrType.NUMBER):3}
================================================================
Aprox lifetime: {TimeDifferenceFriendly(hackfarm.LifeTime)}");

        static string GenerateStringByProportions(double[] proportions, Cc[] colorCode, int length) {
            if (proportions.Length != colorCode.Length)
                throw new ArgumentException("Proportions and characters must be of the same length.");

            int[] counts = new int[proportions.Length];
            int assigned = 0;

            // First pass: calculate initial counts using floor
            for (int i = 0; i < proportions.Length; i++) {
                counts[i] = (int)Math.Floor(proportions[i] * length);
                assigned += counts[i];
            }

            // Distribute the remainder
            int remainder = length - assigned;

            // Get decimal parts and their indices
            double[] decimalParts = new double[proportions.Length];
            for (int i = 0; i < proportions.Length; i++) {
                decimalParts[i] = (proportions[i] * length) - counts[i];
            }

            // Assign remaining characters based on largest decimal parts
            while (remainder > 0) {
                int maxIndex = 0;
                for (int i = 1; i < decimalParts.Length; i++) {
                    if (decimalParts[i] > decimalParts[maxIndex]) {
                        maxIndex = i;
                    }
                }

                counts[maxIndex]++;
                decimalParts[maxIndex] = 0; // Mark as used
                remainder--;
            }

            // Build final string
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < counts.Length; i++) {
                sb.Append($"[color={Util.CC(colorCode[i])}]" + new string('█', counts[i]) + "[/color]");
            }

            return sb.ToString();
        }
        static string TimeDifferenceFriendly(double time) {
            int hours = (int)(time / 3600);
            if (hours < 1) return $"Less than {Util.Format("1", StrType.UNIT, "hr")}";
            return $"Aprox.{Util.Format($"{hours}", StrType.UNIT, "hr")} remaining";
        }
    }
    const double FLARE_TIME = 120.0;
    static double startEpoch = 0, endEpoch = 0, remainingTime = 0;
    static List<WeakReference<DriftSector>> sectorAttacked = [];
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
            // No idea why, but hard reference to DriftSector is not working here.
            sectorAttacked.Add(new WeakReference<DriftSector>((targetNode as DriftNode).Sector));
        }
        if (result == 0 && !targetNode.OwnedByPlayer) {
            targetNode.TransferOwnership();
            if (targetNode.GetType() == typeof(DriftNode)) NetworkManager.QueueAddHackFarm((targetNode as DriftNode).HackFarm);
        }
        return 0;
    }
}