using Godot;
using System.Collections.Generic;
public static partial class TerminalProcessor {
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
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            string[] components = StringExtensions.Split(positionalArgs[i], "/", false);
            string fileName = components[^1], parentPath = string.Join('/', components[..^1]);
            parentDirectory = CurrDir.GetDirectory(parentPath);
            if (parentDirectory == null) { Say("-r", $"File not found: {parentPath}"); return; }
            int result = parentDirectory.RemoveFile(positionalArgs[i]);
            switch (result) {
                case 0: break;
                case 1: Say($"{positionalArgs[i]} was not found."); break;
                default: Say("-r", $"{Util.Format(positionalArgs[i], StrType.FILE)} removal failed. Error code: {result}"); break;
            }
        }
    }
    static void MkF(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) { Say("-r", $"No file name provided."); return; }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            string[] components = StringExtensions.Split(positionalArgs[i], "/", false);
            string fileName = components[^1], parentPath = string.Join('/', components[..^1]);

            parentDirectory = CurrDir.GetDirectory(parentPath);
            if (parentDirectory == null) { Say("-r", $"Directory not found: {Util.Format(parentPath, StrType.DIR)}"); return; }
            int result = parentDirectory.AddFile(fileName);
            switch (result) {
                case 0: break;
                case 1: Say($"{Util.Format(positionalArgs[i], StrType.FILE)} already exists. Skipped."); break;
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
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            string[] components = StringExtensions.Split(positionalArgs[i], "/", false);
            string dirName = components[^1], parentPath = string.Join('/', components[..^1]);

            parentDirectory = CurrDir.GetDirectory(parentPath);
            if (parentDirectory == null) { Say("-r", $"Directory not found: {Util.Format(parentPath, StrType.DIR)}"); return; }
            int result = parentDirectory.AddDir(dirName);
            switch (result) {
                case 0: break;
                case 1: Say($"{Util.Format(parentPath, StrType.DIR)} already exists. Skipped."); break;
                default: Say("-r", $"{Util.Format(parentPath, StrType.DIR)} creation failed. Error code: ${result}"); break;
            }
        }
    }
    static void RmDir(Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) {
            Say("-r", "-No directory name provided.");
            return;
        }
        NodeDirectory parentDirectory;
        for (int i = 0; i < positionalArgs.Length; i++) {
            parentDirectory = CurrDir.GetDirectory(positionalArgs[i]);
            if (parentDirectory == null) { Say("-r", $"Directory not found: {Util.Format(positionalArgs[0], StrType.DIR)}"); return; }
            if (parentDirectory.Parent == null) { Say("-r", $"Can not remove root."); return; }
            parentDirectory = parentDirectory.Parent;
            int result = parentDirectory.RemoveDir(positionalArgs[i]);
            switch (result) {
                case 0: break;
                case 1: Say("-r", $"{Util.Format(positionalArgs[i], StrType.DIR)} was not found."); break;
                default: Say("-r", $"{Util.Format(positionalArgs[i], StrType.DIR)} removal failed. Error code: {result}"); break;
            }
        }
    }
}