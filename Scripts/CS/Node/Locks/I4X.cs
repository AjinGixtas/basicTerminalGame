using Godot;

public class I4X : Lock {
    public override string[] Flag => ["--i4", "--2xtract"];
    public override int Cost => 1;
    public override int MinLvl => 1;

    readonly string[] intPool = ["1", "2", "3", "4"];
    readonly string[] actionPool = ["fl1p", "2bin", "der3f", "bl4nk"];
    public I4X() {
        name = "I5";
        clue = "Among the crowd will be knights and heroes. But only one dare rise to our blade.";
        help = $"{Flag[0]} [int] {Flag[1]} [string]";
        inp = "";
    }

    public override void Intialize() {
        ans = [intPool[GD.RandRange(0, 3)], actionPool[GD.RandRange(0, 3)]];
        base.Intialize();
    }
}