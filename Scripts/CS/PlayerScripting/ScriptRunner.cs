using System;
using MoonSharp.Interpreter;
using System.IO;
using Godot;

public static class ScriptRunner {

	static ScriptRunner() {
		UserData.RegisterType<TestModule>();
		UserData.RegisterType<NetworkModule>();
		UserData.RegisterType<MainModule>();
		UserData.RegisterType<HackFarmModule>();
        UserData.RegisterType<FileModule>();
		UserData.RegisterType<CError>();
	}

	public static void RunPlayerScript(string scriptContent) {

		var script = new MoonSharp.Interpreter.Script();
		script.Globals["TestModule"] = new TestModule();
		script.Globals["NetworkModule"] = new NetworkModule();
		script.Globals["MainModule"] = new MainModule();
		script.Globals["MinerModule"] = new HackFarmModule();
        script.Globals["FileModule"] = new FileModule();
		script.Globals["CError"] = new CError();

        script.Globals["print"] = DynValue.NewCallback((context, args) => { 
			TerminalProcessor.Say(Util.Format("Use ax:Say() instead of print()", StrType.WARNING));
			return DynValue.Nil;
		});
		try {
			script.DoString(scriptContent);
		} catch (ScriptRuntimeException ex) {
			TerminalProcessor.Say($"Script error: {ex}");
		}
	}

	public static void RunPlayerScriptFromFile(string filePath) {
		NodeFile file = TerminalProcessor.CurrDir.GetFile(filePath);
		if (file == null) {
			TerminalProcessor.Say("-r", $"Script file not found: {Util.Format(filePath, StrType.FILE)}");
			return;
		}
        RunPlayerScript(file.Content);
	}
}
