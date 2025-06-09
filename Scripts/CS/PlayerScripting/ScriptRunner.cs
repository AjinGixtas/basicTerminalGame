using System;
using MoonSharp.Interpreter;
using System.IO;
using Godot;

public static class ScriptRunner {

	static ScriptRunner() {
		UserData.RegisterType<TestModule>();
        UserData.RegisterType<NetworkModule>();
		UserData.RegisterType<CrackModule>();
        UserData.RegisterType<HackFarmModule>();
        UserData.RegisterType<FileModule>();
		UserData.RegisterType<CError>();
        UserData.RegisterType<MainModule>();
    }

    public static void RunPlayerScript(string scriptContent) {

		var script = new MoonSharp.Interpreter.Script();
		script.Globals["tet"] = new TestModule();
		script.Globals["net"] = new NetworkModule();
		script.Globals["kar"] = new CrackModule();
		script.Globals["bot"] = new HackFarmModule();
        script.Globals["fio"] = new FileModule();
		script.Globals["cer"] = new CError();
        script.Globals["ax"] = new MainModule();

        script.Globals["print"] = DynValue.NewCallback((context, args) => { 
			ShellCore.Say(Util.Format("Use ax:Say() instead of print()", StrType.WARNING));
			return DynValue.Nil;
		});
		try {
			script.DoString(scriptContent);
		} catch (ScriptRuntimeException ex) {
			ShellCore.Say($"Script error: {ex}");
		}
	}

	public static void RunPlayerScriptFromFile(string filePath) {
		NodeFile file = ShellCore.CurrDir.GetFile(filePath);
		if (file == null) {
			ShellCore.Say("-r", $"Script file not found: {Util.Format(filePath, StrType.FILE)}");
			return;
		}
        RunPlayerScript(file.Content);
	}
}
