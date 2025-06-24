using Godot;
using System;
using System.Collections.Generic;
public static partial class ShellCore {
    public static event Action<string, string> LSrunCMD;
    static void LS(Dictionary<string, string> farg, string[] parg) {
        NodeDirectory targetDir = CurrDir;
        if (parg.Length != 0) {
            targetDir = CurrDir.GetDir(parg[0]);
            if (targetDir == null) { Say("-r", $"Directory not found: {Util.Format(parg[0], StrSty.DIR)}"); return; }
        }

        if (targetDir.Childrens.Count == 0) { Say(""); return; }
        string output = "";
        foreach (NodeSystemItem item in targetDir.Childrens) {
            output += ($"{Util.Format(((item is NodeDirectory) ? "./" : "") + item.Name, (item is NodeDirectory) ? StrSty.DIR : StrSty.FILE)}" + new string(' ', 5)).PadRight(16);
        }
        Say(output);
        LSrunCMD?.Invoke(parg.Length > 0 ? parg[0] : "", targetDir.GetPath()); 
    }
    public static event Action<string, string, CError> CDrunCMD;
    static void CD(Dictionary<string, string> farg, string[] parg) {
        if (parg.Length == 0) { Say("-r", "No directory provided."); CDrunCMD.Invoke(parg[0], CurrDir.GetPath(), CError.MISSING); return; }
        NodeDirectory targetDir = CurrDir.GetDir(parg[0]);
        if (targetDir == null) { Say("-r", $"Directory not found: {Util.Format(parg[0], StrSty.DIR)}"); CDrunCMD.Invoke(parg[0], CurrDir.GetPath(), CError.NOT_FOUND); return; }
        CurrDir = targetDir;
        CDrunCMD?.Invoke(parg[0], CurrDir.GetPath(), CError.OK);
    }
    public static event Action<string, string, CError> CATrunCMD;
    static void Cat(Dictionary<string, string> farg, string[] parg) {
        if (parg.Length == 0) { Say("-r", "No file specified."); CATrunCMD?.Invoke("", "", CError.MISSING); return; }
        string fileName = parg[0];
        NodeFile file = CurrDir.GetFile(fileName);
        if (file == null) { Say("-r", $"File `{fileName}` not found."); CATrunCMD?.Invoke(fileName, "", CError.NOT_FOUND); return; }
        Say(file.Content);
        CATrunCMD?.Invoke(fileName, file.GetPath(), CError.OK);
    }
    static void Pwd(Dictionary<string, string> farg, string[] parg) {
        Say($"{Util.Format(CurrDir.GetPath(), StrSty.DIR)}");
    }
    static void RmF(Dictionary<string, string> farg, string[] parg) {
        if (parg.Length == 0) { Say("-r", $"No file name provided."); return; }
        for (int i = 0; i < parg.Length; i++) {
            CError result = CurrDir.RemoveFile(parg[i]);
            switch (result) {
                case CError.OK: break;
                case CError.NOT_FOUND: Say("-r", $"{Util.Format(parg[i], StrSty.FILE)} was not found."); break;
                case CError.INVALID: Say("-r", $"Unexpected error when making {Util.Format(parg[i], StrSty.FILE)}. Game bug found and can be reported"); break;
                case CError.UNKNOWN: Say("-r", $"{Util.Format(parg[i], StrSty.FILE)} can't be removed. Game bug found and can be reported."); break;
                default: Say("-r", $"{Util.Format(parg[i], StrSty.FILE)} removal failed. Error code: {result}"); break;
            }
        }
    }
    public static event Action<string, string, CError> MKFrunCMD;
    static void MkF(Dictionary<string, string> farg, string[] parg) {
        if (parg.Length == 0) { Say("-r", $"No file name provided."); return; }
        for (int i = 0; i < parg.Length; i++) {
            CError result = CurrDir.AddFile(parg[i]);
            switch (result) {
                case CError.OK: break;
                case CError.NOT_FOUND: Say("-r", $"Parent directory of {Util.Format(parg[i], StrSty.FILE)} was not found."); break;
                case CError.INVALID: Say("-r", $"Unexpected error when making {Util.Format(parg[i], StrSty.FILE)}. Game bug found and can be reported."); break;
                case CError.DUPLICATE: Say($"{Util.Format(parg[i], StrSty.FILE)} already exists. Skipped."); break;
                default: Say("-r", $"{Util.Format(parg[i], StrSty.FILE)} creation failed. Error code: ${result}"); break;
            }
            MKFrunCMD?.Invoke(parg[i], (CurrDir.GetFile(parg[i])?.GetPath()) ?? "", result);
        }
    }
    public static event Action<string, string, CError> EDITrunCMD;
    static void Edit(Dictionary<string, string> farg, string[] parg) {
        if (parg.Length == 0) { Say("-r", "No file name provided."); return; }
        int fileOpened = 0;
        for (int i = 0; i < parg.Length; i++) {
            NodeFile file = CurrDir.GetFile(parg[i]);
            if (file == null) {
                Say("-r", $"File not found: {Util.Format(parg[i], StrSty.FILE)}");
                EDITrunCMD?.Invoke(parg[i], "", CError.NOT_FOUND);
            } else { overseer.textEditor.OpenNewFile(CurrNode.HostName, file); ++fileOpened; EDITrunCMD?.Invoke(parg[i], CurrDir.GetFile(parg[i]).GetPath(), CError.OK); }
        }
        Say($"{fileOpened} file(s) opened. See the editor.");
    }
    static void MkDir(Dictionary<string, string> farg, string[] parg) {
        if (parg.Length == 0) { Say("-r", $"No directory name provided."); return; }
        for (int i = 0; i < parg.Length; i++) {
            CError result = CurrDir.AddDir(parg[i]);
            switch (result) {
                case CError.OK: break;
                case CError.NOT_FOUND: Say("-r", $"Parent directory of {Util.Format(parg[i], StrSty.DIR)} was not found."); break;
                case CError.INVALID: Say("-r", $"Unexpected error when making {Util.Format(parg[i], StrSty.DIR)}. Game bug found and can be reported"); break;
                case CError.DUPLICATE: Say($"{Util.Format(parg[i], StrSty.DIR)} already exists. Skipped."); break;
                default: Say("-r", $"{Util.Format(parg[i], StrSty.DIR)} creation failed. Error code: ${result}"); break;
            }
        }
    }
    static void RmDir(Dictionary<string, string> farg, string[] parg) {
        if (parg.Length == 0) {
            Say("-r", "No directory name provided.");
            return;
        }
        for (int i = 0; i < parg.Length; i++) {
            CError result = CurrDir.RemoveDir(parg[i]);
            switch (result) {
                case CError.OK: break;
                case CError.NOT_FOUND: Say("-r", $"{Util.Format(parg[i], StrSty.DIR)} was not found."); break;
                case CError.INVALID: Say("-r", $"{Util.Format(parg[i], StrSty.DIR)} has no parent directory, which is invalid."); break;
                case CError.UNKNOWN: Say("-r", $"{Util.Format(parg[i], StrSty.DIR)} can't be removed. Game bug found and can be reported."); break;
                default: Say("-r", $"{Util.Format(parg[i], StrSty.DIR)} removal failed. Error code: {result}"); break;
            }
        }
    }
}