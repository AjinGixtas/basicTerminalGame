using Godot;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public static partial class TerminalProcessor {
    static int SubmitCommand(string newCommand) {
        const int MAX_HISTORY_CMD_SIZE = 64;
        if (IsProcessing) return 1;
        if (commandHistory.Count == 0 || (commandHistory.Count > 0 && commandHistory[^1] != newCommand)) {
            commandHistory.Add(newCommand);
            while (commandHistory.Count > MAX_HISTORY_CMD_SIZE) { commandHistory.RemoveAt(0); }
            _commandHistoryIndex = commandHistory.Count;
        }
        string[] commands = SplitCommands(newCommand);
        Say("-n", $"{terminalCommandPrompt.Text.Replace("\r", "").Replace("\n", "")}{Util.Format(Util.EscapeBBCode(newCommand), StrType.CMD_FUL)}");
        queuedAction = ExecuteCommands;
        queuedCommands = commands;
        StartProcess(Mathf.Clamp(.1 * Mathf.Pow(1.02, newCommand.Length), .1, 1.0));
        return 0;
        static void ExecuteCommands(string[] commands) {
            for (int i = 0; i < commands.Length; i++) {
                string[] components = Tokenize(commands[i]);
                if (components.Length == 0) continue;
                if (!ProcessCommand(components[0], components[1..])) break;
            }
        }
        static string[] Tokenize(string input) {
            var args = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;
            char quoteChar = '\0';
            bool escape = false;

            foreach (char c in input) {
                if (escape) {
                    current.Append(c);
                    escape = false;
                } else if (c == '\\') {
                    escape = true;
                } else if (inQuotes) {
                    if (c == quoteChar) {
                        inQuotes = false;
                    } else {
                        current.Append(c);
                    }
                } else {
                    if (char.IsWhiteSpace(c)) {
                        if (current.Length > 0) {
                            args.Add(current.ToString());
                            current.Clear();
                        }
                    } else if (c == '"' || c == '\'') {
                        inQuotes = true;
                        quoteChar = c;
                    } else {
                        current.Append(c);
                    }
                }
            }

            if (current.Length > 0)
                args.Add(current.ToString());

            return [..args];
        }
        static string[] SplitCommands(string input) {
            var commands = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;
            char quoteChar = '\0';
            bool escape = false;

            foreach (char c in input) {
                if (escape) {
                    current.Append(c);
                    escape = false;
                } else if (c == '\\') {
                    escape = true;
                } else if (inQuotes) {
                    if (c == quoteChar) {
                        inQuotes = false;
                    }
                    current.Append(c);
                } else {
                    if (c == '"' || c == '\'') {
                        inQuotes = true;
                        quoteChar = c;
                        current.Append(c);
                    } else if (c == ';') {
                        // semicolon outside quotes means split here
                        var cmd = current.ToString().Trim();
                        if (cmd.Length > 0)
                            commands.Add(cmd);
                        current.Clear();
                    } else {
                        current.Append(c);
                    }
                }
            }

            var lastCmd = current.ToString().Trim();
            if (lastCmd.Length > 0)
                commands.Add(lastCmd);

            return [..commands];
        }
    }
    static bool ProcessCommand(string command, string[] args) {
        command = command.ToLower();
        (Dictionary<string, string> parsedArgs, string[] positionalArgs) = ParseArgs(args);
        switch (command) {
            case "ls": LS(parsedArgs, positionalArgs); break; // Content of folder
            case "cd": CD(parsedArgs, positionalArgs); break; // Navigate folder
            case "mkf": MkF(parsedArgs, positionalArgs); break; // Make file
            case "rmf": RmF(parsedArgs, positionalArgs); break; // Remove file
            case "pwd": Pwd(parsedArgs, positionalArgs); break; // List current folder path
            case "say" or "echo": Say(parsedArgs, positionalArgs); break; // Output to terminal
            case "help": Help(parsedArgs, positionalArgs); break; // List how commands work
            case "home": Home(parsedArgs, positionalArgs); break; // Go to the player's node
            case "edit" or "nano" or "vim": Edit(parsedArgs, positionalArgs); break; // Open a file for edit
            case "scan" or "nmap": Scan(parsedArgs, positionalArgs); break; // Scan neighbouring node
            case "farm": Farm(parsedArgs, positionalArgs); break; // Interact with a node's HackFarm (GCminer)
            case "link": Link(parsedArgs, positionalArgs); break; // Connect to sector(s)
            case "stats": Stats(parsedArgs, positionalArgs); break;
            case "clear": Clear(parsedArgs, positionalArgs); break; // Clear all text on the terminal
            case "mkdir": MkDir(parsedArgs, positionalArgs); break; // Make folder
            case "rmdir": RmDir(parsedArgs, positionalArgs); break; // Remove folder
            case "sector": Sector(parsedArgs, positionalArgs); break; // List out sectors
            case "karaxe": Karaxe(parsedArgs, positionalArgs); break; // Interact with the rush hacking system
            case "unlink": Unlink(parsedArgs, positionalArgs); break; // Disconnect from sector(s)
            case "inspect": Inspect(parsedArgs, positionalArgs); break; // Doesn't do anything yet
            case "connect": Connect(parsedArgs, positionalArgs); break; // Connect to node
            case "analyze": Analyze(parsedArgs, positionalArgs); break; // Give data about a node
            case "setusername": SetUsername(parsedArgs, positionalArgs); break; // Set the player's username

            case "regenerate": NetworkManager.RegenerateDriftSector(); break; // Regenerate the drift sector
            case "seecolor": SeeColor(parsedArgs, positionalArgs); break;
            default: Say("-r", $"{command} is not a valid command."); break;
        }
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