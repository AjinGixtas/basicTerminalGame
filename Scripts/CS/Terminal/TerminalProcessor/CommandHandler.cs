using Godot;
using System;
using System.Collections.Generic;

public static partial class ShellCore {
    static int SubmitCommand(string newCMD) {
        const int MAX_HISTORY_CMD_SIZE = 64;
        if (IsShellBusy) return 1;
        if (cmdHistory.Count == 0 || (cmdHistory.Count > 0 && cmdHistory[^1] != newCMD)) {
            cmdHistory.Add(newCMD);
            while (cmdHistory.Count > MAX_HISTORY_CMD_SIZE) { cmdHistory.RemoveAt(0); }
            _commandHistoryIndex = cmdHistory.Count;
        }
        string[] commands = Util.SplitCommands(newCMD);
        Say("-n", $"{terminalCommandPrompt.Text.Replace("\r", "").Replace("\n", "")}{Util.Format(Util.EscapeBBCode(newCMD), StrType.CMD_FUL)}");
        queuedAction = ExecuteCommands;
        queuedCommands = commands;
        StartProcess(GD.Randf()*1.0+.5);
        return 0;
        static void ExecuteCommands(string[] cmd) {
            for (int i = 0; i < cmd.Length; i++) {
                string[] components = Util.TokenizeCommand(cmd[i]);
                if (components.Length == 0) continue;
                if (!ProcessCommand(components[0], components[1..])) break;
            }
        }
    }
    public static event Action<string, Dictionary<string, string>, string[]> OnCommandProcessed; // Event to notify when a command is processed
    static bool ProcessCommand(string cmd, string[] args) {
        cmd = cmd.ToLower();
        (Dictionary<string, string> farg, string[] parg) = ParseArgs(args);
        switch (cmd) {
            case "ls": LS(farg, parg); break; // Content of folder
            case "cd": CD(farg, parg); break; // Navigate folder
            case "mkf": MkF(farg, parg); break; // Make file
            case "rmf": RmF(farg, parg); break; // Remove file
            case "pwd": Pwd(farg, parg); break; // List current folder path
            case "run": Run(farg, parg); break; // Run a script file
            case "cat": Cat(farg, parg); break; // Output file content to terminal
            case "say" or "echo": Say(farg, parg); break; // Output to terminal
            case "help": Help(farg, parg); break; // List how commands work
            case "home": Home(farg, parg); break; // Go to the player's node
            case "edit" or "nano" or "vim": Edit(farg, parg); break; // Open a file for edit
            case "scan" or "nmap": Scan(farg, parg); break; // Scan neighbouring node
            case "farm": Farm(farg, parg); break; // Interact with a node's HackFarm (GCminer)
            case "link": Link(farg, parg); break; // Connect to sector(s)
            case "stats": Stats(farg, parg); break;
            case "clear": Clear(farg, parg); break; // Clear all text on the terminal
            case "mkdir": MkDir(farg, parg); break; // Make folder
            case "rmdir": RmDir(farg, parg); break; // Remove folder
            case "sector": Sector(farg, parg); break; // List out sectors
            case "karaxe": Karaxe(farg, parg); break; // Interact with the rush hacking system
            case "unlink": Unlink(farg, parg); break; // Disconnect from sector(s)
            case "inspect": Inspect(farg, parg); break; // Doesn't do anything yet
            case "connect": Connect(farg, parg); break; // Connect to node
            case "analyze": Analyze(farg, parg); break; // Give data about a node
            case "setname": SetName(farg, parg); break; // Set name of user and node

            case "xyzzy": Say("Nothing happens"); break; // Classic Easter egg command

            case "regenerate": NetworkManager.RegenerateDriftSector(); break; // Regenerate the drift sector
            case "seecolor": SeeColor(farg, parg); break;
            case "genstub": GenStub(farg, parg); break; // Generate a Lua stub for a class

            default: Say("-r", $"{cmd} is not a valid command."); break;
        }
        OnCommandProcessed?.Invoke(cmd, farg, parg); // Notify subscribers about the command being processed
        return true;
    }
    static (Dictionary<string, string>, string[]) ParseArgs(string[] args) {
        Dictionary<string, string> parsedArgs = [];
        List<string> positionalArgs = [];
        for (int i = 0; i < args.Length; ++i) {
            if (args[i].StartsWith("--")) {
                if (i + 1 < args.Length && !args[i + 1].StartsWith('-')) { parsedArgs[args[i]] = args[i + 1]; ++i; } else parsedArgs[args[i]] = "";
                continue;
            }
            if (args[i].StartsWith('-')) {
                if (args[i].Length == 2) {
                    if (i + 1 < args.Length && !args[i + 1].StartsWith('-')) parsedArgs[args[i]] = args[i + 1];
                    else parsedArgs[args[i]] = "";
                    ++i;
                } else for (int j = 1; j < args[i].Length; ++j) { parsedArgs[$"-{args[i][j]}"] = ""; }
            } else { positionalArgs.Add(args[i]); }
        }
        return (parsedArgs, positionalArgs.ToArray());
    }
}