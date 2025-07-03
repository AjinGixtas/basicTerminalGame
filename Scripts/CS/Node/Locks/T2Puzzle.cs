public class T2Puzzle : Lock {
    public override string[] Flag => ["--t2"];
    public override LocT LocT => LocT.T2;
    public T2Puzzle() {
        name = "T2P";
        clue = "The first step is always the hardest. But first; youtube.com/watch?v=YNaMdnDP0Z8";
        help = $"{Flag} [int]";
        inp = "Awoooo";
    }
    public override void Initialize() {
        ans = ["Awoooo"];
        base.Initialize();
    }
}