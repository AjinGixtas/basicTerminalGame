using System;
public static partial class ShellCore {
    public static event Action<string, string, CError> RUNrunCMD;
    /// <summary>
    /// Run a script file in the current directory.
    /// </summary>
    /// <param name="farg">Flag arguments, include `--` and `-` in the keys.</param>
    /// <param name="parg">Positional arguments, follow the order they are provided in by the player.</param>
    static void Run(System.Collections.Generic.Dictionary<string, string> farg, string[] parg) {
        if (parg.Length == 0) {
            ShellCore.Say("y", "Usage: run <script_file>");
            RUNrunCMD?.Invoke("", "", CError.MISSING);
            return;
        }
        string scriptPath = parg[0];
        NodeFile file = CurrDir.GetFile(scriptPath);
        if (file == null) {
            ShellCore.Say("r", $"Script file '{scriptPath}' does not exist.");
            RUNrunCMD?.Invoke(scriptPath, "", CError.NOT_FOUND);
            return;
        }
        try {
            string scriptContent = file.Content;
            ScriptRunner.RunPlayerScript(scriptContent, farg, parg[1..]);
            RUNrunCMD?.Invoke(scriptPath, file.GetPath(), CError.OK);
        } catch (System.Exception ex) {
            ShellCore.Say("r", $"Error running script '{scriptPath}': {ex.Message}");
            RUNrunCMD?.Invoke(scriptPath, file.GetPath(), CError.UNKNOWN);
        }
    }
}