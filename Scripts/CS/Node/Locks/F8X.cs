using Godot;

public class F8X : Lock {
    public override string[] Flag => ["--f8", "--3xtract"];
    public override int Cost => 1;
    public override int MinLvl => 3;
    readonly string[] intPool = ["1", "2", "3", "5", "8", "13", "21", "34"];
    readonly string[] actionPool = ["fl1p", "2bin", "der3f", "bl4nk"];
    public F8X() {
        name = "F8";
        clue = "In this world, growth follows memory. From the flicker to the fire storm, each step remembers the two before it.";
        help = $"{Flag[0]} [int] {Flag[1]} [string]";
        inp = "";
    }
    public override void Intialize() {
        ans = [intPool[GD.RandRange(0, 7)], actionPool[GD.RandRange(0, 3)]];
        base.Intialize();
    }
}
