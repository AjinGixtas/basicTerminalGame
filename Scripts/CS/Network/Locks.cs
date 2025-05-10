using Godot;
using System;
using System.Linq;

public abstract class Lock {
    protected string clue="<Empty>", err = "<Empty>", help = "<Empty>", inp = "<Empty>", name = "<Empty>";
    public abstract int Cost { get; }
    public abstract int MinLvl { get; }
    public virtual void Intialize() { GD.Print(Flag[0] + ' ' + ans.Join(" ") + ' '); }
    public virtual bool UnlockAttempt(string input, int index) { return input.Equals(ans[index]); }
    public abstract string[] Flag { get; }
    protected string[] ans;
    public string Name => name;
    public string Clue => clue;
    public string Help => help;
    public string Inp => inp;
    public string Err => err;
}

public class I5 : Lock {
    public override string[] Flag => ["--i5"];
    public override int Cost => 1;
    public override int MinLvl => 1;

    public I5() {
        name = "I5";
        clue = "Among the crowd will be knights and heroes. But only one dare rise to our blade.";
        help = $"{Flag} [int]";
        inp = "";
    }

    public override void Intialize() {
        ans = [$"{GD.RandRange(1, 5)}"];
        base.Intialize();
    }
}
public class P23 : Lock {
    public override string[] Flag => ["--p23"];
    public override int Cost => 1;
    public override int MinLvl => 3;

    readonly string[] intPool = ["2", "3", "5", "7", "11", "13", "17", "19", "23"];

    public P23() {
        name = "P9";
        clue = "Only the sentinels stand tall—their mind is themselves and their origin. Find the true guards among them.";
        help = $"{Flag} [int]";
        inp = "";
    }

    public override void Intialize() {
        ans = [$"{intPool[GD.RandRange(0, 8)]}"];
        base.Intialize();
    }
}
public class F55 : Lock {
    public override string[] Flag => ["--f55"];
    public override int Cost => 1;
    public override int MinLvl => 3;
    readonly string[] intPool = ["1", "1", "2", "3", "5", "8", "13", "21", "34", "55"];
    public F55() {
        name = "F55";
        clue = "In this world, growth follows memory. From the flicker to the fire storm, each step remembers the two before it.";
        help = $"{Flag} [int]";
        inp = "";
    }
    public override void Intialize() {
        ans = [$"{intPool[GD.RandRange(0, 9)]}"];
        base.Intialize();
    }
}
public class I13 : Lock {
    public override string[] Flag => ["--i13"];
    public override int Cost => 1;
    public override int MinLvl => 6;

    public I13() {
        name = "I13";
        clue = "The system flooded with noise. As we descent the axe upon each one, our time shorten.";
        help = $"{Flag} [int]";
        inp = "";
    }

    public override void Intialize() {
        ans = [$"{GD.RandRange(1, 13)}"];
        base.Intialize();
    }
}
public class P16 : Lock {
    public override string[] Flag => ["--p16", "--16xtract"];
    public override int Cost => 1;
    public override int MinLvl => 9;

    readonly string[] intPool = ["2", "3", "5", "7", "11", "13", "17", "19", "23", "29", "31", "37", "41", "43", "47", "53"];
    readonly string[] actionPool = ["2bin", "fl1p", "der3f", "bl4nk"];
    public P16() {
        name = "P16";
        clue = "Gates only yield to those with indivisible will. Fight them all; the resolute will answer";
        help = $"{Flag} [int]";
        inp = "";
    }

    public override void Intialize() {
        ans = [$"{intPool[GD.RandRange(0, 16)]}", actionPool[GD.RandRange(0, actionPool.Length-1)]];
        base.Intialize();
    }
}
public class C0 : Lock {
    public override string[] Flag => ["--c0"];
    public override int Cost => 1;
    public override int MinLvl => 1;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[,] colorOpPairs = { { "red", "cyan" }, { "green", "yellow" }, { "blue", "magenta" }, { "white", "black" } };

    public C0() {
        name = "C0";
        clue = "In the spectrum\'s reflection, the answer is hidden—opposite yet bound.";
        help = $"{Flag} [{colors.Join("|")}]";
    }

