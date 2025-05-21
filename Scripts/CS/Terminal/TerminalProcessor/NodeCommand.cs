using Godot;
using System.Collections.Generic;
using System.Linq;
public static partial class TerminalProcessor {
    static void Home(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        CurrNode = NetworkManager.playerNode;
    }
    static void Scan(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        string L = " └─── ", M = " ├─── ", T = "      ", E = " │    ";
        System.Func<bool[], string> getTreePrefix = arr => string.Concat(arr.Select((b, i) => (i == arr.Length - 1 ? (b ? L : M) : (b ? T : E))));
        System.Func<bool[], string> getDescPrefix = arr => string.Concat(arr.Select((b, i) => (b ? T : E)));
        if (!parsedArgs.TryGetValue("-d", out string value)) { value = "1"; parsedArgs["-d"] = value; }
        if (!int.TryParse(value, out int MAX_DEPTH)) { Say("-r", $"Invalid depth: {Util.Format(value, StrType.NUMBER)}"); return; }

        Stack<(NetworkNode, int, bool[])> stack = new([(_currNode, 0, [])]);
        List<NetworkNode> visited = [];
        string output = "";
        while (stack.Count > 0) {
            (NetworkNode node, int depth, bool[] depthMarks) = stack.Pop();
            if (depth > MAX_DEPTH || visited.Contains(node)) continue;
            NodeAnalysis analyzeResult = node.Analyze();
            output += $"{Util.Format(getTreePrefix(depthMarks), StrType.DECOR)}{Util.Format(analyzeResult.IP.PadRight(15), StrType.IP)} {Util.Format(analyzeResult.HostName, StrType.HOSTNAME)}\n";

            if (parsedArgs.ContainsKey("-v")) {
                string descPrefix = getDescPrefix(depthMarks) + ((depth == MAX_DEPTH ? 0 : ((node.ParentNode != null ? 0 : -1) + node.ChildNode.Count)) > 0 ? " │" : "  ");
                output += $"{Util.Format(descPrefix, StrType.DECOR)}   Display Name:    {Util.Format(analyzeResult.DisplayName, StrType.DISPLAY_NAME)}\n";
                output += $"{Util.Format(descPrefix, StrType.DECOR)}   Node Type:       {Util.Format($"{analyzeResult.NodeType}", StrType.SYMBOL)}\n";
                output += $"{Util.Format(descPrefix, StrType.DECOR)}   Firewall rating: {Util.Format($"{analyzeResult.DefLvl}", StrType.DEF_LVL)}  Security: {Util.Format($"{analyzeResult.SecType}", StrType.SEC_TYPE)}\n";
                if (!string.IsNullOrWhiteSpace(descPrefix))
                    output += $"{Util.Format(descPrefix, StrType.DECOR)}\n";
            }
            visited.Add(node);

            // Use k==1 due to FILO algorithm.
            List<NetworkNode> nextOfQueue = [];
            if (node.ParentNode != null && !visited.Contains(node.ParentNode) && depth <= MAX_DEPTH) nextOfQueue.Add(node.ParentNode);
            foreach (NetworkNode child in node.ChildNode) {
                if (visited.Contains(child) || depth > MAX_DEPTH) continue;
                nextOfQueue.Add(child);
            }

            int k = 0; foreach (NetworkNode child in nextOfQueue) {
                ++k; stack.Push((child, depth + 1, [.. depthMarks, k == 1]));
            }
        }
        Say("-n", output);
    }
    static void Farm(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        foreach (KeyValuePair<string, string> pair in parsedArgs) {
            GD.Print(pair.Key, ' ', parsedArgs[pair.Key]);
        }
        bool showStatus = parsedArgs.ContainsKey("-s") || parsedArgs.ContainsKey("--status");

        bool doUpgrade = parsedArgs.ContainsKey("-u") || parsedArgs.ContainsKey("--upgrade");
        bool hasLevel = parsedArgs.ContainsKey("-l") || parsedArgs.ContainsKey("--level");

        // Disallow mixing upgrade with status/info
        if (showStatus && doUpgrade) { Say("-r", "Can not show status of a node and changing it at the same time."); return; }

        // Check node's ownership
        if (CurrNode.CurrentOwner != NetworkManager.playerNode) { Say("-r", "You don't own this node's GC miner"); return; }
        // Status display
        if (showStatus) {
            int hackLvl = CurrNode.HackFarm.HackLvl, timeLvl = CurrNode.HackFarm.TimeLvl, growLvl = CurrNode.HackFarm.TimeLvl;
            Say(@$"[ GC Farm Status for {CurrNode.DisplayName} ]
Current GC in node: {Util.Format($"{CurrNode.HackFarm.CurrencyPile}", StrType.MONEY)}
Hack  : Lv.{Util.Format($"{hackLvl}", StrType.NUMBER)} -> {Util.Format(Util.Format($"{CurrNode.HackFarm.CurHack}", StrType.MONEY), StrType.UNIT, "/transfer")}
Time  : Lv.{Util.Format($"{timeLvl}", StrType.NUMBER)} -> {Util.Format($"{CurrNode.HackFarm.CurTime}", StrType.UNIT, "s/tranfer")}
Grow  : Lv.{Util.Format($"{growLvl}", StrType.NUMBER)} -> {Util.Format(Util.Format($"{CurrNode.HackFarm.CurGrow}", StrType.MONEY), StrType.UNIT, "/s")}");

            Say(@$"[ Upgrade Info ]
Hack +{Util.Format($"1", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(hackLvl + 1, 1).Sum(i => CurrNode.HackFarm.GetHackCost(i))}", StrType.MONEY)} | +{Util.Format($"10", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(hackLvl + 1, 10).Sum(i => CurrNode.HackFarm.GetHackCost(i))}", StrType.MONEY)}
Time +{Util.Format($"1", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(timeLvl + 1, 1).Sum(i => CurrNode.HackFarm.GetTimeCost(i))}", StrType.MONEY)} | +{Util.Format($"10", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(timeLvl + 1, 10).Sum(i => CurrNode.HackFarm.GetTimeCost(i))}", StrType.MONEY)}
Grow +{Util.Format($"1", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(growLvl + 1, 1).Sum(i => CurrNode.HackFarm.GetGrowCost(i))}", StrType.MONEY)} | +{Util.Format($"10", StrType.NUMBER)} -> {Util.Format($"{Enumerable.Range(growLvl + 1, 10).Sum(i => CurrNode.HackFarm.GetGrowCost(i))}", StrType.MONEY)}");
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
                            int result = CurrNode.HackFarm.UpgradeHackLevel();
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
                            int result = CurrNode.HackFarm.UpgradeTimeLevel();
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
                            int result = CurrNode.HackFarm.UpgradeTimeLevel();
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
    static double startEpoch = 0, endEpoch = 0, remainingTime = 0;
    const double FLARE_TIME = 120.0;
    static int Crack(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        NetworkNode node = CurrNode;
        if (parsedArgs.ContainsKey("--axe") && parsedArgs.ContainsKey("--flare")) {
            Say("-r", "You can not use --axe and --flare at the same time.");
            return 4;
        }
        if (parsedArgs.ContainsKey("--axe")) {
            if (endEpoch < Time.GetUnixTimeFromSystem()) {
                Say("-r", "Karaxe already deactivated.");
                return 6;
            }
            endEpoch = Time.GetUnixTimeFromSystem();
            Say($"{Util.Format("Kraken tendrils axed", StrType.FULL_SUCCESS)}. Flare sequence exited. All [color={Util.CC(Cc.Y)}]lok[/color] closed.");
            return 5;
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
        if (Time.GetUnixTimeFromSystem() > endEpoch) {
            Say($"{Util.Format("Kraken inactive", StrType.ERROR)}. Run {Util.Format("karaxe --flare", StrType.CMD_FUL)} to activate.");
            return 3;
        }

        int result = node.AttempCrackNode(parsedArgs, endEpoch);
        if (result == 0) {
            node.TransferOwnership(NetworkManager.playerNode);
            NetworkManager.AddHackFarm(node.HackFarm);
        }
        return 0;
    }
    static void Connect(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No hostname provided."); return; }
        if (NetworkManager.QueryDNS(positionalArgs[0]) != null) { CurrNode = NetworkManager.QueryDNS(positionalArgs[0]); return; }
        if (CurrNode.ParentNode != null && CurrNode.ParentNode.HostName == positionalArgs[0] ||
            (CurrNode.ParentNode is HoneypotNode && (CurrNode.ParentNode as HoneypotNode).fakeHostName == positionalArgs[0])) { CurrNode = CurrNode.ParentNode; return; }

        NetworkNode node = CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]);
        NetworkNode fnode = CurrNode.ChildNode.FindLast(s => s is HoneypotNode && (s as HoneypotNode).fakeHostName == positionalArgs[0]);
        if (node == null && fnode == null) { Say("-r", $"Host not found: {Util.Format(positionalArgs[0], StrType.HOSTNAME)}"); return; }
        CurrNode = (node ?? fnode);
    }
    static void Analyze(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { positionalArgs = [CurrNode.HostName]; }
        if (CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]) == null
            && (CurrNode.ParentNode != null && CurrNode.ParentNode.HostName != positionalArgs[0])
            && CurrNode.HostName != positionalArgs[0]) {
            Say("-r", $"Host not found: {positionalArgs[0]}");
            return;
        }
        NetworkNode analyzeNode = null;
        if (CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]) != null) analyzeNode = CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]);
        else if (CurrNode.ParentNode != null && CurrNode.ParentNode.HostName == positionalArgs[0]) analyzeNode = CurrNode.ParentNode;
        else if (CurrNode.HostName == positionalArgs[0]) analyzeNode = CurrNode;
        const int globalWidth = 17;
        Say("-tl", $@"
{Util.Format("▶ Node Info", StrType.HEADER)}
{Util.Format("Host name:", StrType.DECOR),globalWidth}{Util.Format(analyzeNode.HostName, StrType.HOSTNAME)}
{Util.Format("IP address:", StrType.DECOR),globalWidth}{Util.Format(analyzeNode.IP, StrType.IP)}
{Util.Format("Display name:", StrType.DECOR),globalWidth}{Util.Format(analyzeNode.DisplayName, StrType.DISPLAY_NAME)}
{Util.Format("▶ Classification", StrType.HEADER)}
{Util.Format("Node type:", StrType.DECOR),globalWidth}{Util.Format($"{analyzeNode.NodeType}", StrType.SYMBOL)}
{Util.Format("Firewall rating:", StrType.DECOR),globalWidth}{Util.Format($"{analyzeNode.DefLvl}", StrType.DEF_LVL)}
{Util.Format("Security level:", StrType.DECOR),globalWidth}{Util.Format($"{analyzeNode.SecType}", StrType.SEC_TYPE)}
");

        // Honeypot node don't dare to impersonate actual organization or corporation.
        if (analyzeNode.CurrentOwner != NetworkManager.playerNode || analyzeNode.GetType() == typeof(HoneypotNode)) {
            Say($"Crack this node security system to get further access.");
            return;
        }
        Say("-tl", $@"
{Util.Format("▶ GC miner detail", StrType.HEADER)}
{Util.Format("Transfer batch:", StrType.DECOR),globalWidth}{Util.Format($"{analyzeNode.HackFarm.HackLvl}", StrType.NUMBER)} ({Util.Format($"{analyzeNode.HackFarm.CurHack}", StrType.MONEY)})
{Util.Format("Transfer speed:", StrType.DECOR),globalWidth}{Util.Format($"{analyzeNode.HackFarm.TimeLvl}", StrType.NUMBER)} ({Util.Format($"{analyzeNode.HackFarm.CurTime}", StrType.UNIT, "s")})
{Util.Format("Mining speed:", StrType.DECOR),globalWidth}{Util.Format($"{analyzeNode.HackFarm.GrowLvl}", StrType.NUMBER)} ({Util.Format(Util.Format($"{analyzeNode.HackFarm.CurGrow}", StrType.MONEY), StrType.UNIT, "/s")})
");
        if (analyzeNode is FactionNode) {
            Say("-tl", $@"
{Util.Format("▶ Faction detail", StrType.HEADER)}
{Util.Format("Name:", StrType.DECOR),globalWidth}{Util.Format(Util.Obfuscate((analyzeNode as FactionNode).Faction.Name), StrType.DISPLAY_NAME)}
{Util.Format("Description:", StrType.DECOR),globalWidth}{Util.Format(Util.Obfuscate((analyzeNode as FactionNode).Faction.Desc), StrType.DESC)}
");
        }
        if (analyzeNode is BusinessNode) {
            Say("-tl", $@"
{Util.Format("▶ Business detail", StrType.HEADER)}
{Util.Format("Stock:", StrType.DECOR),globalWidth}{Util.Format((analyzeNode as BusinessNode).Stock.Name, StrType.DISPLAY_NAME)}
{Util.Format("Value:", StrType.DECOR),globalWidth}{Util.Format((analyzeNode as BusinessNode).Stock.Price.ToString("F2"), StrType.MONEY)}
");
        }
        if (analyzeNode is CorpNode) {
            Say("-tl", $@"
{Util.Format("▶ Faction detail", StrType.HEADER)}
{Util.Format("Name:", StrType.DECOR),globalWidth}{Util.Format(Util.Obfuscate((analyzeNode as CorpNode).Faction.Name), StrType.DISPLAY_NAME)}
{Util.Format("Description:", StrType.DECOR),globalWidth}{Util.Format(Util.Obfuscate((analyzeNode as CorpNode).Faction.Desc), StrType.DESC)}
{Util.Format("▶ Business detail", StrType.HEADER)}
{Util.Format("Stock:", StrType.DECOR),globalWidth}{Util.Format((analyzeNode as CorpNode).Stock.Name, StrType.DISPLAY_NAME)}
{Util.Format("Value:", StrType.DECOR),globalWidth}{Util.Format((analyzeNode as CorpNode).Stock.Price.ToString("F2"), StrType.MONEY)}
");
        }
    }
}