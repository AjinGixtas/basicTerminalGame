using Godot;

public class P90 : Lock {
    public override string[] Flag => ["--p90"];
    public override int Cost => 1;
    public override int MinLvl => 7;

    readonly string[] intPool = ["2","3","5","7","11","13","17","19","23","29","31","37","41","43","47","53","59","61","67","71","73","79","83","89"];

    public P90() {
        name = "P90";
        clue = "Only the sentinels stand tall—their mind is themselves and their origin. Find the true guards among them.";
        help = $"{Flag} [int]";
        inp = "";
    }

    public override void Intialize() {
        ans = [$"{intPool[GD.RandRange(0, 7)]}"];
        base.Intialize();
    }
}
