using Godot;
using System.Linq;

public abstract class Lock {
    protected string clue = "<Empty>", err = "<Empty>", help = "<Empty>", inp = "<Empty>", name = "<Empty>";
    public abstract int Cost { get; }
    public abstract int MinLvl { get; }
    public virtual void Intialize() { GD.Print(Flag[0] + ' ' + ans.Join(" ") + ' '); }
    public virtual bool UnlockAttempt(string input, int index) { return input.Equals(ans[index]); }
    public abstract string[] Flag { get; }
    protected string[] ans;
    public string Name => name;
    public string Clue => clue;
    public string Help => help;
    public string Inp => inp;
    public string Err => err;
}
