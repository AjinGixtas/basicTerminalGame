using Godot;

public class C0 : Lock {
    public override string[] Flag => ["--c0"];
    public override int Cost => 1;
    public override int MinLvl => 1;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly (string, string)[] colorPairs = [("red", "cyan"), ("green", "magenta"), ("blue", "yellow"), ("white", "black")];

    public C0() {
        name = "C0";
        clue = "Name the opposition.";
        help = $"{Flag} [string]";
    }

    public override void Intialize() {
        int pairIndex = GD.RandRange(0, 3); bool swapped = GD.RandRange(0, 1) == 0;
        ans = [colorPairs[pairIndex].Item1];
        inp = colorPairs[pairIndex].Item2;
        if (swapped) {
            (colorPairs[pairIndex].Item2, colorPairs[pairIndex].Item1) = (colorPairs[pairIndex].Item1, colorPairs[pairIndex].Item2);
        }
        err = $"{inp}";
        base.Intialize();
    }
}
