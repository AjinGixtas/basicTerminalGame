using Godot;
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
            { err, s1, s2, s3 });
        }
        object[][] fres= output.ToArray();
        for (int i = 0; i < fres.Length; ++i) {
            for (int j = 0; j < fres[i].Length; ++j) {
                GD.Print(fres[i][j]);
            }
        }
        return fres;
    }
    public void End() {
        ShellCore.EndFlare();
    }
    public enum BeginResult {
        Success = 0,
        InProgress = 1,
    }
}