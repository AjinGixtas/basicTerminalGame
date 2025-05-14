using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class TerminalProcessor {
	static Overseer overseer;
    static RichTextLabel terminalOutputField; static RichTextLabel terminalCommandPrompt; 
	static TextEdit terminalCommandField; 
	static Timer processTimer, updateProcessGraphicTimer;
    static NetworkManager networkManager;
    static NetworkNode _currNode = null; static NodeDirectory _currDir = null;
    static NodeDirectory CurrDir { get { return _currDir; } set { _currDir = value; SetCommandPrompt(); } }
    static NetworkNode CurrNode { get { return _currNode; } set { _currNode = value; _currDir = _currNode.NodeDirectory; SetCommandPrompt(); } }
    static string _userName;
    static string UserName { get { return _userName; } set { _userName = value; SetCommandPrompt(); } }
    static readonly List<string> commandHistory = []; static int _commandHistoryIndex = 0;
    static int CommandHistoryIndex {
        get => _commandHistoryIndex;
        set {
            value = Math.Clamp(value, 0, Math.Max(0, commandHistory.Count - 1));
            _commandHistoryIndex = value;
            if (_commandHistoryIndex < commandHistory.Count) {
                terminalCommandField.Text = commandHistory[_commandHistoryIndex];
                terminalCommandField.SetCaretColumn(commandHistory[_commandHistoryIndex].Length);
            }
        }
    }
    static bool initialized = false;
    public static void Intialize(Overseer overseer, RichTextLabel terminalOutputField, RichTextLabel terminalCommandPrompt, TextEdit terminalCommandField, Timer processTimer, Timer updateProcessGraphicTimer, NetworkManager networkManager) {
        if (initialized) return;

        // Assign provided parameters to static fields
        TerminalProcessor.overseer = overseer;
        TerminalProcessor.terminalOutputField = terminalOutputField;
        TerminalProcessor.terminalCommandPrompt = terminalCommandPrompt;
        TerminalProcessor.terminalCommandField = terminalCommandField;
        TerminalProcessor.processTimer = processTimer;
        TerminalProcessor.updateProcessGraphicTimer = updateProcessGraphicTimer;
        TerminalProcessor.networkManager = networkManager;

        // Initialize the current node and directory
        TerminalProcessor.CurrNode = networkManager.network;
        TerminalProcessor.CurrDir = _currNode.NodeDirectory;

        // Set the username and command prompt
        TerminalProcessor.UserName = networkManager.UserName;
        SetCommandPrompt();
        terminalCommandField.GrabFocus();

        // Mark as initialized
        initialized = true;
        terminalCommandField.ScrollFitContentHeight = true;
        terminalCommandField.ScrollFitContentWidth = true;
    }
    static double minHeight = 18; // Hard coded for convenience, prob should be generate at init runtime instead.
    public static void Process(double delta) {
        if (Input.IsActionJustPressed("moveDownHistory")) {
            if (minHeight >= terminalCommandField.Size.Y) CommandHistoryIndex += 1; 
        }
        if (Input.IsActionJustPressed("moveUpHistory")) { 
            if (minHeight >= terminalCommandField.Size.Y) CommandHistoryIndex -= 1; 
        }
        if (Input.IsActionJustPressed("submitCommand")) {
            if (terminalCommandField.Text.Length == 0) { terminalCommandField.GrabFocus(); return; }
            terminalCommandField.Text = terminalCommandField.Text.Trim('\r', '\n');
            SubmitCommand(terminalCommandField.Text);
            terminalCommandField.Text = "";
            terminalCommandField.GrabFocus();
        }
        TerminalProcessor.ShowMoreChars(delta);
    }

    static Action<Dictionary<string, string>, string[]> finishFunction; static (Dictionary<string, string>, string[]) finishFunctionArgs;
	static bool _isProcessing = false; static bool IsProcessing { get { return _isProcessing; } set { _isProcessing = value; } }
    static readonly string[] SpinnerChars = ["|", "/", "-", "\\"]; static int tick = 0, progress = 0;
    static void StartProcess(double time, Action<Dictionary<string, string>, string[]> func, Dictionary<string, string> parseArgs, string[] positionalArgs) {
        if (IsProcessing) return;
        finishFunction = func; finishFunctionArgs = (parseArgs, positionalArgs);
        IsProcessing = true;
        processTimer.WaitTime = time;
        terminalCommandField.Editable = false;
        progress = 0;
        //tick = GD.RandRange(0, 3);
        //int filledBar = Mathf.FloorToInt(progress / 100.0 * BarSize);
        Say("-n", $"Start");
        processTimer.Start();
        updateProcessGraphicTimer.Start();
    }
    public static void UpdateProcessingGraphic() {
        if (!IsProcessing) return;

        int t = GD.RandRange(0, 1 + (int)Math.Floor(200.0 / processTimer.WaitTime * updateProcessGraphicTimer.WaitTime));
        progress = Mathf.Clamp(progress + t, 0, 99); tick = (tick + 1) % SpinnerChars.Length;

        Say("-n", $"...{progress}%");
        updateProcessGraphicTimer.Start();
    }
    public static void ProcessFinished() {
        IsProcessing = false; terminalCommandField.Editable = true; terminalCommandField.GrabFocus();
        tick = (tick + 1) % 4;
        Say($"...Done!");
        progress = 0; tick = 0;
        finishFunction(finishFunctionArgs.Item1, finishFunctionArgs.Item2);
    }

    const int MAX_HISTORY_CMD_SIZE = 64;
    static void SubmitCommand(string newCommand) {
        if (commandHistory.Count == 0 || (commandHistory.Count > 0 && commandHistory[^1] != newCommand)) {
            commandHistory.Add(newCommand);
            while (commandHistory.Count > MAX_HISTORY_CMD_SIZE) { commandHistory.RemoveAt(0); }
            CommandHistoryIndex = commandHistory.Count;
        }
        string[] commands = newCommand.Split(';', StringSplitOptions.RemoveEmptyEntries);
        Say($"{terminalCommandPrompt.Text}{Util.Format(newCommand, StrType.CMD)}");
        for (int i = 0; i < commands.Length; i++) {
            string[] components = ParseArguments(commands[i]);
            if (components.Length == 0) continue;

            if (!ProcessCommand(components[0], components[1..])) break;
        }
        static string[] ParseArguments(string input) {
            var matches = Regex.Matches(input, @"(?:[\""].+?[\""]|\S+)(?=\s*=\s*)|\S+");
            var result = matches.Select(m => m.Value.Trim('"')).ToArray();
            return [.. result.Select(arg =>
            {
                if (!arg.Contains('\"')) {
                    var split = arg.Split(['='], 2);
                    return split.Length > 1 ? split : [split[0]];
                }
                return [arg];
            })
            .SelectMany(x => x)];
        }
    }
    static bool ProcessCommand(string command, string[] args) {
        command = command.ToLower();
        (Dictionary<string, string> parsedArgs, string[] positionalArgs) = ParseArgs(args);
        switch (command) {
            case "ls": LS(parsedArgs, positionalArgs); break;
            case "cd": CD(parsedArgs, positionalArgs); break;
            case "mkf": MkF(parsedArgs, positionalArgs); break;
            case "rmf": RmF(parsedArgs, positionalArgs); break;
            case "pwd": Pwd(parsedArgs, positionalArgs); break;
            case "say": Say(parsedArgs, positionalArgs); break;
            case "help": Help(parsedArgs, positionalArgs); break;
            case "home": Home(parsedArgs, positionalArgs); break;
            case "edit": Edit(parsedArgs, positionalArgs); break;
            case "scan": Scan(parsedArgs, positionalArgs); return false;
            case "farm": Farm(parsedArgs, positionalArgs); break;
            case "clear": Clear(parsedArgs, positionalArgs); break;
            case "mkdir": MkDir(parsedArgs, positionalArgs); break;
            case "rmdir": RmDir(parsedArgs, positionalArgs); break;
            case "karaxe": Crack(parsedArgs, positionalArgs); break;
            case "inspect": Inspect(parsedArgs, positionalArgs); break;
            case "connect": Connect(parsedArgs, positionalArgs); break;
            case "analyze": Analyze(parsedArgs, positionalArgs); return false;
            default: Say("-r", $"{command} is not a valid command."); break;
        }
        return true;
    }

    static void RmF(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No file name provided."); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            string[] components = positionalArgs[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string fileName = components[^1], parentPath = string.Join('/', components[..^1]);
            parentDirectory = CurrDir.GetDirectory(parentPath);
            if (parentDirectory == null) { Say("-r", $"File not found: {parentPath}"); return; }
            int result = parentDirectory.RemoveFile(positionalArgs[i]);
            switch (result) {
                case 0: break;
                case 1: Say($"{positionalArgs[i]} was not found."); break;
                default: Say("-r", $"{Util.Format(positionalArgs[i], StrType.FILE)} removal failed. Error code: {result}"); break;
            }
        }
    }
    static void LS(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        NodeDirectory targetDir = CurrDir;
        if (positionalArgs.Length != 0) {
            targetDir = CurrDir.GetDirectory(positionalArgs[0]);
            if (targetDir == null) { Say("-r", $"Directory not found: {Util.Format(positionalArgs[0], StrType.DIR)}"); return; }
        }

        if (targetDir.Childrens.Count == 0) { Say(""); return; }
        string output = "";
        foreach (NodeSystemItem item in targetDir.Childrens) {
            output += $"{Util.Format(item.Name, (item is NodeDirectory) ? StrType.DIR : StrType.FILE)} ";
        }
        Say(output);
    }
    static void CD(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", "No directory provided."); return; }
        NodeDirectory targetDir = CurrDir.GetDirectory(positionalArgs[0]);
        if (targetDir == null) { Say("-r", $"Directory not found: {Util.Format(positionalArgs[0], StrType.DIR)}"); return; }
        CurrDir = targetDir;
    }
    static void Pwd(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        Say($"{Util.Format(CurrDir.GetPath(), StrType.DIR)}");
    }
    // It gets 2 since this one is REALLY close to standard, but since it's also called independently a lot, allow for seperate shorthand flag is way better.
    static void Say(Dictionary<string, string> parseArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say(""); return; }
        string c = terminalOutputField.GetParsedText();
        if (c.Length >= MAX_HISTORY_CHAR_SIZE) {
            terminalOutputField.Clear();
            terminalOutputField.AppendText(c[^RESET_HISTORY_CHAR_SIZE..]);
        }
        string text = string.Join(" ", positionalArgs);
        if (parseArgs.ContainsKey("-e")) text = Regex.Unescape(text); // handles \n, \t, etc.
        if (!parseArgs.ContainsKey("-n")) text += "\n";
        if (parseArgs.ContainsKey("-r")) text = Util.Format(text, StrType.ERROR);
        if (parseArgs.ContainsKey("-t") && text.EndsWith('\n')) text = text[..^1]; // Trim only one trailing newline
        terminalOutputField.AppendText(text);
    }
    public static void Say(params string[] args) {
        if (args.Length == 0) { Say(""); return; }

        string c = terminalOutputField.GetParsedText();
        if (c.Length >= MAX_HISTORY_CHAR_SIZE) {
            terminalOutputField.Clear();
            terminalOutputField.AppendText(c[^RESET_HISTORY_CHAR_SIZE..]);
        }

        bool noNewline = false, interpretEscapes = false, makeRed = false;
        bool trimTrailingNewline = false, trimLeadingNewline = false;
        int argStart = 0;

        // Handle optional flags like -n, -e, -r, -t, -l
        if (args[0].StartsWith('-')) {
            string flags = args[0];
            noNewline = flags.Contains('n');
            interpretEscapes = flags.Contains('e');
            makeRed = flags.Contains('r');
            trimTrailingNewline = flags.Contains('t');
            trimLeadingNewline = flags.Contains('l');
            argStart = 1;
        }

        string text = string.Join(" ", args[argStart..]);
        if (interpretEscapes) text = Regex.Unescape(text); // handles \n, \t, etc.
        if (trimLeadingNewline) text = text.TrimStart('\n', '\r');
        if (trimTrailingNewline) text = text.TrimEnd('\n', '\r');
        if (!noNewline) text += "\n";
        if (makeRed) text = Util.Format(text, StrType.ERROR);

        terminalOutputField.AppendText(text);
    }
    static void Help(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        string fileName = parsedArgs.ContainsKey("-v") ? "helpVerbose.txt" : "helpShort.txt";
        FileAccess fileAccess = FileAccess.Open($"res://Utilities/TextFiles/CommandOutput/{fileName}", FileAccess.ModeFlags.Read);
        Say(fileAccess.GetAsText());
    }
    static void Home(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        CurrNode = networkManager.network;
    }
    const int MAX_HISTORY_CHAR_SIZE = 65536, RESET_HISTORY_CHAR_SIZE = 16384;
    static void Edit(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", "No file name provided."); return; }
        for (int i = 0; i < positionalArgs.Length; i++) {
            NodeFile file = _currDir.GetFile(positionalArgs[i]);
            if (file == null) {
                Say("-r", $"File not found: {Util.Format(positionalArgs[i], StrType.FILE)}");
                return;
            }
            overseer.textEditor.OpenNewFile(CurrNode.HostName, file);
        }
    }
    static void MkF(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No file name provided."); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            string[] components = positionalArgs[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string fileName = components[^1], parentPath = string.Join('/', components[..^1]);

            parentDirectory = CurrDir.GetDirectory(parentPath);
            if (parentDirectory == null) { Say("-r", $"Directory not found: {Util.Format(parentPath, StrType.DIR)}"); return; }
            int result = parentDirectory.AddFile(fileName);
            switch (result) {
                case 0: break;
                case 1: Say($"{Util.Format(positionalArgs[i], StrType.FILE)} already exists. Skipped."); break;
                default: Say("-r", $"{Util.Format(positionalArgs[i], StrType.FILE)} creation failed. Error code: ${result}"); break;
            }
        }
    }
    static void Scan(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        StartProcess(1.0, ScanCallback, parsedArgs, positionalArgs);
        void ScanCallback(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
            string L = " └─── ", M = " ├─── ", T = "      ", E = " │    ";
            Func<bool[], string> getTreePrefix = arr => string.Concat(arr.Select((b, i) => (i == arr.Length - 1 ? (b ? L : M) : (b ? T : E))));
            Func<bool[], string> getDescPrefix = arr => string.Concat(arr.Select((b, i) => (b ? T : E)));
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
                    output += $"{Util.Format(descPrefix, StrType.DECOR)}  Display Name: {Util.Format(analyzeResult.DisplayName, StrType.DISPLAY_NAME)}\n";
                    output += $"{Util.Format(descPrefix, StrType.DECOR)}  Node Type:    {Util.Format(analyzeResult.NodeType.ToString(), StrType.SYMBOL)}\n";
                    output += $"{Util.Format(descPrefix, StrType.DECOR)}  Defense:      {Util.Format(analyzeResult.DefLvl.ToString(), StrType.DEF_LVL)}  Security: {Util.Format(Util.MapEnum<SecurityType>(analyzeResult.SecLvl).ToString(), StrType.SEC_LVL, analyzeResult.RetLvl.ToString())}\n";
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
    }
    static void Farm(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        bool showStatus = parsedArgs.ContainsKey("-s") || parsedArgs.ContainsKey("--status");
        bool showUpgradeInfo = parsedArgs.ContainsKey("--upgrad3info");

        bool doUpgrade = parsedArgs.ContainsKey("-u") || parsedArgs.ContainsKey("--upgrade");
        bool hasLevel = parsedArgs.ContainsKey("-l") || parsedArgs.ContainsKey("--level");
        bool isAuto = parsedArgs.ContainsKey("-a") || parsedArgs.ContainsKey("--auto");

        // Disallow mixing upgrade with status/info
        if ((showStatus || showUpgradeInfo) && doUpgrade) { Say("-r", "Cannot combine upgrade with status or upgrade info."); return; }

        // Check node's ownership
        if (CurrNode.CurrentOwner != networkManager.network) { Say("-r", "You don't own this node's GC miner"); return; }
        // Status display
        if (showStatus || showUpgradeInfo) {
            int hackLvl = CurrNode.HackFarm.HackLvl, timeLvl = CurrNode.HackFarm.TimeLvl, growLvl = CurrNode.HackFarm.TimeLvl;
Say(@$"[ GC Farm Status for {CurrNode.DisplayName} ]
Hack  : Lv.{hackLvl} → {CurrNode.HackFarm.CurHack:n1} GC/transfer
Time  : Lv.{timeLvl} → {CurrNode.HackFarm.CurTime:n2} s/transfer
Grow  : Lv.{growLvl} → {CurrNode.HackFarm.CurGrow:n1} GC/s");

            if (showUpgradeInfo) {
                Say(@$"[ Upgrade Info ]
Hack +1 → {Enumerable.Range(hackLvl + 1, Math.Min(hackLvl + 2, 255) - hackLvl + 1).Sum(i => CurrNode.HackFarm.GetHackCost(i)):n1} | +10 → {Enumerable.Range(hackLvl + 1, Math.Min(hackLvl + 11, 255) - hackLvl + 1).Sum(i => CurrNode.HackFarm.GetHackCost(i)):n1}
Time +1 → {Enumerable.Range(timeLvl + 1, Math.Min(hackLvl + 2, 255) - timeLvl + 1).Sum(i => CurrNode.HackFarm.GetTimeCost(i)):n1} | +10 → {Enumerable.Range(timeLvl + 1, Math.Min(hackLvl + 11, 255) - timeLvl + 1).Sum(i => CurrNode.HackFarm.GetTimeCost(i)):n1}
Grow +1 → {Enumerable.Range(growLvl + 1, Math.Min(hackLvl + 2, 255) - growLvl + 1).Sum(i => CurrNode.HackFarm.GetGrowCost(i)):n1} | +10 → {Enumerable.Range(growLvl + 1, Math.Min(hackLvl + 11, 255) - growLvl + 1).Sum(i => CurrNode.HackFarm.GetGrowCost(i)):n1}");
            }
            return;
        }

        // Upgrade path
        if (doUpgrade) {
            string upgradeType = parsedArgs.GetValueOrDefault("-u") ?? parsedArgs.GetValueOrDefault("--upgrade") ?? "";
            int level = 1;

            if (!isAuto && !hasLevel) {
                Say("You must specify either --auto or --level with --upgrade.");
                return;
            }

            if (hasLevel) {
                if (!int.TryParse(parsedArgs.GetValueOrDefault("-l") ?? parsedArgs.GetValueOrDefault("--level"), out level) || level <= 0) {
                    Say("Invalid level amount.");
                    return;
                }
            }

            switch (upgradeType.ToLower()) {
                case "hack":
                    break;
                case "time":
                    break;
                case "grow":
                    break;
                default:
                    Say("Unknown upgrade type. Use: hack, time, or grow.");
                    return;
            }
            Say("Upgrade complete.");
        } else {
            Say("No action specified. Use --status or --upgrade.");
        }
    }
    static void Clear(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        terminalOutputField.Clear();
    }
    static void MkDir(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No directory name provided."); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            string[] components = positionalArgs[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string dirName = components[^1], parentPath = string.Join('/', components[..^1]);

            parentDirectory = CurrDir.GetDirectory(parentPath);
            if (parentDirectory == null) { Say("-r", $"Directory not found: {Util.Format(parentPath, StrType.DIR)}"); return; }
            int result = parentDirectory.AddDir(dirName);
            switch (result) {
                case 0: break;
                case 1: Say($"{Util.Format(parentPath, StrType.DIR)} already exists. Skipped."); break;
                default: Say("-r", $"{Util.Format(parentPath, StrType.DIR)} creation failed. Error code: ${result}"); break;
            }
        }
    }
    static void RmDir(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) {
            Say("-r", "-No directory name provided.");
            return;
        }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            parentDirectory = CurrDir.GetDirectory(positionalArgs[i]);
            if (parentDirectory == null) { Say("-r", $"Directory not found: {Util.Format(positionalArgs[0], StrType.DIR)}"); return; }
            if (parentDirectory.Parent == null) { Say("-r", $"Can not remove root."); return; }
            parentDirectory = parentDirectory.Parent;
            int result = parentDirectory.RemoveDir(positionalArgs[i]);
            switch (result) {
                case 0: break;
                case 1: Say("-r", $"{Util.Format(positionalArgs[i], StrType.DIR)} was not found."); break;
                default: Say("-r", $"{Util.Format(positionalArgs[i], StrType.DIR)} removal failed. Error code: {result}"); break;
            }
        }
    }
    static double startEpoch = 0, endEpoch = 0, remainingTime = 0;
    const double FLARE_TIME = 60.0;
    static int Crack(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        NetworkNode node = CurrNode;
        if (parsedArgs.ContainsKey("--flare")) {
            if (Time.GetUnixTimeFromSystem() < endEpoch) {
                Say("Karaxe already in effect.");
                return 1;
            }
            startEpoch = Time.GetUnixTimeFromSystem(); remainingTime = FLARE_TIME; endEpoch = startEpoch + remainingTime;
            Say($"{Util.Format("Kraken axe activated", StrType.FULL_SUCCESS)}. All node defense's system {Util.Format("~=EXP0SED=~", StrType.ERROR)}");
            return 2;
        }
        if (Time.GetUnixTimeFromSystem() > endEpoch) {
            Say($"{Util.Format("Kraken axe inactive", StrType.ERROR)}. Run {Util.Format("karaxe --flare", StrType.CMD)} to activate.");
            return 3;
        }

        int result = node.AttempCrackNode(parsedArgs, endEpoch);
        if (result == 0) { 
            node.TransferOwnership(networkManager.network); 
        }
        return 0;
    }
    static void Inspect(Dictionary<string, string> parsedArgs, string[] positionalArgs) {

    }
    static void Connect(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No hostname provided."); return; }
        if (networkManager.DNS.TryGetValue(positionalArgs[0], out NetworkNode value)) { CurrNode = value; return; }
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
        StartProcess(.5 + GD.Randf() * .5, AnalyzeCallback, parsedArgs, positionalArgs);
        void AnalyzeCallback(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
            NetworkNode analyzeNode = null;
            if (CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]) != null) analyzeNode = CurrNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]);
            else if (CurrNode.ParentNode != null && CurrNode.ParentNode.HostName == positionalArgs[0]) analyzeNode = CurrNode.ParentNode;
            else if (CurrNode.HostName == positionalArgs[0]) analyzeNode = CurrNode;
            Say("-tl", $@"
{Util.Format("▶ Node Info", StrType.HEADER)}
{Util.Format("Host name:", StrType.DECOR)}      {Util.Format(analyzeNode.HostName, StrType.HOSTNAME)}
{Util.Format("IP address:", StrType.DECOR)}     {Util.Format(analyzeNode.IP, StrType.IP)}
{Util.Format("Display name:", StrType.DECOR)}   {Util.Format(analyzeNode.DisplayName, StrType.DISPLAY_NAME)}
{Util.Format("▶ Classification", StrType.HEADER)}
{Util.Format("Node type:", StrType.DECOR)}      {Util.Format($"{analyzeNode.NodeType}", StrType.SYMBOL)}
{Util.Format("Defense level:", StrType.DECOR)}  {Util.Format($"{analyzeNode.DefLvl}", StrType.DEF_LVL)}
{Util.Format("Security level:", StrType.DECOR)} {Util.Format($"{analyzeNode.SecType}", StrType.SEC_LVL)}
");

            // Honeypot node don't dare to impersonate actual organization or corporation.
            if (analyzeNode.CurrentOwner != networkManager.network || analyzeNode.GetType() == typeof(HoneypotNode)) {
                Say($"Crack this node security system to get further access.");
                return;
            }
            Say("-tl", $@"
{Util.Format("▶ GC miner detail", StrType.HEADER)}
{Util.Format("Transfer batch:", StrType.DECOR)} {Util.Format(analyzeNode.HackFarm.HackLvl.ToString(), StrType.NUMBER)} ({Util.Format(analyzeNode.HackFarm.CurHack.ToString("F2"), StrType.MONEY)})
{Util.Format("Transfer speed:", StrType.DECOR)} {Util.Format(analyzeNode.HackFarm.TimeLvl.ToString(), StrType.NUMBER)} ({Util.Format(analyzeNode.HackFarm.CurTime.ToString("F2"), StrType.UNIT, "s")})
{Util.Format("Mining speed:", StrType.DECOR)}   {Util.Format(analyzeNode.HackFarm.GrowLvl.ToString(), StrType.NUMBER)} ({Util.Format(analyzeNode.HackFarm.CurGrow.ToString("F2"), StrType.UNIT, "GC/s")})
");
            if (analyzeNode is FactionNode) {
                Say("-tl", $@"
{Util.Format("▶ Faction detail", StrType.HEADER)}
{Util.Format("Name:", StrType.DECOR)}           {Util.Format(Util.Obfuscate((analyzeNode as FactionNode).Faction.Name), StrType.DISPLAY_NAME)}
{Util.Format("Description:", StrType.DECOR)}    {Util.Format(Util.Obfuscate((analyzeNode as FactionNode).Faction.Desc), StrType.DESC)}
");
            }
            if (analyzeNode is BusinessNode) {
                Say("-tl", $@"
{Util.Format("▶ Business detail", StrType.HEADER)}
{Util.Format("Stock:", StrType.DECOR)}          {Util.Format((analyzeNode as BusinessNode).Stock.Name, StrType.DISPLAY_NAME)}
{Util.Format("Value:", StrType.DECOR)}          {Util.Format((analyzeNode as BusinessNode).Stock.Price.ToString("F2"), StrType.MONEY)}
");
            }
            if (analyzeNode is CorpNode) {
                Say("-tl", $@"
{Util.Format("▶ Faction detail", StrType.HEADER)}
{Util.Format("Name:", StrType.DECOR)}           {Util.Format(Util.Obfuscate((analyzeNode as CorpNode).Faction.Name), StrType.DISPLAY_NAME)}
{Util.Format("Description:", StrType.DECOR)}    {Util.Format(Util.Obfuscate((analyzeNode as CorpNode).Faction.Desc), StrType.DESC)}
{Util.Format("▶ Business detail", StrType.HEADER)}
{Util.Format("Stock:", StrType.DECOR)}          {Util.Format((analyzeNode as CorpNode).Stock.Name, StrType.DISPLAY_NAME)}
{Util.Format("Value:", StrType.DECOR)}          {Util.Format((analyzeNode as CorpNode).Stock.Price.ToString("F2"), StrType.MONEY)}
");
            }
        }
    }
    static (Dictionary<string, string>, string[]) ParseArgs(string[] args) {
        Dictionary<string, string> parsedArgs = [];
        List<string> positionalArgs = [];
        for (int i = 0; i < args.Length; ++i) {
            if (args[i].StartsWith("--")) {
                if (i + 1 < args.Length && !args[i + 1].StartsWith('-')) { parsedArgs[args[i]] = args[i + 1]; ++i; } 
                else parsedArgs[args[i]] = "";
                continue;
            }
            // if it's only one short flag, allow it to take in arg, otherwise, no arg.
            if (args[i].StartsWith('-')) {
                if (args[i].Length == 2) { 
                    if (i + 1 < args.Length && !args[i + 1].StartsWith('-')) parsedArgs[args[i]] = args[i + 1];
                    else parsedArgs[args[i]] = "";
                    ++i; 
                } else for (int j = 1; j < args[i].Length; ++j) { parsedArgs[('-' + args[i][j]).ToString()] = ""; }
            } else { positionalArgs.Add(args[i]); }
        }
        return (parsedArgs, positionalArgs.ToArray());
    }
    static void SetCommandPrompt() { terminalCommandPrompt.Text = $"{Util.Format(networkManager.UserName, StrType.USERNAME)}@{Util.Format(CurrNode.HostName, StrType.HOSTNAME)}:{Util.Format(CurrDir.GetPath(), StrType.DIR)}>"; }
    static string EscapeBBCode(string code) { return code.Replace("[", "[lb]"); }
    public static void OnCommandFieldTextChanged() {
        terminalCommandField.Text = terminalCommandField.Text.Replace("\n", "").Replace("\r", "");
        terminalCommandField.SetCaretColumn(terminalCommandField.Text.Length);
    }

    const int INSTA_FILL_MARGIN = 10;
    const double TIME_TIL_NEXT_LINE = .05; static double timeLeft = TIME_TIL_NEXT_LINE;
    static void ShowMoreChars(double delta) {
        // Get current number of visible characters
        int curChar = terminalOutputField.VisibleCharacters;
        int allChar = terminalOutputField.GetTotalCharacterCount();
        if (curChar >= allChar) return;

        // Get full text and split into lines
        string allText = terminalOutputField.GetParsedText();
        string[] lines = allText.Split('\n');

        // Get total and current visible lines
        int allLines = lines.Length; int curLines = CountVisibleLines(lines, curChar);
        int lineDelta = 0;
        if (allLines - curLines > INSTA_FILL_MARGIN) { lineDelta += (int)Math.Ceiling((allLines - curLines - INSTA_FILL_MARGIN) * .1); }
        timeLeft -= delta;
        if (timeLeft < 0) { timeLeft += TIME_TIL_NEXT_LINE; lineDelta += 1; }
        int newLineIndex = curLines + lineDelta;

        // Update chars
        int charsToShow = GetCharacterIndexAtLine(lines, newLineIndex);
        terminalOutputField.VisibleCharacters = Mathf.Clamp(charsToShow, 0, allText.Length);
    }
    static int CountVisibleLines(string[] lines, int visibleCharCount) {
        int sum = 0, i = -1;
        for (int j = 0; j < lines.Length; j++) {
            sum += lines[j].Length + 1;
            if (sum > visibleCharCount) break;
            i = j;
        }
        return i+1;
    }
    static int GetCharacterIndexAtLine(string[] lines, int lineIndex) {
        return lines.Take(lineIndex + 1).Sum(s => s.Length+1);
    }
}