using System.Collections.Generic;

public class CrackModule {
    public BeginResult Begin() {
        int result = TerminalProcessor.BeginFlare();
        return (BeginResult)result;
    }
    public void AttackNode(Dictionary<string,string> flagKeyPairs) {
        TerminalProcessor.Attack(flagKeyPairs);
    }
    public void End() {
        TerminalProcessor.EndFlare();
    }
    public enum BeginResult {
        Success = 0,
        InProgress = 1,
    }
}