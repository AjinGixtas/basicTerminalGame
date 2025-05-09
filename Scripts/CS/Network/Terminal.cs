using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Terminal : MarginContainer {
    public Overseer overseer;
    RichTextLabel terminalOutputField; RichTextLabel terminalCommandPrompt; LineEdit terminalCommandField; Timer processTimer, updateProcessGraphicTimer;
    NetworkManager networkManager;
    NetworkNode currentNode = null; NodeDirectory currentDirectory = null;
    NodeDirectory CurrentDirectory  { get { return currentDirectory; } set { currentDirectory = value; SetCommandPrompt(); } }
    NetworkNode CurrentNode         { get { return currentNode; } set { currentNode = value; currentDirectory = currentNode.NodeDirectory; SetCommandPrompt(); } }
    string userName;
    readonly List<string> commandHistory = []; int commandHistoryIndex = 0; 
    int CommandHistoryIndex { 
        get => commandHistoryIndex; 
        set {
            if (commandHistoryIndex == value) { return; }
            commandHistoryIndex = Math.Clamp(value, 0, commandHistory.Count-1);
            if (commandHistoryIndex < commandHistory.Count) {
                terminalCommandField.Text = commandHistory[commandHistoryIndex];
                terminalCommandField.CaretColumn = commandHistory[commandHistoryIndex].Length;
            }
        } 
    }
    [Export] string UserName { get { return userName; } set { userName = value; SetCommandPrompt(); } }
    bool isProcessing = false;
    public override void _Ready() {
        IntializeOnReadyVar();
        currentNode = networkManager.network;
        CurrentDirectory = currentNode.NodeDirectory;
        CurrentDirectory.AddFile("pootis.txt");
        SetCommandPrompt();
        terminalCommandField.Edit();
    }
    void IntializeOnReadyVar() {
        terminalOutputField = GetNode<RichTextLabel>("Splitter/TerminalOutputArea");
        terminalCommandField = GetNode<LineEdit>("Splitter/TerminalCommandLine/CommandField");
        terminalCommandPrompt = GetNode<RichTextLabel>("Splitter/TerminalCommandLine/CommandPrompt");
        processTimer = GetNode<Timer>("Splitter/TimersContainer/ProcessTimer");
        updateProcessGraphicTimer = GetNode<Timer>("Splitter/TimersContainer/UpdateProcessGraphicTimer");
        networkManager = GetNode<NetworkManager>("Splitter/NetworkManager");
    }

    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("moveDownHistory")) CommandHistoryIndex += 1;
        if (Input.IsActionJustPressed("moveUpHistory")) CommandHistoryIndex -= 1;
    }

    Action<Dictionary<string, string>, string[]> finishFunction; (Dictionary<string, string>, string[]) finishFunctionArgs;
    void StartProcess(double time, Action<Dictionary<string, string>, string[]> func, Dictionary<string, string> parseArgs, string[] positionalArgs) {
        if (isProcessing) return;
        finishFunction = func; finishFunctionArgs = (parseArgs, positionalArgs);
        isProcessing = true;
        processTimer.WaitTime = time;
        terminalCommandField.Editable = false;
        progress = 0;
        //tick = GD.RandRange(0, 3);
        //int filledBar = Mathf.FloorToInt(progress / 100.0 * BarSize);
        Say("-n", $"Start");
        processTimer.Start();
        updateProcessGraphicTimer.Start();
    }
    static readonly string[] SpinnerChars = ["|", "/", "-", "\\"]; int tick = 0, progress = 0;
    public void UpdateProcessingGraphic() {
        if (!isProcessing) return;

        int t = GD.RandRange(0, 1+(int)Math.Floor(150.0 / processTimer.WaitTime * updateProcessGraphicTimer.WaitTime));
        progress = Mathf.Clamp(progress + t, 0, 99); tick = (tick + 1) % SpinnerChars.Length;

        Say("-n", $"...{progress}%");
        updateProcessGraphicTimer.Start();
    }
    public void ProcessFinished() {
        isProcessing = false; terminalCommandField.Editable = true; terminalCommandField.Edit();
        tick = (tick + 1) % SpinnerChars.Length;
        Say($"...Done!");
        progress = 0; tick = 0;
        finishFunction(finishFunctionArgs.Item1, finishFunctionArgs.Item2);
    }
    const int MAX_HISTORY_CMD_SIZE = 64;
    public void SubmitCommand(string newText) {
        terminalCommandField.Text = "";
        if (commandHistory.Count == 0 || (commandHistory.Count > 0 && commandHistory[^1] != newText)) { 
            commandHistory.Add(newText); 
            while (commandHistory.Count > MAX_HISTORY_CMD_SIZE) { commandHistory.RemoveAt(0); }
            commandHistoryIndex = commandHistory.Count; 
        }
        string[] commands = newText.Split(';', StringSplitOptions.RemoveEmptyEntries);
        Say($"{terminalCommandPrompt.Text}{newText}");
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
    bool ProcessCommand(string command, string[] args) {
        command = command.ToLower();
        (Dictionary<string, string> parsedArgs, string[] positionalArgs) = ParseArgs(args);
        switch (command) {
            case "mkf": MkF(parsedArgs, positionalArgs); break;
            case "rmf": RmF(parsedArgs, positionalArgs); break;
            case "ls": LS(parsedArgs, positionalArgs); break;
            case "cd": CD(parsedArgs, positionalArgs); break;
            case "pwd": Pwd(parsedArgs, positionalArgs); break;
            case "say": Say(parsedArgs, positionalArgs); break;
            case "help": Help(parsedArgs, positionalArgs); break;
            case "home": Home(parsedArgs, positionalArgs); break;
            case "edit": Edit(parsedArgs, positionalArgs); break;
            case "scan": Scan(parsedArgs, positionalArgs); return false;
            case "clear": Clear(parsedArgs, positionalArgs); break;
            case "mkdir": MkDir(parsedArgs, positionalArgs); break;
            case "rmdir": RmDir(parsedArgs, positionalArgs); break;
            case "crack": Crack(parsedArgs, positionalArgs); break;
            case "inspect": Inspect(parsedArgs, positionalArgs); break;
            case "connect": Connect(parsedArgs, positionalArgs); break;
            case "analyze": Analyze(parsedArgs, positionalArgs); return false;
            default: Say("-r", $"{command} is not a valid command."); break;
        }
        return true;
    }

    void RmF(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No file name provided."); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            string[] components = positionalArgs[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string fileName = components[^1], parentPath = string.Join('/', components[..^1]);
            parentDirectory = CurrentDirectory.GetDirectory(parentPath);
            if (parentDirectory == null) { Say("-r", $"File not found: {parentPath}"); return; }
            int result = parentDirectory.RemoveFile(positionalArgs[i]);
            switch (result) {
                case 0: break;
                case 1: Say($"{positionalArgs[i]} was not found."); break;
                default: Say("-r", $"{positionalArgs[i]} removal failed. Error code: {result}"); break;
            }
        }
    }
    void LS(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        NodeDirectory targetDir = CurrentDirectory;
        if (positionalArgs.Length != 0) {
            targetDir = CurrentDirectory.GetDirectory(positionalArgs[0]);
            if (targetDir == null) { Say("-r", $"Directory not found: {positionalArgs[0]}"); return; }
        }

        if (targetDir.Childrens.Count == 0) { Say(""); return; }
        string output = "";
        foreach (NodeSystemItem item in targetDir.Childrens) {
            if (item is NodeDirectory) { output += $"[color={Util.CC(Cc.C)}]/{item.Name}[/color] "; } 
            else { output += $"[color={Util.CC(Cc.G)}]{item.Name}[/color] "; }
        }
        Say(output);
    }
    void CD(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", "No directory provided."); return; }
        NodeDirectory targetDir = CurrentDirectory.GetDirectory(positionalArgs[0]);
        if (targetDir == null) { Say("-r", $"Directory not found: {positionalArgs[0]}"); return; }
        CurrentDirectory = targetDir;
    }
    void Pwd(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        Say($"{CurrentDirectory.GetPath()}");
    }
    // It gets 2 since this one is REALLY close to standard, but since it's also called independently a lot, allow for seperate shorthand flag is way better.
    void Say(Dictionary<string, string> parseArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say(""); return; }
        string c = terminalOutputField.GetParsedText();
        if (c.Length >= MAX_HISTORY_CHAR_SIZE) {
            terminalOutputField.Clear();
            terminalOutputField.AppendText(c[^RESET_HISTORY_CHAR_SIZE..]);
        }
        string text = string.Join(" ", positionalArgs);
        if (parseArgs.ContainsKey("-e")) text = Regex.Unescape(text); // handles \n, \t, etc.
        if (!parseArgs.ContainsKey("-n")) text += "\n";
        if (parseArgs.ContainsKey("-r")) text = $"[color={Util.CC(Cc.R)}]{text}[/color]";
        if (parseArgs.ContainsKey("-t") && text.EndsWith('\n')) text = text[..^1]; // Trim only one trailing newline
        terminalOutputField.AppendText(text);
    }
    void Say(params string[] args) {
        if (args.Length == 0) { Say(""); return; }

        string c = terminalOutputField.GetParsedText();
        if (c.Length >= MAX_HISTORY_CHAR_SIZE) {
            terminalOutputField.Clear();
            terminalOutputField.AppendText(c[^RESET_HISTORY_CHAR_SIZE..]);
        }

        bool noNewline = false, interpretEscapes = false, makeRed = false, trimTrailingNewline = false;
        int argStart = 0;

        // Handle optional flags like -n, -e, -r, -t
        if (args[0].StartsWith('-')) {
            string flags = args[0];
            noNewline = flags.Contains('n');
            interpretEscapes = flags.Contains('e');
            makeRed = flags.Contains('r');
            trimTrailingNewline = flags.Contains('t');
            argStart = 1;
        }

        string text = string.Join(" ", args[argStart..]);
        if (interpretEscapes) text = Regex.Unescape(text); // handles \n, \t, etc.
        if (trimTrailingNewline && text.EndsWith('\n')) text = text[..^1]; // Trim only one trailing newline
        if (!noNewline) text += "\n";
        if (makeRed) text = $"[color={Util.CC(Cc.R)}]{text}[/color]";

        terminalOutputField.AppendText(text);
    }
    void Help(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        string fileName = parsedArgs.ContainsKey("-v") ? "helpVerbose.txt" : "helpShort.txt";
        FileAccess fileAccess = FileAccess.Open($"res://Utilities/TextFiles/CommandOutput/{fileName}", FileAccess.ModeFlags.Read);
        Say(fileAccess.GetAsText());
    }
    void Home(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        CurrentNode = networkManager.network;
    }
    const int MAX_HISTORY_CHAR_SIZE = 65536, RESET_HISTORY_CHAR_SIZE = 16384;
    void Edit(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", "No file name provided."); return; }
        for (int i = 0; i < positionalArgs.Length; i++) {
            NodeFile file = currentDirectory.GetFile(positionalArgs[i]);
            if (file == null) {
                Say("-r", $"File not found: {positionalArgs[i]}");
                return;
            }
            overseer.textEditor.OpenNewFile(CurrentNode.HostName, file);
        }
    }
    void MkF(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No file name provided."); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            string[] components = positionalArgs[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string fileName = components[^1], parentPath = string.Join('/', components[..^1]);

            parentDirectory = CurrentDirectory.GetDirectory(parentPath);
            if (parentDirectory == null) { Say("-r", $"Directory not found: {parentPath}"); return; }
            int result = parentDirectory.AddFile(fileName);
            switch (result) {
                case 0: break;
                case 1: Say($"{positionalArgs[i]} already exists. Skipped."); break;
                default: Say("-r", $"{positionalArgs[i]} creation failed. Error code: ${result}"); break;
            }
        }
    }
    void Scan(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        StartProcess(.1, ScanCallback, parsedArgs, positionalArgs);
        void ScanCallback(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
            string L = " └─── ", M = " ├─── ", T = "      ", E = " │    ";
            Func<bool[], string> getTreePrefix = arr => string.Concat(arr.Select((b, i) => (i == arr.Length - 1 ? (b ? L : M) : (b ? T : E))));
            Func<bool[], string> getDescPrefix = arr => string.Concat(arr.Select((b, i) => (b ? T : E)));
            if (!parsedArgs.TryGetValue("-d", out string value)) { value = "1"; parsedArgs["-d"] = value; }
            if (!int.TryParse(value, out int MAX_DEPTH)) { Say("-r", $"Invalid depth: {value}"); return; }

            Stack<(NetworkNode, int, bool[])> stack = new([(currentNode, 0, [])]);
            List<NetworkNode> visited = [];
            string output = "";
            while (stack.Count > 0) {
                (NetworkNode node, int depth, bool[] depthMarks) = stack.Pop();
                if (depth > MAX_DEPTH || visited.Contains(node)) continue;
                Dictionary<string, string> analyzeResult = node.Analyze();
                output += $"[color={Util.CC(Cc.rgb)}]{getTreePrefix(depthMarks)}[/color][color={Util.CC(Cc.C)}]{analyzeResult["IP"], -15}[/color] [color=green]{analyzeResult["hostName"]}[/color]\n";
                
                if (parsedArgs.ContainsKey("-v")) {
                    string defColorCode = analyzeResult["defLvl"] switch {
                        "0" or "1" or "2" => Util.CC(Cc.B),
                        "3" or "4" or "5" or "6" or "7" => Util.CC(Cc.Y),
                        "8" or "9" or "10" => Util.CC(Cc.R),
                        _ => Util.CC(Cc.C)
                    };
                    string secColorCode = analyzeResult["secType"] switch {
                        "NOSEC" or "LOSEC" => Util.CC(Cc.R),
                         "MISEC" => Util.CC(Cc.Y),
                        "HISEC" or "MASEC" => Util.CC(Cc.B),
                        _ => Util.CC(Cc.R)
                    };
                    string descPrefix = getDescPrefix(depthMarks) + ((depth == MAX_DEPTH ? 0 : ((node.ParentNode != null ? 0 : -1) + node.ChildNode.Count)) > 0 ? " │" : "  ");
                    output += $"[color={Util.CC(Cc.rgb)}]{descPrefix}[/color]  Display Name: [color={Util.CC(Cc.gR)}]{analyzeResult["displayName"]}[/color];\n";
                    output += $"[color={Util.CC(Cc.rgb)}]{descPrefix}[/color]  Node Type:    [color={Util.CC(Cc.m)}]{analyzeResult["nodeType"]}[/color];\n";
                    output += $"[color={Util.CC(Cc.rgb)}]{descPrefix}[/color]  Defense:      [color={defColorCode}]{analyzeResult["defLvl"]}[/color]  "
                            + $"Security: [color={secColorCode}]{analyzeResult["secType"]}[/color]\n";
                    output += $"[color={Util.CC(Cc.rgb)}]{descPrefix}[/color]\n";
                }
                visited.Add(node);

                // Use k==1 due to FILO algorithm.
                List<NetworkNode> nextOfQueue = [];
                if (node.ParentNode != null && !visited.Contains(node.ParentNode) && depth <= MAX_DEPTH) nextOfQueue.Add(node.ParentNode);
                foreach(NetworkNode child in node.ChildNode) {
                    if (visited.Contains(child) || depth > MAX_DEPTH) continue;
                    nextOfQueue.Add(child);
                }

                int k = 0; foreach(NetworkNode child in nextOfQueue) { 
                    ++k; stack.Push((child, depth + 1, [..depthMarks, k == 1]));
                }
            }
            Say(output);
        }
    }
    void Clear(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        terminalOutputField.Clear();
    }
    void MkDir(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No directory name provided."); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            string[] components = positionalArgs[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string dirName = components[^1], parentPath = string.Join('/', components[..^1]);

            parentDirectory = CurrentDirectory.GetDirectory(parentPath);
            if (parentDirectory == null) { Say("-r", $"Directory not found: {parentPath}"); return; }
            int result = parentDirectory.AddDir(dirName);
            switch (result) {
                case 0: break;
                case 1: Say($"{positionalArgs[i]} already exists. Skipped."); break;
                default: Say("-r", $"{positionalArgs[i]} creation failed. Error code: ${result}"); break;
            }
        }
    }   
    void RmDir(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) {
            Say("-r", "-No directory name provided.");
            return;
        }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            parentDirectory = CurrentDirectory.GetDirectory(positionalArgs[i]);
            if (parentDirectory == null) { Say("-r", $"Directory not found: {positionalArgs[0]}"); return; }
            if (parentDirectory.Parent == null) { Say("-r", $"Can not remove root."); return; }
            parentDirectory = parentDirectory.Parent;
            int result = parentDirectory.RemoveDir(positionalArgs[i]);
            switch(result) {
                case 0: break;
                case 1: Say("-r", $"{positionalArgs[i]} was not found."); break;
                default: Say("-r", $"{positionalArgs[i]} removal failed. Error code: {result}"); break;
            }
        }
    }
    void Crack(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        NetworkNode node = CurrentNode;
        for(int i = 0; i < positionalArgs.Length; ++i) { GD.Print(positionalArgs[i]); }
        if (parsedArgs.ContainsKey("--init")) {
            int beginResult = node.BeginCrackNode(); 
            if (beginResult == 0) { Say("Cracking begin."); }
            else if (beginResult == 1) { Say("Cracking already in process."); }
            else if (beginResult == 2) { Say("No security system found."); } 
            else { Say("-r", $"Unknown error: {beginResult}"); }
            return;
        }
        (int result, string msg) = node.AttempCrackNode(parsedArgs);
        Say("-t", msg);
        if (result == 0) {
            Say($"[color={Util.CC(Cc.G)}]Node defense cracked.[/color] All security system [color={Util.CC(Cc.gR)}]destroyed[/color].");
            Say($"Run [color={Util.CC(Cc.C)}]analyze[/color] for all new information.");
            node.TransferOwnership(networkManager.network);
        }
    }
    void Inspect(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        
    }
    void Connect(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No hostname provided."); return; }
        if (networkManager.DNS.TryGetValue(positionalArgs[0], out NetworkNode value)) { CurrentNode = value; return; }
        if (CurrentNode.ParentNode != null && CurrentNode.ParentNode.HostName == positionalArgs[0] || 
            (CurrentNode.ParentNode is HoneypotNode && (CurrentNode.ParentNode as HoneypotNode).fakeHostName == positionalArgs[0])) 
            { CurrentNode = CurrentNode.ParentNode; return; }
        
        NetworkNode node = CurrentNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]);
        NetworkNode fnode = CurrentNode.ChildNode.FindLast(s => s is HoneypotNode && (s as HoneypotNode).fakeHostName == positionalArgs[0]);
        if (node == null && fnode == null) { Say("-r", $"Host not found: {positionalArgs[0]}"); return; }
        CurrentNode = (node ?? fnode);
    }
    void Analyze(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { positionalArgs = [CurrentNode.HostName]; }
        if (CurrentNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]) == null 
            && (CurrentNode.ParentNode != null && CurrentNode.ParentNode.HostName != positionalArgs[0])
            && CurrentNode.HostName != positionalArgs[0]) {
            Say("-r", $"Host not found: {positionalArgs[0]}");
            return;
        }
        StartProcess(.5 + GD.Randf() * .5, AnalyzeCallback, parsedArgs, positionalArgs);
        void AnalyzeCallback(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
            NetworkNode analyzeNode = null;
            if (CurrentNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]) != null) analyzeNode = CurrentNode.ChildNode.FindLast(s => s.HostName == positionalArgs[0]);
            else if (CurrentNode.ParentNode != null && CurrentNode.ParentNode.HostName == positionalArgs[0]) analyzeNode = CurrentNode.ParentNode;
            else if (CurrentNode.HostName == positionalArgs[0]) analyzeNode = CurrentNode;
            string defColorCode = analyzeNode.DefLvl switch {
                < 3 => Util.CC(Cc.B),
                < 8 => Util.CC(Cc.Y),
                < 10 => Util.CC(Cc.R),
                _ => Util.CC(Cc.R)
            };
            string secColorCode = analyzeNode.SecType switch {
                SecurityType.NOSEC or SecurityType.LOSEC => Util.CC(Cc.R),
                SecurityType.MISEC => Util.CC(Cc.Y),
                SecurityType.HISEC or SecurityType.MASEC => Util.CC(Cc.B),
                _ => Util.CC(Cc.R)
            };
            Say($"[color={Util.CC(Cc.gR)}]▶ Node Info[/color]");
            Say($"[color={Util.CC(Cc.rgb)}]Host name:[/color]      [color={Util.CC(Cc.G)}]{analyzeNode.HostName}[/color]");
            Say($"[color={Util.CC(Cc.rgb)}]IP address:[/color]     [color={Util.CC(Cc.C)}]{analyzeNode.IP}[/color]");
            Say($"[color={Util.CC(Cc.rgb)}]Display name:[/color]   [color={Util.CC(Cc.y)}]{analyzeNode.DisplayName}[/color]");

            Say($"[color={Util.CC(Cc.gR)}]▶ Classification[/color]");
            Say($"[color={Util.CC(Cc.rgb)}]Node type:[/color]      [color={Util.CC(Cc.m)}]{analyzeNode.NodeType}[/color]");
            Say($"[color={Util.CC(Cc.rgb)}]Defense level:[/color]  [color={defColorCode}]{analyzeNode.DefLvl}[/color]");
            Say($"[color={Util.CC(Cc.rgb)}]Security level:[/color] [color={secColorCode}]{analyzeNode.SecType}[/color]");

            if (analyzeNode.CurrentOwner != networkManager.network) {
                Say($"Crack this node security system to get further access.");
                return; // Not yours yet, no more detail!
            }
            Say($"[color={Util.CC(Cc.gR)}]▶ GC miner detail[/color]");
            Say($"[color={Util.CC(Cc.rgb)}]Transfer batch:[/color] [color={Util.CC(Cc.c)}]{analyzeNode.HackFarm.HackLevel}[/color] ([color={Util.CC(Cc.c)}]{analyzeNode.HackFarm.CurHack:F2}[/color] [color={Util.CC(Cc.rgb)}]GC[/color])");
            Say($"[color={Util.CC(Cc.rgb)}]Transfer speed:[/color] [color={Util.CC(Cc.c)}]{analyzeNode.HackFarm.TimeLevel}[/color] ([color={Util.CC(Cc.c)}]{analyzeNode.HackFarm.CurTime:F2}[/color] [color={Util.CC(Cc.rgb)}]s[/color])");
            Say($"[color={Util.CC(Cc.rgb)}]Mining speed:[/color]   [color={Util.CC(Cc.c)}]{analyzeNode.HackFarm.GrowLevel}[/color] ([color={Util.CC(Cc.c)}]{analyzeNode.HackFarm.CurGrow:F2}[/color] [color={Util.CC(Cc.rgb)}]GC/s[/color])");
            GD.Print(Util.CC(Cc.c));

            if (analyzeNode is FactionNode) {
                Say($"[color={Util.CC(Cc.gR)}]▶ Faction detail[/color]");
                Say($"[color={Util.CC(Cc.rgb)}]Name:[/color]           [color={Util.CC(Cc.y)}]{Util.Obfuscate((analyzeNode as FactionNode).Faction.Name)}[/color]");
                Say($"[color={Util.CC(Cc.rgb)}]Description:[/color]    [color={Util.CC(Cc.Y)}]{Util.Obfuscate((analyzeNode as FactionNode).Faction.Desc)}[/color]");
            }
            if (analyzeNode is BusinessNode) {
                Say($"[color={Util.CC(Cc.gR)}]▶ Business detail[/color]");
                Say($"[color={Util.CC(Cc.rgb)}]Stock:[/color]          [color={Util.CC(Cc.y)}]{(analyzeNode as BusinessNode).Stock.Name}[/color]");
                Say($"[color={Util.CC(Cc.rgb)}]Value:[/color]          [color={Util.CC(Cc.C)}]{(analyzeNode as BusinessNode).Stock.Price}[/color]");
            }
            if (analyzeNode is CorpNode) {
                Say($"[color={Util.CC(Cc.gR)}]▶ Faction detail[/color]");
                Say($"[color={Util.CC(Cc.rgb)}]Name:[/color]           [color={Util.CC(Cc.y)}]{Util.Obfuscate((analyzeNode as CorpNode).Faction.Name)}[/color]");
                Say($"[color={Util.CC(Cc.rgb)}]Description:[/color]    [color={Util.CC(Cc.Y)}]{Util.Obfuscate((analyzeNode as CorpNode).Faction.Desc)}[/color]");
                Say($"[color={Util.CC(Cc.gR)}]▶ Business detail[/color]");
                Say($"[color={Util.CC(Cc.rgb)}]Stock:[/color]          [color={Util.CC(Cc.y)}]{(analyzeNode as CorpNode).Stock.Name}[/color]");
                Say($"[color={Util.CC(Cc.rgb)}]Value:[/color]          [color={Util.CC(Cc.C)}]{(analyzeNode as CorpNode).Stock.Price}[/color] [color={Util.CC(Cc.Y)}]GC[/color]");
            }
        }
    }
    static (Dictionary<string, string>, string[]) ParseArgs(string[] args) {
        Dictionary<string, string> parsedArgs = [];
        List<string> positionalArgs = [];
        for (int i = 0; i < args.Length; ++i) {
            if (args[i].StartsWith("--")) {
                if (i + 1 < args.Length && !args[i + 1].StartsWith('-') && !args[i + 1].StartsWith("--")) {
                    parsedArgs[args[i]] = args[i + 1]; ++i;
                } else parsedArgs[args[i]] = "";
            } else if (args[i].StartsWith('-')) {
                if (args[i].Length == 2) {
                    if (i + 1 < args.Length && !args[i + 1].StartsWith('-') && !args[i + 1].StartsWith("--")) {
                        parsedArgs[args[i]] = args[i + 1]; ++i;
                    } else parsedArgs[args[i]] = "";
                } else for (int j = 1; j < args[i].Length; ++j) { parsedArgs['-' + args[i][j].ToString()] = ""; }
            } else { positionalArgs.Add(args[i]); }
        }
        return (parsedArgs, positionalArgs.ToArray());
    }
    void SetCommandPrompt() { terminalCommandPrompt.Text = $"[color={Util.CC(Cc.m)}]{networkManager.UserName}[/color]@[color={Util.CC(Cc.G)}]{CurrentNode.HostName}[/color]:{CurrentDirectory.GetPath()}>"; }
    string EscapeBBCode(string code) { return code.Replace("[", "[lb]"); }
}
