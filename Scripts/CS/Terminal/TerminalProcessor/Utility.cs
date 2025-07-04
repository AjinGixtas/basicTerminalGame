using Godot;
using System.Collections.Generic;
using System.Linq;

public static partial class ShellCore {
    static void SetCommandPrompt() {
        if (TCMD_Prompt == null || CurrNode == null || CurrDir == null) return;
        TCMD_Prompt.Text = $"{Util.Format(PlayerDataManager.Username, StrSty.USERNAME)}@{Util.Format(CurrNode.HostName, StrSty.HOSTNAME)}:{Util.Format(CurrDir.GetPath(), StrSty.DIR)}>";
    }
    const double TIME_TIL_NEXT_LINE = .1; 
    static double timeLeft = TIME_TIL_NEXT_LINE;
    static double curLineFraction = 0.0;
    static void ShowMoreChars(double delta) {
        const int INSTA_FILL_MARGIN = 5;
        // Get current number of visible characters
        int curChar = TOUT_Field.VisibleCharacters;
        int allChar = TOUT_Field.GetTotalCharacterCount();
        if (curChar >= allChar) return;

        // Get full text and split into lines
        string allText = TOUT_Field.GetParsedText();
        string[] lines = allText.Split('\n');

        // Get total and current visible lines
        int allLines = lines.Length;
        
        double lineDelta = 0;
        if (allLines - curLineFraction > INSTA_FILL_MARGIN) 
            lineDelta += (allLines - curLineFraction) * 5.0 * delta; 
        else lineDelta = allLines - curLineFraction;
        curLineFraction += lineDelta;

        // Update chars
        int charsToShow = GetCharacterIndexAtLine(lines, (int)Mathf.Floor(curLineFraction));
        TOUT_Field.VisibleCharacters = Mathf.Clamp(charsToShow, 0, allText.Length);
    }
    static int CountVisibleLines(string[] lines, int visibleCharCount) {
        int sum = 0, i = -1;
        for (int j = 0; j < lines.Length; j++) {
            sum += lines[j].Length + 1;
            if (sum > visibleCharCount) break;
            i = j;
        }
        return i + 1;
    }
    static int GetCharacterIndexAtLine(string[] lines, int lineIndex) {
        return lines.Take(lineIndex + 1).Sum(s => s.Length + 1);
    }
    static string GetLastLinesUnderLimit(string text, int maxChars) {
        var lines = text;
        var newline = '\n';
        List<int> lineBreaks = new();

        // Step 1: Find all newline positions
        for (int i = 0; i < lines.Length; i++) {
            if (lines[i] == newline)
                lineBreaks.Add(i);
        }

        int totalChars = 0;
        int startLineIndex = lineBreaks.Count;

        // Step 2: Walk backwards through line breaks
        for (int i = lineBreaks.Count - 1; i >= 0; i--) {
            int lineStart = (i == 0) ? 0 : lineBreaks[i - 1] + 1;
            int lineEnd = lineBreaks[i] + 1; // include '\n'
            int lineLength = lineEnd - lineStart;

            if (totalChars + lineLength > maxChars)
                break;

            totalChars += lineLength;
            startLineIndex = i;
        }

        // Step 3: Slice the original span
        int startChar = (startLineIndex == 0) ? 0 : lineBreaks[startLineIndex - 1] + 1;
        return text[startChar..];
    }
}