    public override void Intialize() {
        int pairIndex = GD.RandRange(0, 3), elementIndex = GD.RandRange(0, 1);
        ans = [colorOpPairs[pairIndex, elementIndex]];
        inp = colorOpPairs[pairIndex, 1 - elementIndex];
        err = $"{inp}";
        base.Intialize();
    }
}
public class C1 : Lock {
    public override string[] Flag => ["--c1"];
    public override int Cost => 1;
    public override int MinLvl => 3;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[][] colorGroups = [
        ["white", "black"],
        ["red", "green", "blue"],
        ["cyan", "magenta", "yellow"]
    ];

    public C1() {
        name = "C1";
        clue = "Most colors align with orders known. One strays—quietly dissonant.";
        help = $"{Flag} [{colors.Join("|")}]";
    }

    public override void Intialize() {
        string[] groupPile = Util.Shuffle(colorGroups[GD.RandRange(0, 2)].ToArray());
        string[] colorPile = Util.Shuffle(colors.ToArray());
        string[] group = [groupPile[0], groupPile[1], "null"];

        for (int i = 0; i < colorPile.Length; ++i) {
            if (groupPile.Contains(colorPile[i])) continue;
            group[2] = colorPile[i];
            ans = [group[2]];
            break;
        }

        group = Util.Shuffle(group.ToArray());
        inp = group.Join(" ");
        base.Intialize();
    }
}
public class C3 : Lock {
    public override string[] Flag => ["--c3"];
    public override int Cost => 1;
    public override int MinLvl => 3;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[] groupNames = ["bw", "rgb", "cmy"];
    readonly string[][] colorGroups = [
        ["white", "black"],
        ["red", "green", "blue"],
        ["cyan", "magenta", "yellow"]
    ];

    public C3() {
        name = "C3";
        clue = "From scattered hues, seek the harmony—find which order they almost obey.";
        help = $"{Flag} [{groupNames.Join("|")}]";
    }

    public override void Intialize() {
        int index = GD.RandRange(0, 2);
        ans = [groupNames[index]];

        string[] groupPile = Util.Shuffle(colorGroups[index].ToArray());
        string[] colorPile = Util.Shuffle(colors.ToArray());
        string[] group = [groupPile[0], groupPile[1], "null"];

        for (int i = 0; i < colorPile.Length; ++i) {
            if (groupPile.Contains(colorPile[i])) continue;
            group[2] = colorPile[i];
            break;
        }

        group = Util.Shuffle(group.ToArray());
        inp = group.Join(" ");
        base.Intialize();
    }
}
public class M2 : Lock {
    public override string[] Flag => ["--m2"];
    public override int Cost => 1;
    public override int MinLvl => 3;

    public M2() {
        name = "M2";
        clue = "Compression stabilizes when energy repeats. Seek the last square before the breach.";
        help = $"{Flag} [int]";
    }

    public override void Intialize() {
        int upperBound = GD.RandRange(2, 256);
        int root = Math.Sqrt(upperBound) == Math.Floor(Math.Sqrt(upperBound)) ? (int)Math.Sqrt(upperBound) - 1 : (int)Math.Floor(Math.Sqrt(upperBound));
        ans = [$"{root * root}"];
        inp = $"{upperBound}";
        base.Intialize();
    }
}
public class M3 : Lock {
    public override string[] Flag => ["--m3"];
    public override int Cost => 1;
    public override int MinLvl => 3;

    public M3() {
        name = "M3";
        clue = "Before comprehension dissolves, a final vessel holds the chaos—a perfect cube, just big enough to bind the disc.";
        help = $"{Flag} [int]";
    }

    public override void Intialize() {
        int upperBound = GD.RandRange(2, 512);
        int root = Math.Cbrt(upperBound) == Math.Floor(Math.Cbrt(upperBound)) ? (int)Math.Cbrt(upperBound) + 1 : (int)Math.Ceiling(Math.Cbrt(upperBound));
        ans = [$"{root * root * root}"];
        inp = $"{upperBound}";
        base.Intialize();
    }
}
