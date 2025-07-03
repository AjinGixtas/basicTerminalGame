public class T3Puzzle : Lock {
    public override string[] Flag => ["--t2"];
    public override LocT LocT => LocT.T2;
    public T3Puzzle() {
        name = "T2P";
        clue = "The first step is always the hardest. But first; KÜNSTLICHE INTELLIGENZ!";
        help = $"{Flag} [int]";
        inp = "AI";
    }
    public override void Initialize() {
        ans = ["AI"];
        base.Initialize();
    }
}