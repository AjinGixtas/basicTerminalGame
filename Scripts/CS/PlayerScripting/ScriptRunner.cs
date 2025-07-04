using Jint;
using MoonSharp.Interpreter;
using System;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using System.Linq;
public static class ScriptRunner {

    static ScriptRunner() {
        UserData.RegisterType<TestModule>();
        UserData.RegisterType<NetworkModule>();
        UserData.RegisterType<CrackModule>();
        UserData.RegisterType<HackFarmModule>();
        UserData.RegisterType<FileModule>();
        UserData.RegisterType<CError>();
        UserData.RegisterType<MainModule>();
        UserData.RegisterType<CraftModule>();
    }
    public static void RunPlayerScript(ScriptLanguage lang, string scriptContent, Dictionary<string, string> farg, string[] parg) {
        switch (lang) {
            case ScriptLanguage.Lua:
                RunLua(scriptContent, farg, parg);
                break;
            case ScriptLanguage.Javascript:
                RunJS(scriptContent, farg, parg);
                break;
            case ScriptLanguage.Python:
                RunPy(scriptContent, farg, parg);
                break;
            case ScriptLanguage.CSharp:
                RunCS(scriptContent, farg, parg);
                break;
            default:
                ShellCore.Say("-r", "Unknown script language.");
                break;
        }
    }

    public static void RunPlayerScript(string scriptContent, Dictionary<string, string> farg, string[] parg) {
        throw new Exception("DEPERACATED");
        return;
        var script = new MoonSharp.Interpreter.Script();
        // script.Globals["tet"] = new TestModule();
        script.Globals["NetworkModule"] = new NetworkModule();
        script.Globals["KaraxeModule"] = new CrackModule();
        script.Globals["BotNetModule"] = new HackFarmModule();
        script.Globals["FileModule"] = new FileModule();
        script.Globals["CError"] = new CError();
        script.Globals["SecurityType"] = new SecLvl();
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
        throw new Exception("NOT USED");
        return;
        NodeFile file = ShellCore.CurrDir.GetFile(filePath);
        if (file == null) {
            ShellCore.Say("-r", $"Script file not found: {Util.Format(filePath, StrSty.FILE)}");
            return;
        }
        RunPlayerScript(file.Content, [], []);
    }
    private static Dictionary<string, object> GetCoreModules() {
        return new Dictionary<string, object> {
            { "NetworkModule", new NetworkModule() },
            { "KaraxeModule", new CrackModule() },
            { "BotNetModule", new HackFarmModule() },
            { "FileModule", new FileModule() },
            { "CError", new CError() },
            { "SecLvl", new SecLvl() },
            { "MainModule", new MainModule() },
            { "CraftModule", new CraftModule() },
        };
    }

