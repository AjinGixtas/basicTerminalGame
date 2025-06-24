using Godot;
using System.Linq;

public class C1 : Lock {
    public override string[] Flag => ["--c1"];
    public override int Cost => 1;
    public override int MinLvl => 3;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[][] colorGroups = [
        ["white", "black"],
        ["red", "green", "blue"],
        ["cyan", "magenta", "yellow"]
    ];

    public C1() {
        name = "C1";
        clue = "Three stand before you. Expel the outcast.";
        help = $"{Flag} [string]";
    }

    public override void Intialize() {
        string[] groupPile = Util.Shuffle(colorGroups[GD.RandRange(0, 2)].ToArray());
        string[] colorPile = Util.Shuffle(colors.ToArray());
        string[] group = [groupPile[0], groupPile[1], "null"];

        for (int i = 0; i < colorPile.Length; ++i) {
            if (groupPile.Contains(colorPile[i])) continue;
            group[2] = colorPile[i];
            ans = [group[2]];
            break;
        }

        group = Util.Shuffle(group.ToArray());
        inp = group.Join(" ");
        base.Intialize();
    }
    public override bool UnlockAttempt(string input, int index) {
        return base.UnlockAttempt(input.ToLower(), index);
    }
}
