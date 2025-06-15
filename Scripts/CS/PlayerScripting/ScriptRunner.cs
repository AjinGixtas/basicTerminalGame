using Godot;
using MoonSharp.Interpreter;
using System;

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

	public static void RunPlayerScript(string scriptContent, System.Collections.Generic.Dictionary<string, string> flagArgs, string[] posArgs) {

		var script = new MoonSharp.Interpreter.Script();
		script.Globals["tet"] = new TestModule();
		script.Globals["net"] = new NetworkModule();
		script.Globals["kar"] = new CrackModule();
		script.Globals["bot"] = new HackFarmModule();
		script.Globals["fio"] = new FileModule();
		script.Globals["CError"] = new CError();
		script.Globals["SecurityType"] = new SecurityType();
		script.Globals["ax"] = new MainModule();

		Table flagArgsTable = new(script);
		foreach (var kvp in flagArgs) flagArgsTable[kvp.Key] = string.IsNullOrEmpty(kvp.Value) ? true : DynValue.NewString(kvp.Value);
		Table posArgsTable = new(script);
		for (int i = 0; i < posArgs.Length; i++) posArgsTable[i] = DynValue.NewString(posArgs[i]);

		script.Globals["arf"] = flagArgsTable;
        script.Globals["arp"] = posArgsTable;

        script.Globals["print"] = DynValue.NewCallback((context, args) => {
			ShellCore.Say(Util.Format("Use ax:Say() instead of print()", StrType.WARNING));
			return DynValue.Nil;
		});
		try {
			script.DoString(scriptContent);
		} catch (SyntaxErrorException ex) {
            ShellCore.Say("-r", ex.DecoratedMessage);
        } catch (ScriptRuntimeException ex) {
            ShellCore.Say("-r", ex.DecoratedMessage);
        } catch (InterpreterException ex) {
            ShellCore.Say("-r", ex.DecoratedMessage);
        } catch (Exception ex) {
            ShellCore.Say("-r", "Internal error, please report this: " + ex.Message);
        }
    }

	public static void RunPlayerScriptFromFile(string filePath) {
		NodeFile file = ShellCore.CurrDir.GetFile(filePath);
		if (file == null) {
			ShellCore.Say("-r", $"Script file not found: {Util.Format(filePath, StrType.FILE)}");
			return;
		}
		RunPlayerScript(file.Content, [], []);
	}
}