        private static void RunLua(string scriptContent, Dictionary<string, string> farg, string[] parg) {
            var script = new MoonSharp.Interpreter.Script();

            // Inject modules
            foreach (var kvp in GetCoreModules())
                script.Globals[kvp.Key] = kvp.Value;

            // Inject args
            Table flagArgsTable = new(script);
            foreach (var kvp in farg)
                flagArgsTable[kvp.Key] = string.IsNullOrEmpty(kvp.Value) ? DynValue.True : DynValue.NewString(kvp.Value);

            Table posArgsTable = new(script);
            for (int i = 0; i < parg.Length; i++)
                posArgsTable[i] = DynValue.NewString(parg[i]);

            script.Globals["arf"] = flagArgsTable;
            script.Globals["arp"] = posArgsTable;

            // Override print
            script.Globals["print"] = DynValue.NewCallback((context, args) => {
                ShellCore.Say(Util.Format("Use mai:Say() (or the fitting obj name in the script context) instead of print()", StrSty.WARNING));
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
    private static void RunJS(string scriptContent, Dictionary<string, string> farg, string[] parg) {
        var engine = new Jint.Engine(cfg => cfg
            .AllowClr()
            .LimitRecursion(64)
            .TimeoutInterval(TimeSpan.FromSeconds(2))
        );

        // Inject modules
        foreach (var kvp in GetCoreModules())
            engine.SetValue(kvp.Key, kvp.Value);

        // Inject args
        engine.SetValue("arf", farg);
        engine.SetValue("arp", parg);

        // Inject enums
        engine.SetValue("CError", Enum.GetValues(typeof(CError))
            .Cast<CError>()
            .ToDictionary(e => e.ToString(), e => (int)e)
        );

        engine.SetValue("SecLvl", Enum.GetValues(typeof(SecLvl))
            .Cast<SecLvl>()
            .ToDictionary(e => e.ToString(), e => (int)e)
        );

        // Override print
        engine.SetValue("print", new Action<string>((msg) => {
            ShellCore.Say("-r", "Use mai:Say() (or the fitting obj name) instead of print()");
        }));

        try {
            engine.Execute(scriptContent);
        } catch (Exception ex) {
            ShellCore.Say("-r", $"JS Error: {ex.Message}");
        }
    }

    private static void RunPy(string scriptContent, Dictionary<string, string> farg, string[] parg) {
        ShellCore.Say("-r", "Python support has been put off indefinitely"); return;
        var engine = IronPython.Hosting.Python.CreateEngine();
        var scope = engine.CreateScope();

        foreach (var kvp in GetCoreModules())
            scope.SetVariable(kvp.Key, kvp.Value);

        scope.SetVariable("arf", farg);
        scope.SetVariable("arp", parg);
        scope.SetVariable("print", new Action<string>((msg) => {
            ShellCore.Say("-r", "Use MainModule.Say() (or the fitting obj name) instead of print()");
        }));

        try {
            engine.Execute(scriptContent, scope);
        } catch (Exception ex) {
            var eo = engine.GetService<ExceptionOperations>(ex);

            string errorText = eo.FormatException(ex);
            ShellCore.Say("Exception:\n" + errorText);
        }
    }
    private static async void RunCS(string scriptContent, Dictionary<string, string> farg, string[] parg) {
        ShellCore.Say("-r", "C# support has been put off indefinitely"); return;
        var globals = new Dictionary<string, object>(GetCoreModules()) {
            { "arf", farg },
            { "arp", parg },
            { "print", new Action<string>((msg) => {
                ShellCore.Say("-r", "Use mai:Say() instead of print()");
            })}
        };

        var globalsWrapper = new ScriptGlobals() {
            farg = farg,
            parg = parg,
            NetworkModule = new(),
            CrackModule = new(),
            HackFarmModule = new(),
            FileModule = new(),
            CError = new(),
            SecurityType = new(),
            MainModule = new(),
        };

        try {
            await CSharpScript.EvaluateAsync(
                scriptContent,
                Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                    .WithImports("System", "System.Collections.Generic").AddReferences(typeof(NetworkModule).Assembly, typeof(CrackModule).Assembly, typeof(HackFarmModule).Assembly, typeof(FileModule).Assembly, typeof(CError).Assembly, typeof(SecLvl).Assembly, typeof(MainModule).Assembly).AddImports("BasicTerminalGame"),
                globalsWrapper
            );
        } catch (CompilationErrorException ex) {
            foreach (var diagnostic in ex.Diagnostics) {
                if (diagnostic.Location != Location.None && diagnostic.Location.IsInSource) {
                    var lineSpan = diagnostic.Location.GetLineSpan();
                    var line = lineSpan.StartLinePosition.Line + 1;
                    var column = lineSpan.StartLinePosition.Character + 1;
                    ShellCore.Say("-r", $"C# Compilation Error at line {line}, col {column}: {diagnostic.GetMessage()}");
                } else {
                    ShellCore.Say("-r", "C# Compilation Error: " + diagnostic.GetMessage());
                }
            }
        } catch (Exception ex) {
            var trace = new System.Diagnostics.StackTrace(ex, true);
            var frame = trace.GetFrames()?.FirstOrDefault(f => f.GetFileLineNumber() != 0);
            if (frame != null) {
                var line = frame.GetFileLineNumber();
                ShellCore.Say("-r", $"C# Runtime Error at line {line}: {ex.Message}");
            } else {
                ShellCore.Say("-r", $"C# Runtime Error: {ex.Message}");
            }
        }

    }

    // Helper class for C# globals
    public class ScriptGlobals {
        public Dictionary<string, string> farg;
        public string[] parg;
        public NetworkModule NetworkModule;
        public CrackModule CrackModule;
        public HackFarmModule HackFarmModule;
        public FileModule FileModule;
        public CError CError;
        public SecLvl SecurityType;
        public MainModule MainModule;
    }
    
    // Script lang
    public enum ScriptLanguage {
        Lua,
        Javascript,
        Python,
        CSharp
    }
    public static ScriptLanguage DetectLanguageFromExtension(string filePath) {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch {
            ".lua" => ScriptLanguage.Lua,
            ".js" => ScriptLanguage.Javascript,
            ".py" => ScriptLanguage.Python,
            ".cs" => ScriptLanguage.CSharp,
            _ => ScriptLanguage.Lua, // Default
        };
    }
}
