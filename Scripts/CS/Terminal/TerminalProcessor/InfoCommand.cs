using Godot;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public static partial class TerminalProcessor {
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
        Say($"Balance:  {Util.Format($"{PlayerDataManager.GC_PublicDisplay}", StrType.MONEY)}");
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
}