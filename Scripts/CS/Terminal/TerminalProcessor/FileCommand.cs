using Godot;
using System.Collections.Generic;
public static partial class ShellCore {
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
    static void RmF(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No file name provided."); return; }
        for (int i = 0; i < positionalArgs.Length; i++) {
            CError result = CurrDir.RemoveFile(positionalArgs[i]);
            switch (result) {
                case CError.OK: break;
                case CError.NOT_FOUND: Say("-r", $"{Util.Format(positionalArgs[i], StrType.FILE)} was not found."); break;
                case CError.INVALID: Say("-r", $"Unexpected error when making {Util.Format(positionalArgs[i], StrType.FILE)}. Game bug found and can be reported"); break;
                case CError.UNKNOWN: Say("-r", $"{Util.Format(positionalArgs[i], StrType.FILE)} can't be removed. Game bug found and can be reported."); break;
                default: Say("-r", $"{Util.Format(positionalArgs[i], StrType.FILE)} removal failed. Error code: {result}"); break;
            }
        }
    }
    static void MkF(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No file name provided."); return; }
        for (int i = 0; i < positionalArgs.Length; i++) {
            CError result = CurrDir.AddFile(positionalArgs[i]);
            switch (result) {
                case CError.OK: break;
                case CError.NOT_FOUND: Say("-r", $"Parent directory of {Util.Format(positionalArgs[i], StrType.FILE)} was not found."); break;
                case CError.INVALID: Say("-r", $"Unexpected error when making {Util.Format(positionalArgs[i], StrType.FILE)}. Game bug found and can be reported."); break;
                case CError.DUPLICATE: Say($"{Util.Format(positionalArgs[i], StrType.FILE)} already exists. Skipped."); break;
                default: Say("-r", $"{Util.Format(positionalArgs[i], StrType.FILE)} creation failed. Error code: ${result}"); break;
            }
        }
    }
    static void Edit(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", "No file name provided."); return; }
        int fileOpened = 0;
        for (int i = 0; i < positionalArgs.Length; i++) {
            NodeFile file = CurrDir.GetFile(positionalArgs[i]);
            if (file == null) {
                Say("-r", $"File not found: {Util.Format(positionalArgs[i], StrType.FILE)}");
            } else { overseer.textEditor.OpenNewFile(CurrNode.HostName, file); ++fileOpened; }
        }
        Say($"{fileOpened} file(s) opened. See the editor.");
    }
    static void MkDir(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No directory name provided."); return; }
        for (int i = 0; i < positionalArgs.Length; i++) {
            CError result = CurrDir.AddDir(positionalArgs[i]);
            switch (result) {
                case CError.OK: break;
                case CError.NOT_FOUND: Say("-r", $"Parent directory of {Util.Format(positionalArgs[i], StrType.DIR)} was not found."); break;
                case CError.INVALID: Say("-r", $"Unexpected error when making {Util.Format(positionalArgs[i], StrType.DIR)}. Game bug found and can be reported"); break;
                case CError.DUPLICATE: Say($"{Util.Format(positionalArgs[i], StrType.DIR)} already exists. Skipped."); break;
                default: Say("-r", $"{Util.Format(positionalArgs[i], StrType.DIR)} creation failed. Error code: ${result}"); break;
            }
        }
    }
    static void RmDir(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) {
            Say("-r", "-No directory name provided.");
            return;
        }
        for (int i = 0; i < positionalArgs.Length; i++) {
            CError result = CurrDir.RemoveDir(positionalArgs[i]);
            switch (result) {
                case CError.OK: break;
                case CError.NOT_FOUND: Say("-r", $"{Util.Format(positionalArgs[i], StrType.DIR)} was not found."); break;
                case CError.INVALID: Say("-r", $"{Util.Format(positionalArgs[i], StrType.DIR)} has no parent directory, which is invalid."); break;
                case CError.UNKNOWN: Say("-r", $"{Util.Format(positionalArgs[i], StrType.DIR)} can't be removed. Game bug found and can be reported."); break;
                default: Say("-r", $"{Util.Format(positionalArgs[i], StrType.DIR)} removal failed. Error code: {result}"); break;
            }
        }
    }
}