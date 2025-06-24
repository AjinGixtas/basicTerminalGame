using Godot;
using System.Linq;

public class C3 : Lock {
    public override string[] Flag => ["--c3"];
    public override int Cost => 1;
    public override int MinLvl => 3;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[] groupNames = ["bw", "rgb", "cmy"];
    readonly string[][] colorGroups = [
        ["white", "black"],
        ["red", "green", "blue"],
        ["cyan", "magenta", "yellow"]
    ];

    public C3() {
        name = "C3";
        clue = "Three hues, two kindred. Find their family.";
        help = $"{Flag} [string]";
    }

    public override void Intialize() {
        int index = GD.RandRange(0, 2);
        ans = [groupNames[index]];

        string[] groupPile = Util.Shuffle(colorGroups[index].ToArray());
        string[] colorPile = Util.Shuffle(colors.ToArray());
        string[] group = [groupPile[0], groupPile[1], "null"];

        for (int i = 0; i < colorPile.Length; ++i) {
            if (groupPile.Contains(colorPile[i])) continue;
            group[2] = colorPile[i];
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
