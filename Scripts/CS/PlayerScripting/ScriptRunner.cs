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

	public static void RunPlayerScript(string scriptContent, System.Collections.Generic.Dictionary<string, string> farg, string[] parg) {

		var script = new MoonSharp.Interpreter.Script();
		// script.Globals["tet"] = new TestModule();
		script.Globals["NetworkModule"] = new NetworkModule();
		script.Globals["KaraxeModule"] = new CrackModule();
		script.Globals["BotNetModule"] = new HackFarmModule();
		script.Globals["FileModule"] = new FileModule();
		script.Globals["CError"] = new CError();
		script.Globals["SecurityType"] = new SecurityType();
		script.Globals["MainModule"] = new MainModule();

		Table flagArgsTable = new(script);
		foreach (var kvp in farg) flagArgsTable[kvp.Key] = string.IsNullOrEmpty(kvp.Value) ? true : DynValue.NewString(kvp.Value);
		Table posArgsTable = new(script);
		for (int i = 0; i < parg.Length; i++) posArgsTable[i] = DynValue.NewString(parg[i]);

		script.Globals["arf"] = flagArgsTable;
        script.Globals["arp"] = posArgsTable;

        script.Globals["print"] = DynValue.NewCallback((context, args) => {
			ShellCore.Say(Util.Format("Use ax:Say() instead of print()", StrSty.WARNING));
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
			ShellCore.Say("-r", $"Script file not found: {Util.Format(filePath, StrSty.FILE)}");
			return;
		}
		RunPlayerScript(file.Content, [], []);
	}
}