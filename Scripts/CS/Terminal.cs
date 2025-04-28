using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Terminal : VSplitContainer {
    RichTextLabel terminalOutputField; Label terminalCommandPrompt; LineEdit terminalCommandField; Timer processTimer, updateProcessGraphicTimer;
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
        terminalOutputField = GetNode<RichTextLabel>("TerminalOutputArea");
        terminalCommandField = GetNode<LineEdit>("TerminalCommandLine/CommandField");
        terminalCommandPrompt = GetNode<Label>("TerminalCommandLine/CommandPrompt");
        processTimer = GetNode<Timer>("TimersContainer/ProcessTimer");
        updateProcessGraphicTimer = GetNode<Timer>("TimersContainer/UpdateProcessGraphicTimer");
        networkManager = GetNode<NetworkManager>("NetworkManager");
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
        Echo("-n", $"Start");
        processTimer.Start();
        updateProcessGraphicTimer.Start();
    }
    static readonly string[] SpinnerChars = ["|", "/", "-", "\\"];
    const int BarSize = 50; int tick = 0, progress = 0;
    public void UpdateProcessingGraphic() {
        if (!isProcessing) return;

        int t = GD.RandRange(0, 1+(int)Math.Floor(150.0 / processTimer.WaitTime * updateProcessGraphicTimer.WaitTime));
        progress = Mathf.Clamp(progress + t, 0, 99); tick = (tick + 1) % SpinnerChars.Length;

        //int filledBar = Mathf.FloorToInt(progress / 100.0 * BarSize);
        Echo("-n", $"...{progress}%");
        updateProcessGraphicTimer.Start();
    }
    public void ProcessFinished() {
        isProcessing = false; terminalCommandField.Editable = true; terminalCommandField.Edit();
        tick = (tick + 1) % SpinnerChars.Length;
        Echo($"...Done!");
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
        Echo($"{terminalCommandPrompt.Text}{newText}");
        for (int i = 0; i < commands.Length; i++) {
            string[] components = commands[i].Split([' '], StringSplitOptions.RemoveEmptyEntries);
            if (!ProcessCommand(components[0], components[1..])) {
                break;
            }
        }
    }
    bool ProcessCommand(string command, string[] args) {
        command = command.ToLower();
        switch (command) {
            case "ls": LS(args); break;
            case "cd": CD(args); break;
            case "pwd": Pwd(args); break;
            case "home": Home(args); break;
            case "echo": Echo(args); break;
            case "scan": Scan(args); return false;
            case "clear": Clear(args); break;
            case "mkdir": MkDir(args); break;
            case "rmdir": RmDir(args); break;
            case "crack": Crack(args); break;
            case "connect": Connect(args); break;
            case "analyze": Analyze(args); return false;
            default: Echo($"{command} is not a valid command."); break;
        }
        return true;
    }

    void LS(params string[] args) {
        NodeDirectory targetDir = CurrentDirectory;
        if (args.Length != 0) {
            targetDir = CurrentDirectory.GetDirectory(args[0]);
            if (targetDir == null) { Echo($"[color=red]Directory not found: {args[0]}[/color]"); return; }
        }

        if (targetDir.Childrens.Count == 0) { Echo(""); return; }
        string output = "";
        foreach (NodeSystemItem item in targetDir.Childrens) {
            if (item is NodeDirectory) { output += $"[color=blue]/{item.Name}[/color] "; } 
            else { output += $"[color=cyan]{item.Name}[/color] "; }
        }
        Echo(output);
    }
    void CD(params string[] args) {
        if (args.Length == 0) { Echo("[color=red]No directory provided.[/color]"); return; }
        NodeDirectory targetDir = CurrentDirectory.GetDirectory(args[0]);
        if (targetDir == null) { Echo($"[color=red]Directory not found: {args[0]}[/color]"); return; }
        CurrentDirectory = targetDir;
    }
    void Pwd(params string[] args) {
        Echo($"{CurrentDirectory.GetPath()}");
    }
    void Home(params string[] args) {
        CurrentNode = networkManager.network;
    }
    const int MAX_HISTORY_CHAR_SIZE = 2048, RESET_HISTORY_CHAR_SIZE = 1024;
    void Echo(params string[] args) {
        if (args.Length == 0) { return; }
        string c = terminalOutputField.GetParsedText();
        if (c.Length >= MAX_HISTORY_CHAR_SIZE) {
            terminalOutputField.Clear(); terminalOutputField.AppendText(c[^RESET_HISTORY_CHAR_SIZE..]);
        }
        if (args[0] == "-n") terminalOutputField.AppendText(string.Join(" ", args[1..]));
        else terminalOutputField.AppendText($"{args.Join(" ")}\n");
    }
    void Scan(params string[] args) {
        StartProcess(.1, ScanCallback, args);
        void ScanCallback(params string[] args) {
            Dictionary<string, string> parsedArgs = new() {
                { "-d", "1" },
                { "-v", "" }
            }; ParseArgs(parsedArgs, args);
            if (!int.TryParse(parsedArgs["-d"], out int MAX_DEPTH)) { Echo($"[color=red]Directory not found: {parsedArgs["-d"]}[/color]"); return; }

            Stack<(NetworkNode, int)> stack = new([(currentNode, 0)]);
            List<NetworkNode> visited = [];
            string output = "";
            while (stack.Count > 0) {
                (NetworkNode node, int depth) = stack.Pop();
                if (depth > MAX_DEPTH || visited.Contains(node)) continue;
                    Dictionary<string, string> analyzeResult = node.Analyze();
                output += $"{new string('-', depth*5)}+ [color=cyan]{analyzeResult["IP"], -15}[/color] [color=green]{analyzeResult["hostName"]}[/color]\n";
                if (parsedArgs["-v"] == "-v") {
                    string defColorCode = analyzeResult["defLvl"] switch {
                        "0" => "white",
                        "1" or "2" or "3" => "cyan",
                        "4" or "5" or "6" => "blue",
                        "7" or "8" or "9" => "yellow",
                        "10" => "red",
                        _ => "white"
                    };
                    string secColorCode = analyzeResult["secType"] switch {
                        "NOSEC" => "white",
                        "LOSEC" => "cyan",
                        "MISEC" => "blue",
                        "HISEC" => "yellow",
                        _ => "red"
                    };
                    output += $"{new string(' ', depth * 5)}     Display Name:  {analyzeResult["displayName"]};\n";
                    output += $"{new string(' ', depth * 5)}     Node Type:     [color=purple]{analyzeResult["nodeType"]}[/color];\n";
                    output += $"{new string(' ', depth * 5)}     Defense Level: [color={defColorCode}]{analyzeResult["defLvl"]}[/color]; Security Type: [color={secColorCode}]{analyzeResult["secType"]}[/color]\n";
                }
                visited.Add(node);
                if (node.ParentNode != null) { stack.Push((node.ParentNode, depth + 1)); }
                foreach(NetworkNode child in node.ChildNode) { stack.Push((child, depth + 1)); }
            }
            Echo(output);
        }
    }
    void Clear(params string[] args) {
        terminalOutputField.Clear();
    }
    void MkDir(params string[] args) {
        if (args.Length == 0) { Echo("[color=red]No directory name provided.[/color]"); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < args.Length; i++) {
            string[] components = args[i].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string dirName = components[^1], parentPath = string.Join('/', components[..^1]);

            parentDirectory = CurrentDirectory.GetDirectory(parentPath);
            if (parentDirectory == null) { Echo($"[color=red]Directory not found: {parentPath}[/color]"); return; }
            int result = parentDirectory.Add(new NodeDirectory(dirName));
            switch (result) {
                case 0: break;
                case 1: Echo($"{args[i]} already exists. Skipped."); break;
                default: Echo($"[color=red]{args[i]} creation failed. Error code: ${result}[/color]"); break;
            }
        }
    }   
    void RmDir(params string[] args) {
        if (args.Length == 0) {
            Echo("[color=red]No folder name provided.[/color]");
            return;
        }
        NodeDirectory parentDirectory;
        for (int i = 0; i < args.Length; i++) {
            parentDirectory = CurrentDirectory.GetDirectory(args[i]);
            if (parentDirectory == null) { Echo($"[color=red]Directory not found: {args[0]}[/color]"); return; }
            if (parentDirectory.Parent == null) { Echo($"[color=red]Can not remove root.[/color]"); return; }
            parentDirectory = parentDirectory.Parent;
            int result = parentDirectory.Remove(args[i]);
            switch(result) {
                case 0: break;
                case 1: Echo($"[color=red]{args[i]} was not found.[/color]"); break;
                default: Echo($"[color=red]{args[i]} removal failed. Error code: {result}[/color]"); break;
            }
        }
    }
    void Crack(params string[] args) {
        Dictionary<string, string> parsedArgs = new(){ {"init", "" } }; ParseArgs(parsedArgs, args);
        NetworkNode node = CurrentNode;
        if (parsedArgs["init"] == "init") {
            GD.Print(node.LockSystem == null);
            int beginResult = node.LockSystem.BeginCrack(); 
            if (beginResult == 0) { Echo("Cracking begin."); }
            else if (beginResult == 1) { Echo("Cracking already in process."); }
        }
        Tuple<bool, string> crackResult = node.LockSystem.CrackAttempt(parsedArgs);
        Echo($"{crackResult.Item2}");
    }
    void Connect(params string[] args) {
        if (args.Length == 0) { Echo("[color=red]No hostname provided.[/color]"); return; }
        if (CurrentNode.ParentNode != null && CurrentNode.ParentNode.HostName == args[0]) { CurrentNode = CurrentNode.ParentNode; return; }
        if (networkManager.DNS.ContainsKey(args[0])) { CurrentNode = networkManager.DNS[args[0]]; return; }
        if (CurrentNode.ChildNode.FindLast(s => s.HostName == args[0]) == null) { Echo($"[color=red]Host not found: {args[0]}[/color]"); return; }
        CurrentNode = CurrentNode.ChildNode.FindLast(s => s.HostName == args[0]);
    }
    void Analyze(params string[] args) {
        if (args.Length == 0) { args = [CurrentNode.HostName]; }
        if (CurrentNode.ChildNode.FindLast(s => s.HostName == args[0]) == null 
            && (CurrentNode.ParentNode != null && CurrentNode.ParentNode.HostName != args[0])
            && CurrentNode.HostName != args[0]) {
            Echo($"[color=red]Host not found: {args[0]}[/color]");
            return;
        }
        StartProcess(.5 + GD.Randf() * .5, AnalyzeCallback, args);
        void AnalyzeCallback(params string[] args) {
            NetworkNode analyzeNode = null;
            if (CurrentNode.ChildNode.FindLast(s => s.HostName == args[0]) != null) analyzeNode = CurrentNode.ChildNode.FindLast(s => s.HostName == args[0]);
            else if (CurrentNode.ParentNode != null && CurrentNode.ParentNode.HostName == args[0]) analyzeNode = CurrentNode.ParentNode;
            else if (CurrentNode.HostName == args[0]) analyzeNode = CurrentNode;
            Echo($"Display name:   {analyzeNode.DisplayName}");
            Echo($"Host name:      {analyzeNode.HostName}");
            Echo($"Host type:      {analyzeNode.NodeType}");
            Echo($"Defense level:  {analyzeNode.DefLvl}");
            Echo($"Security level: {analyzeNode.SecType}");
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
    void SetCommandPrompt() { terminalCommandPrompt.Text = $"{networkManager.UserName}@{CurrentNode.HostName}:{CurrentDirectory.GetPath()}>"; }
    string EscapeBBCode(string code) { return code.Replace("[", "[lb]"); }
}
