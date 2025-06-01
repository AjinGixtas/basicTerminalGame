using Godot;
using System.Collections.Generic;
using System.Linq;

public static partial class TerminalProcessor {
    static void Farm(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
    }
    const double FLARE_TIME = 120.0;
    static double startEpoch = 0, endEpoch = 0, remainingTime = 0;
    static List<DriftSector> sectorAttacked = [];
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
            sectorAttacked.Add((targetNode as DriftNode).Sector);
        }
        if (result == 0 && !targetNode.OwnedByPlayer && targetNode.GetType() != typeof(DriftNode)) {
            targetNode.TransferOwnership();
        }
        return 0;
        static void EndFlare() {
            endEpoch = 0;
            crackDurationTimer.Stop();
            if (sectorAttacked.Count > 0) {
                foreach (DriftSector sector in sectorAttacked) {
                    NetworkManager.DisconnectFromSector(sector);
                    sector.LockedDown = true;
                }
                sectorAttacked.Clear();
            }
        }
    }
}