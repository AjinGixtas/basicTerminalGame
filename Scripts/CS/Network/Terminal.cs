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
        CurrentDirectory.Add(new NodeFile("pootis.txt"));
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

    Action<string[]> finishFunction; string[] finishFunctionArgs;
    void StartProcess(double time, Action<string[]> func, string[] args) {
        if (isProcessing) return;
        finishFunction = func; finishFunctionArgs = args;
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
        finishFunction(finishFunctionArgs);
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
            var matches = Regex.Matches(input, @"[\""].+?[\""]|\S+");
            return [.. matches.Select(m => m.Value.Trim('"'))];
        }
    }
    bool ProcessCommand(string command, string[] args) {
        command = command.ToLower();
        switch (command) {
            case "ls": LS(args); break;
            case "cd": CD(args); break;
            case "pwd": Pwd(args); break;
            case "say": Say(args); break;
            case "help": Help(args); break;
            case "home": Home(args); break;
            case "edit": Edit(args); break;
            case "newf": Newf(args); break;
            case "scan": Scan(args); return false;
            case "clear": Clear(args); break;
            case "mkdir": MkDir(args); break;
            case "rmdir": RmDir(args); break;
            case "crack": Crack(args); break;
            case "connect": Connect(args); break;
            case "analyze": Analyze(args); return false;
            default: Say($"{command} is not a valid command."); break;
        }
        return true;
    }

    void LS(params string[] args) {
        NodeDirectory targetDir = CurrentDirectory;
        if (args.Length != 0) {
            targetDir = CurrentDirectory.GetDirectory(args[0]);
            if (targetDir == null) { Say($"[color=red]Directory not found: {args[0]}[/color]"); return; }
        }

        if (targetDir.Childrens.Count == 0) { Say(""); return; }
        string output = "";
        foreach (NodeSystemItem item in targetDir.Childrens) {
            if (item is NodeDirectory) { output += $"[color=cyan]/{item.Name}[/color] "; } 
            else { output += $"[color=green]{item.Name}[/color] "; }
        }
        Say(output);
    }
    void CD(params string[] args) {
        if (args.Length == 0) { Say("[color=red]No directory provided.[/color]"); return; }
        NodeDirectory targetDir = CurrentDirectory.GetDirectory(args[0]);
        if (targetDir == null) { Say($"[color=red]Directory not found: {args[0]}[/color]"); return; }
        CurrentDirectory = targetDir;
    }
    void Pwd(params string[] args) {
        Say($"{CurrentDirectory.GetPath()}");
    }
    void Say(params string[] args) {
        if (args.Length == 0) return;

        string c = terminalOutputField.GetParsedText();
        if (c.Length >= MAX_HISTORY_CHAR_SIZE) {
            terminalOutputField.Clear();
            terminalOutputField.AppendText(c[^RESET_HISTORY_CHAR_SIZE..]);
        }

        bool noNewline = false, interpretEscapes = false;
        int argStart = 0;

        // Handle optional flags like -n, -e, or -ne
        if (args[0].StartsWith('-')) {
            noNewline = args[0].Contains('n');
            interpretEscapes = args[0].Contains('e');
            argStart = 1;
        }

        string text = string.Join(" ", args[argStart..]);
        if (interpretEscapes) text = Regex.Unescape(text); // handles \n, \t, etc.
        if (!noNewline) text += "\n";
        terminalOutputField.AppendText(text);
    }
    void Help(params string[] args) {
        Dictionary<string, string> parsedArgs = new() {
                { "-v", "" }
            }; ParseArgs(parsedArgs, args);
        string fileName = parsedArgs["-v"] == "-v" ? "helpVerbose.txt" : "helpShort.txt";
        FileAccess fileAccess = FileAccess.Open($"res://Utilities/TextFiles/CommandOutput/{fileName}", FileAccess.ModeFlags.Read);
        Say(fileAccess.GetAsText());
    }
    void Home(params string[] args) {
        CurrentNode = networkManager.network;
    }
    const int MAX_HISTORY_CHAR_SIZE = 65536, RESET_HISTORY_CHAR_SIZE = 16384;
    void Edit(params string[] args) {
        if (args.Length == 0) { Say("[color=red]No file name provided.[/color]"); return; }
        for (int i = 0; i < args.Length; i++) {
            NodeFile file = currentDirectory.GetFile(args[i]);
            if (file == null) {
                Say($"[color=red]File not found: {args[i]}[/color]");
                return;
            }
            overseer.textEditor.OpenNewFile(CurrentNode.HostName, file);
        }
    }
    void Newf(params string[] args) {
        if (args.Length == 0) { Say("[color=red]No file name provided.[/color]"); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < args.Length; i++) {
            string[] components = args[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string fileName = components[^1], parentPath = string.Join('/', components[..^1]);

            parentDirectory = CurrentDirectory.GetDirectory(parentPath);
            if (parentDirectory == null) { Say($"[color=red]Directory not found: {parentPath}[/color]"); return; }
            int result = parentDirectory.Add(new NodeFile(fileName));
            switch (result) {
                case 0: break;
                case 1: Say($"{args[i]} already exists. Skipped."); break;
                default: Say($"[color=red]{args[i]} creation failed. Error code: ${result}[/color]"); break;
            }
        }
    }
    void Scan(params string[] args) {
        StartProcess(.1, ScanCallback, args);
        void ScanCallback(params string[] args) {
            string L = " └─── ", M = " ├─── ", T = "      ", E = " │    ";
            Func<bool[], string> getTreePrefix = arr => string.Concat(arr.Select((b, i) => (i == arr.Length - 1 ? (b ? L : M) : (b ? T : E))));
            Func<bool[], string> getDescPrefix = arr => string.Concat(arr.Select((b, i) => (b ? T : E)));
            Dictionary<string, string> parsedArgs = new() {
                { "-d", "1" },
                { "-v", "" }
            }; ParseArgs(parsedArgs, args);
            if (!int.TryParse(parsedArgs["-d"], out int MAX_DEPTH)) { Say($"[color=red]Directory not found: {parsedArgs["-d"]}[/color]"); return; }

            Stack<(NetworkNode, int, bool[])> stack = new([(currentNode, 0, [])]);
            List<NetworkNode> visited = [];
            string output = "";
            while (stack.Count > 0) {
                (NetworkNode node, int depth, bool[] depthMarks) = stack.Pop();
                if (depth > MAX_DEPTH || visited.Contains(node)) continue;
                Dictionary<string, string> analyzeResult = node.Analyze();
                output += $"{getTreePrefix(depthMarks)}[color=cyan]{analyzeResult["IP"], -15}[/color] [color=green]{analyzeResult["hostName"]}[/color]\n";
                
                if (parsedArgs["-v"] == "-v") {
                    string defColorCode = analyzeResult["defLvl"] switch {
                        "0" or "1" or "2" => "blue",
                        "3" or "4" or "5" or "6" or "7" => "yellow",
                        "8" or "9" or "10" => "red",
                        _ => "cyan"
                    };
                    string secColorCode = analyzeResult["secType"] switch {
                        "NOSEC" or "LOSEC" => "red",
                         "MISEC" => "yellow",
                        "HISEC" or "MASEC" => "blue",
                        _ => "red"
                    };
                    string descPrefix = getDescPrefix(depthMarks) + ((depth == MAX_DEPTH ? 0 : ((node.ParentNode != null ? 0 : -1) + node.ChildNode.Count)) > 0 ? " │" : "  ");
                    output += $"{descPrefix}  Display Name:   [color=salmon]{analyzeResult["displayName"]}[/color];\n";
                    output += $"{descPrefix}  Node Type:      [color=purple]{analyzeResult["nodeType"]}[/color];\n";
                    output += $"{descPrefix}  Defense:        [color={defColorCode}]{analyzeResult["defLvl"]}[/color]  "
                            + $"Security: [color={secColorCode}]{analyzeResult["secType"]}[/color]\n";
                    output += $"{descPrefix}\n";
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
    void Clear(params string[] args) {
        terminalOutputField.Clear();
    }
    void MkDir(params string[] args) {
        if (args.Length == 0) { Say("[color=red]No directory name provided.[/color]"); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < args.Length; i++) {
            string[] components = args[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string dirName = components[^1], parentPath = string.Join('/', components[..^1]);

            parentDirectory = CurrentDirectory.GetDirectory(parentPath);
            if (parentDirectory == null) { Say($"[color=red]Directory not found: {parentPath}[/color]"); return; }
            int result = parentDirectory.Add(new NodeDirectory(dirName));
            switch (result) {
                case 0: break;
                case 1: Say($"{args[i]} already exists. Skipped."); break;
                default: Say($"[color=red]{args[i]} creation failed. Error code: ${result}[/color]"); break;
            }
        }
    }   
    void RmDir(params string[] args) {
        if (args.Length == 0) {
            Say("[color=red]No folder name provided.[/color]");
            return;
        }
        NodeDirectory parentDirectory;
        for (int i = 0; i < args.Length; i++) {
            parentDirectory = CurrentDirectory.GetDirectory(args[i]);
            if (parentDirectory == null) { Say($"[color=red]Directory not found: {args[0]}[/color]"); return; }
            if (parentDirectory.Parent == null) { Say($"[color=red]Can not remove root.[/color]"); return; }
            parentDirectory = parentDirectory.Parent;
            int result = parentDirectory.Remove(args[i]);
            switch(result) {
                case 0: break;
                case 1: Say($"[color=red]{args[i]} was not found.[/color]"); break;
                default: Say($"[color=red]{args[i]} removal failed. Error code: {result}[/color]"); break;
            }
        }
    }
    readonly string[] MSG_FORMAT = [
        "[color=red]Unknown error[/color]",
        "[color=red]Missing flag ${}[/color]", 
        "[color=red]Missing key ${}[/color]",
        "[color=red]Incorrect key ${}[/color]"
    ];
    void Crack(params string[] args) {
        Dictionary<string, string> parsedArgs = new(){ {"init", "" } }; ParseArgs(parsedArgs, args);
        NetworkNode node = CurrentNode;
        if (parsedArgs["init"] == "init") {
            int beginResult = node.BeginCrackNode(); 
            if (beginResult == 0) { Say("Cracking begin."); }
            else if (beginResult == 1) { Say("Cracking already in process."); }
        }
        (int result, string msg) = node.AttempCrackNode(parsedArgs);
        
        if (result != 0) { Say(string.Format(MSG_FORMAT[result], msg)); }
        else {
            Say($"[color=green]Node defense cracked.[/color] All security system [color=green]disabled[/color].");
            Say($"Run [color=cyan]analyze[/color] for all new information.");
        }
    }
    void Connect(params string[] args) {
        if (args.Length == 0) { Say("[color=red]No hostname provided.[/color]"); return; }
        if (networkManager.DNS.TryGetValue(args[0], out NetworkNode value)) { CurrentNode = value; return; }
        if (CurrentNode.ParentNode != null && CurrentNode.ParentNode.HostName == args[0] || 
            (CurrentNode.ParentNode is HoneypotNode && (CurrentNode.ParentNode as HoneypotNode).fakeHostName == args[0])) 
            { CurrentNode = CurrentNode.ParentNode; return; }
        
        NetworkNode node = CurrentNode.ChildNode.FindLast(s => s.HostName == args[0]);
        NetworkNode fnode = CurrentNode.ChildNode.FindLast(s => s is HoneypotNode && (s as HoneypotNode).fakeHostName == args[0]);
        if (node == null && fnode == null) { Say($"[color=red]Host not found: {args[0]}[/color]"); return; }
        CurrentNode = (node ?? fnode);
    }
    void Analyze(params string[] args) {
        if (args.Length == 0) { args = [CurrentNode.HostName]; }
        if (CurrentNode.ChildNode.FindLast(s => s.HostName == args[0]) == null 
            && (CurrentNode.ParentNode != null && CurrentNode.ParentNode.HostName != args[0])
            && CurrentNode.HostName != args[0]) {
            Say($"[color=red]Host not found: {args[0]}[/color]");
            return;
        }
        StartProcess(.5 + GD.Randf() * .5, AnalyzeCallback, args);
        void AnalyzeCallback(params string[] args) {
            NetworkNode analyzeNode = null;
            if (CurrentNode.ChildNode.FindLast(s => s.HostName == args[0]) != null) analyzeNode = CurrentNode.ChildNode.FindLast(s => s.HostName == args[0]);
            else if (CurrentNode.ParentNode != null && CurrentNode.ParentNode.HostName == args[0]) analyzeNode = CurrentNode.ParentNode;
            else if (CurrentNode.HostName == args[0]) analyzeNode = CurrentNode;
            string defColorCode = analyzeNode.DefLvl switch {
                < 3 => "blue",
                < 8 => "yellow",
                < 10 => "red",
                _ => "red"
            };
            string secColorCode = analyzeNode.SecType switch {
                SecurityType.NOSEC or SecurityType.LOSEC => "red",
                SecurityType.MISEC => "yellow",
                SecurityType.HISEC or SecurityType.MASEC => "blue",
                _ => "red"
            };
            Say($"[color=orange]▶ Node Info[/color]");
            Say($"Host name:      [color=green]{analyzeNode.HostName}[/color]");
            Say($"IP address:     [color=cyan]{analyzeNode.IP}[/color]");
            Say($"Display name:   [color=orange]{analyzeNode.DisplayName}[/color]");

            Say($"[color=orange]▶ Classification[/color]");
            Say($"Node type:      [color=purple]{analyzeNode.NodeType}[/color]");
            Say($"Defense level:  [color={defColorCode}]{analyzeNode.DefLvl}[/color]");
            Say($"Security level: [color={secColorCode}]{analyzeNode.SecType}[/color]");

            //if (analyzeNode.CurrentOwner != networkManager.network) return; // Not yours yet, no more detail!

            Say($"[color=orange]▶ GC miner detail[/color]");
            Say($"Transfer batch: [color=cyan]{analyzeNode.HackFarm.HackLevel}[/color] [color=gray]({analyzeNode.HackFarm.CurHack:F2}GC)[/color]");
            Say($"Transfer speed: [color=cyan]{analyzeNode.HackFarm.TimeLevel}[/color] [color=gray]({analyzeNode.HackFarm.CurTime:F2}s)[/color]");
            Say($"Mining speed:   [color=cyan]{analyzeNode.HackFarm.GrowLevel}[/color] [color=gray]({analyzeNode.HackFarm.CurHack:F2}GC/s)[/color]");

            if (analyzeNode is FactionNode) {
                Say($"[color=orange]▶ Faction detail[/color]");
                Say($"Name:           [color=brown]{(analyzeNode as FactionNode).Faction.Name}[/color]");
                Say($"Description:    [color=yellow]{(analyzeNode as FactionNode).Faction.Desc}[/color]");
            }
            if (analyzeNode is BusinessNode) {
                Say($"[color=orange]▶ Business detail[/color]");
                Say($"Stock:          [color=purple]{(analyzeNode as BusinessNode).Stock.Name}[/color]");
                Say($"Value:          [color=cyan]{(analyzeNode as BusinessNode).Stock.Price}[/color]");
            }
            if (analyzeNode is CorpNode) {
                Say($"[color=orange]▶ Faction detail[/color]");
                Say($"Name:           [color=brown]{(analyzeNode as CorpNode).Faction.Name}[/color]");
                Say($"Description:    [color=yellow]{(analyzeNode as CorpNode).Faction.Desc}[/color]");
                Say($"[color=orange]▶ Business detail[/color]");
                Say($"Stock:          [color=purple]{(analyzeNode as CorpNode).Stock.Name}[/color]");
                Say($"Value:          [color=cyan]{(analyzeNode as CorpNode).Stock.Price}[/color]");
            }
        }
    }
    void ParseArgs(Dictionary<string, string> defaultArg, params string[] args) {
        for (int i = 0; i < args.Length; i++) {
            if (defaultArg.ContainsKey(args[i])) {
                if (defaultArg[args[i]] == "") defaultArg[args[i]] = args[i];
                else if (i < args.Length - 1) defaultArg[args[i]] = args[i + 1];
            } else if (args[i][0] == '-') {
                if (i < args.Length - 1) defaultArg[args[i]] = args[i + 1];
                else defaultArg[args[i]] = args[i];
            }
        }
    }
    void SetCommandPrompt() { terminalCommandPrompt.Text = $"[color=purple]{networkManager.UserName}[/color]@[color=green]{CurrentNode.HostName}[/color]:{CurrentDirectory.GetPath()}>"; }
    string EscapeBBCode(string code) { return code.Replace("[", "[lb]"); }
}
