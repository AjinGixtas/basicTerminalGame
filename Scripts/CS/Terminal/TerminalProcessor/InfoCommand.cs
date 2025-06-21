using Godot;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public static partial class ShellCore {
    public static void Cat(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", "No file specified."); return; }
        string fileName = positionalArgs[0];
        NodeFile file = CurrDir.GetFile(fileName);
        if (file == null) { Say("-r", $"File `{fileName}` not found."); return; }
        Say(file.Content);
    }
    // It gets 2 since this one is REALLY close to standard, but since it's also called independently a lot, allow for seperate shorthand flag is way better.
    const int MAX_HISTORY_CHAR_SIZE = 65536, RESET_HISTORY_CHAR_SIZE = 16384;
    static void Say(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say(""); return; }
        string c = terminalOutputField.Text;
        if (c.Length >= MAX_HISTORY_CHAR_SIZE) {
            terminalOutputField.Clear();
            terminalOutputField.AppendText(GetLastLinesUnderLimit(c, RESET_HISTORY_CHAR_SIZE));
        }
        string text = string.Join(" ", positionalArgs);
        if (parsedArgs.ContainsKey("-e")) text = Regex.Unescape(text); // handles \n, \t, etc.
        if (!parsedArgs.ContainsKey("-b")) text = Util.EscapeBBCode(text);
        if (parsedArgs.ContainsKey("-t")) text = text.TrimEnd('\n', '\r');
        if (parsedArgs.ContainsKey("-l")) text = text.TrimStart('\n', '\r');
        if (!parsedArgs.ContainsKey("-n")) text += "\n";
        if (parsedArgs.ContainsKey("-r")) text = Util.Format(text, StrType.ERROR);
        if (parsedArgs.ContainsKey("-w")) text = Util.Format(text, StrType.WARNING);
        terminalOutputField.AppendText(text);
    }
    public static void SayM(string content) {
        if (terminalOutputField == null) return;
        string c = terminalOutputField.Text;
        if (c.Length >= MAX_HISTORY_CHAR_SIZE) {
            terminalOutputField.Clear();
            terminalOutputField.AppendText(GetLastLinesUnderLimit(c, RESET_HISTORY_CHAR_SIZE));
        }
        terminalOutputField.ScrollToLine(terminalOutputField.GetLineCount() - 1);
        terminalOutputField.AppendText(content);
    }
    public static void Say(params string[] args) {
        if (terminalOutputField == null) return;
        if (args.Length == 0) { Say(""); return; }

        string c = terminalOutputField.Text;
        if (c.Length >= MAX_HISTORY_CHAR_SIZE) {
            terminalOutputField.Clear();
            terminalOutputField.AppendText(GetLastLinesUnderLimit(c, RESET_HISTORY_CHAR_SIZE));
        }

        bool noNewline = false, interpretEscapes = false, escapeBBcode = false, makeRed = false, makeYellow = false;
        bool trimTrailingNewline = false, trimLeadingNewline = false;
        int argStart = 0;

        // Handle optional flags like -n, -e, -r, -t, -l
        if (args[0].StartsWith('-')) {
            string flags = args[0];
            noNewline = flags.Contains('n');
            interpretEscapes = flags.Contains('e');
            escapeBBcode = flags.Contains('b');
            makeRed = flags.Contains('r');
            makeYellow = flags.Contains('y');
            trimTrailingNewline = flags.Contains('t');
            trimLeadingNewline = flags.Contains('l');
            argStart = 1;
        }

        string text = string.Join(" ", args[argStart..]);
        if (interpretEscapes) text = Regex.Unescape(text); // handles \n, \t, etc.
        if (escapeBBcode) text = Util.EscapeBBCode(text);
        if (trimLeadingNewline) text = text.TrimStart('\n', '\r');
        if (trimTrailingNewline) text = text.TrimEnd('\n', '\r');
        if (!noNewline) text += "\n";
        if (makeRed) text = Util.Format(text, StrType.ERROR);
        if (makeYellow) text = Util.Format(text, StrType.WARNING);
        terminalOutputField.ScrollToLine(terminalOutputField.GetLineCount() - 1);
        terminalOutputField.AppendText(text);
    }
    static void Help(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        string fileName = parsedArgs.ContainsKey("-v") ? "helpVerbose.txt" : "helpShort.txt";
        FileAccess fileAccess = FileAccess.Open($"res://Utilities/TextFiles/CommandOutput/{fileName}", FileAccess.ModeFlags.Read);
        Say(fileAccess.GetAsText());
    }
    static void Stats(Dictionary<string, string> parsedArgs, string[] postionalArgs) {
        Say($"Username: {Util.Format(PlayerDataManager.Username, StrType.USERNAME)}");
        Say($"Balance:  {Util.Format($"{PlayerDataManager.GC_Cur}", StrType.MONEY)}");
        
        Say(
@$"Resouces: 
{Util.Format($"{PlayerDataManager.MineInv[0]}", StrType.T_MINERAL, "0")}
{Util.Format($"{PlayerDataManager.MineInv[1]}", StrType.T_MINERAL, "1")}
{Util.Format($"{PlayerDataManager.MineInv[2]}", StrType.T_MINERAL, "2")}
{Util.Format($"{PlayerDataManager.MineInv[3]}", StrType.T_MINERAL, "3")}
{Util.Format($"{PlayerDataManager.MineInv[4]}", StrType.T_MINERAL, "4")}
{Util.Format($"{PlayerDataManager.MineInv[5]}", StrType.T_MINERAL, "5")}
{Util.Format($"{PlayerDataManager.MineInv[6]}", StrType.T_MINERAL, "6")}
{Util.Format($"{PlayerDataManager.MineInv[7]}", StrType.T_MINERAL, "7")}
{Util.Format($"{PlayerDataManager.MineInv[8]}", StrType.T_MINERAL, "8")}
{Util.Format($"{PlayerDataManager.MineInv[9]}", StrType.T_MINERAL, "9")}");
    }
    static void Clear(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        Clear();
    }
    static void Inspect(Dictionary<string, string> parsedArgs, string[] positionalArgs) {

    }
    static void SetName(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        string username = Util.GetArg(parsedArgs, "-u", "--username");
        string hostname = Util.GetArg(parsedArgs, "-h", "--hostname");
        string displayName = Util.GetArg(parsedArgs, "-d", "--displayname");
        if (username != null) { 
            if (Regex.IsMatch(username, @"^[a-z_][a-z0-9_-]{0,31}$")) SetUsername(username);
            else Say("-r", "Invalid username. Must start with a letter or underscore, followed by letters, numbers, underscores, or hyphens, and be 1-32 characters long.");
        }
        if (hostname != null) {
            if (Regex.IsMatch(hostname, @"^(?!-)[a-zA-Z0-9-]{1,63}(?<!-)$")) NetworkManager.PlayerNode.HostName = hostname;
            else Say("-r", "Invalid hostname. Must be 1-63 characters long, start and end with a letter or number, and contain only letters, numbers, and hyphens.");
        }
        if (displayName != null) {
            if (Regex.IsMatch(displayName, @"^[^\x00-\x1F\x7F]{1,100}$")) NetworkManager.PlayerNode.DisplayName = displayName;
            else Say("-r", "Invalid display name. Must be 1-100 characters long and not contain control characters.");
        }
        SetCommandPrompt();
    }
    static void SeeColor(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        var colorNames = new (Cc, string)[] {
        (Cc._, "Black"),
        (Cc.w, "Dark Gray"),
        (Cc.W, "White"),
        (Cc.R, "Bright Red"),
        (Cc.G, "Bright Green"),
        (Cc.B, "Bright Blue"),
        (Cc.C, "Bright Cyan"),
        (Cc.M, "Bright Magenta"),
        (Cc.Y, "Bright Yellow"),
        (Cc.r, "Deep Red"),
        (Cc.g, "Deep Green"),
        (Cc.b, "Deep Blue"),
        (Cc.c, "Dark Teal"),
        (Cc.m, "Deep Magenta"),
        (Cc.y, "Olive Yellow"),
        (Cc.gB, "Aqua Blue"),
        (Cc.rB, "Violet"),
        (Cc.rG, "Lime Green"),
        (Cc.bG, "Mint Green"),
        (Cc.bR, "Hot Pink"),
        (Cc.gR, "Amber"),
        (Cc.LB, "Periwinkle"),
        (Cc.LG, "Light Green"),
        (Cc.LC, "Light Cyan"),
        (Cc.LR, "Light Coral"),
        (Cc.LM, "Light Magenta"),
        (Cc.LY, "Light Yellow"),
    };

        string output = "";
        int index = 0;
        foreach (var (cc, name) in colorNames) {
            output += $"[color={Util.CC(cc)}]{new string(' ', index * 3)} ███ {name} {(int)cc}[/color]\n";
            index++;
        }
        Say(output.TrimEnd('\n'));
    }
    static void GenStub(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", "No class name provided."); return; }
        string iFile = positionalArgs[0], oFile = positionalArgs[1];
        string csCode = FileAccess.Open($"{iFile}", FileAccess.ModeFlags.Read).GetAsText();
        if (csCode == null) { Say("-r", $"Class {iFile} not found."); return; }
        var clsData = APIxtractor.ParseClass(csCode);
        string luaStub = APIxtractor.GenerateLuaStub(clsData);
        FileAccess f = FileAccess.Open($"{oFile}", FileAccess.ModeFlags.Write);
        f.StoreString(luaStub);
        f.Flush();
        Say($"Generated Lua stub for class {iFile}");
    }

    public static void Clear() {
        terminalOutputField.Clear();
    }
    public static void SetUsername(string newUsername) {
        PlayerDataManager.Username = newUsername;
        SetCommandPrompt();
    }
}