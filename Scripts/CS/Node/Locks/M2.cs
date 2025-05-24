using Godot;

public class M2 : Lock {
    public override string[] Flag => ["--m2"];
    public override int Cost => 1;
    public override int MinLvl => 3;

    public M2() {
        name = "M2";
        clue = "Seek the last square before the overflow.";
        help = $"{Flag} [int]";
    }

    public override void Intialize() {
        int upperBound = GD.RandRange(2, 256);
        int root = Mathf.Sqrt(upperBound) == Mathf.Floor(Mathf.Sqrt(upperBound)) ? (int)Mathf.Sqrt(upperBound) - 1 : (int)Mathf.Floor(Mathf.Sqrt(upperBound));
        ans = [$"{root * root}"];
        inp = $"{upperBound}";
        base.Intialize();
    }
}
