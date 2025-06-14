using Godot;
public static partial class ShellCore {
    static void Run(System.Collections.Generic.Dictionary<string, string> parsedArgs, string[] positionalArgs) {
        if (positionalArgs.Length == 0) {
            GD.Print("Usage: run <script_file>");
            return;
        }
        string scriptPath = positionalArgs[0];
        NodeFile file = CurrDir.GetFile(scriptPath);
        if (file == null) {
            GD.Print($"Script file '{scriptPath}' does not exist.");
            return;
        }
        try {
            string scriptContent = file.Content;
            ScriptRunner.RunPlayerScript(scriptContent, parsedArgs, positionalArgs);
        } catch (System.Exception ex) {
            GD.PrintErr($"Error running script '{scriptPath}': {ex.Message}");
        }
    }
}