using MoonSharp.Interpreter;
using System.Collections.Generic;

public class CrackModule {
    public int Begin() {
        int result = ShellCore.BeginFlare();
        return result;
    }

    public object[][] AttackNode(Dictionary<string, string> flagKeyPairs) {
        var result = ShellCore.Attack(flagKeyPairs);
        var output = new List<object[]>(result.Length);

        foreach (var (err, s1, s2, s3) in result) {
            output.Add(new object[]
            { (int)err,  // or err.ToString()
            s1, s2, s3 });
        }
        return output.ToArray();
    }
    public void End() {
        ShellCore.EndFlare();
    }
    public enum BeginResult {
        Success = 0,
        InProgress = 1,
    }
}