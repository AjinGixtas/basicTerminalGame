using Godot;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public static partial class ShellCore {
    // It gets 2 since this one is REALLY close to standard, but since it's also called independently a lot, allow for seperate shorthand flag is way better.
    static void Say(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        const int MAX_HISTORY_CHAR_SIZE = 65536, RESET_HISTORY_CHAR_SIZE = 16384;
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
    public static void Say(params string[] args) {
        const int MAX_HISTORY_CHAR_SIZE = 65536, RESET_HISTORY_CHAR_SIZE = 16384;
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
        
        Say($"Resouces: {Util.Format($"{PlayerDataManager.MineInv[0]}", StrType.MINERAL, "0")}");
    }
    static void Clear(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        terminalOutputField.Clear();
    }
    static void Inspect(Dictionary<string, string> parsedArgs, string[] positionalArgs) {

    }
    static void SetUsername(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No username provided."); return; }
        if (positionalArgs[0].Length > 20) { Say("-r", $"Username too long. Max length is 20 characters."); return; }
        PlayerDataManager.Username = positionalArgs[0];
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
            output += $"[color={Util.CC(cc)}]{new string(' ', index * 3)} ███ {name}[/color]\n";
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
}