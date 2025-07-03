using Godot;
using System.Linq;

public abstract class Lock {
    protected string clue = "<Empty>", err = "<Empty>", help = "<Empty>", inp = "<Empty>", name = "<Empty>";
    public virtual int Cost => 1;
    public abstract LocT LocT { get; }
    public virtual void Initialize() { GD.Print(inp); GD.Print(string.Join(" ", Flag.Zip(ans, (a, b) => new[] { a, b }).SelectMany(x => x)) + " "); }
    public virtual bool[] UnlockAttempt(string[] keys) { 
        if (keys.Length != Flag.Length) { throw new System.Exception("This never supposes to happen"); }
        bool[] result = new bool[Flag.Length];
        for (int i = 0; i < Flag.Length; ++i) {
            result[i] = keys[i].Equals(ans[i]);
        }
        return result;
    }
    public abstract string[] Flag { get; }
    protected string[] ans;
    public string Name => name;
    public string Clue => clue;
    public string Help => help;
    public string Inp => inp;
    public string Err => err;
}
