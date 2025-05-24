using Godot;

public class I16 : Lock {
    public override string[] Flag => ["--i16"];
    public override int Cost => 1;
    public override int MinLvl => 4;

    public I16() {
        name = "I16";
        clue = "The system flooded with noise. As we descent the axe upon each one, our time shorten.";
        help = $"{Flag} [int]";
        inp = "";
    }

    public override void Intialize() {
        ans = [$"{GD.RandRange(1, 16)}"];
        base.Intialize();
    }
}
