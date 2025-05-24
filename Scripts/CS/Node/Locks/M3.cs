using Godot;

public class M3 : Lock {
    public override string[] Flag => ["--m3"];
    public override int Cost => 1;
    public override int MinLvl => 3;

    public M3() {
        name = "M3";
        clue = "Before comprehension dissolves, a final vessel holds the chaos—a perfect cube, find it.";
        help = $"{Flag} [int]";
    }

    public override void Intialize() {
        int upperBound = GD.RandRange(2, 512);
        int root = Mathf.Pow(upperBound, 1.0 / 3.0) == Mathf.Floor(Mathf.Pow(upperBound, 1.0 / 3.0)) ? (int)Mathf.Pow(upperBound, 1.0 / 3.0) + 1 : (int)Mathf.Ceil(Mathf.Pow(upperBound, 1.0 / 3.0));
        ans = [$"{root * root * root}"];
        inp = $"{upperBound}";
        base.Intialize();
    }
}