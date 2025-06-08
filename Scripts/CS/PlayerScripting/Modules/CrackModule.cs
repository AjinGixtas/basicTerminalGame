using MoonSharp.Interpreter;
using System.Collections.Generic;

public class CrackModule {
    public int Begin() {
        int result = TerminalProcessor.BeginFlare();
        return result;
    }

    public List<object[]> AttackNode(Dictionary<string, string> flagKeyPairs) {
        var result = TerminalProcessor.Attack(flagKeyPairs);
        var output = new List<object[]>(result.Length);

        foreach (var (err, s1, s2, s3) in result) {
            output.Add(new object[]
            {
            (int)err,  // or err.ToString()
            s1,
            s2,
            s3
            });
        }
        return output;
    }
    public void End() {
        TerminalProcessor.EndFlare();
    }
    public enum BeginResult {
        Success = 0,
        InProgress = 1,
    }
}