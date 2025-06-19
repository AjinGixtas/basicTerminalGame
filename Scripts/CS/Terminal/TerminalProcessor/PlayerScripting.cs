using Godot;
public static partial class ShellCore {
    static void Run(System.Collections.Generic.Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) {
            ShellCore.Say("y", "Usage: run <script_file>");
            return;
        }
        string scriptPath = positionalArgs[0];
        NodeFile file = CurrDir.GetFile(scriptPath);
        if (file == null) {
            ShellCore.Say("r", $"Script file '{scriptPath}' does not exist.");
            return;
        }
        try {
            string scriptContent = file.Content;
            ScriptRunner.RunPlayerScript(scriptContent, parsedArgs, positionalArgs);
        } catch (System.Exception ex) {
            ShellCore.Say("r", $"Error running script '{scriptPath}': {ex.Message}");
        }
    }
}