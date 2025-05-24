using Godot;

public class P16X : Lock {
    public override string[] Flag => ["--p16", "--4xtract"];
    public override int Cost => 1;
    public override int MinLvl => 6;

    readonly string[] intPool = ["2", "3", "5", "7", "11", "13", "17", "19", "23", "29", "31", "37", "41", "43", "47", "53"];
    readonly string[] actionPool = ["fl1p", "2bin", "der3f", "bl4nk"];
    public P16X() {
        name = "P16X";
        clue = "Gates only yield to those with indivisible will. Fight them all; the resolute will answer";
        help = $"{Flag[0]} [int] {Flag[1]} [string]";
        inp = "";
    }

    public override void Intialize() {
        ans = [$"{intPool[GD.RandRange(0, 15)]}", actionPool[GD.RandRange(0, 3)]];
        base.Intialize();
    }
}
