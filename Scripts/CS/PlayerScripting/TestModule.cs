using System;
using MoonSharp.Interpreter;
public class TestModule {
    public void Say() {
        TerminalProcessor.Say("Hello from TestModule!");
    }
}