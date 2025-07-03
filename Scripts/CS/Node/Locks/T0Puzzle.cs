using System.Linq;
using Godot;

public class T0Puzzle : Lock {
    public override string[] Flag => ["--t0"];
    public override LocT LocT => LocT.T0;
    public T0Puzzle() {
        name = "T0P";
        clue = "The first step is always the hardest. But first; what is \"Berkeley Open Infrastructure for Network Computing\"?";
        help = $"{Flag} [int]";
        inp = "BOINC";
    }
    public override void Initialize() {
        ans = ["BOINC"];
        base.Initialize();
    }
}
public class C0 : Lock {
    public override string[] Flag => ["--c0"];
    public override LocT LocT => LocT.C0;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly (string, string)[] colorPairs = [("red", "cyan"), ("green", "magenta"), ("blue", "yellow"), ("white", "black")];

    public C0() {
        name = "C0";
        clue = "Name the opposition.";
        help = $"{Flag[0]} [string]";
    }
    public override void Initialize() {
        int pairIndex = GD.RandRange(0, 3); bool swapped = GD.RandRange(0, 1) == 0;
        ans = [colorPairs[pairIndex].Item1];
        inp = colorPairs[pairIndex].Item2;
        if (swapped) {
            (colorPairs[pairIndex].Item2, colorPairs[pairIndex].Item1) = (colorPairs[pairIndex].Item1, colorPairs[pairIndex].Item2);
        }
        err = $"{inp}";
        base.Initialize();
    }
    public override bool[] UnlockAttempt(string[] keys) {
        keys = keys.Select(k => k.ToLower()).ToArray();
        return base.UnlockAttempt(keys);
    }
}
public class C1 : Lock {
    public override string[] Flag => ["--c1"];
    public override LocT LocT => LocT.C1;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[][] colorGroups = [
        ["white", "black"],
        ["red", "green", "blue"],
        ["cyan", "magenta", "yellow"]
    ];

    public C1() {
        name = "C1";
        clue = "Three stand before you. Expel the outcast.";
        help = $"{Flag[0]} [string]";
    }

    public override void Initialize() {
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
        base.Initialize();
    }
    public override bool[] UnlockAttempt(string[] keys) {
        keys = keys.Select(k => k.ToLower()).ToArray();
        return base.UnlockAttempt(keys);
    }
}
public class C3 : Lock {
    public override string[] Flag => ["--c3"];
    public override LocT LocT => LocT.C3;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[] groupNames = ["bw", "rgb", "cmy"];
    readonly string[][] colorGroups = [
        ["white", "black"],
        ["red", "green", "blue"],
        ["cyan", "magenta", "yellow"]
    ];

    public C3() {
        name = "C3";
        clue = "Three hues, two kindred. Find their family.";
        help = $"{Flag[0]} [string]";
    }

    public override void Initialize() {
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
        base.Initialize();
    }
    public override bool[] UnlockAttempt(string[] keys) {
        keys = keys.Select(k => k.ToLower()).ToArray();
        return base.UnlockAttempt(keys);
    }
}
public class I4X : Lock {
    public override string[] Flag => ["--i4", "--2xtract"];
    public override LocT LocT => LocT.I4X;

    readonly string[] intPool = ["1", "2", "3", "4"];
    readonly string[] actionPool = ["fl1p", "2bin", "der3f", "bl4nk"];
    public I4X() {
        name = "I4X";
        clue = "Among the crowd will be knights and heroes. But only one dare rise to our blade.";
        help = $"{Flag[0]} [int] {Flag[1]} [string]";
        inp = "";
    }

    public override void Initialize() {
        ans = [intPool[GD.RandRange(0, 3)], actionPool[GD.RandRange(0, 3)]];
        base.Initialize();
    }
}
public class M2 : Lock {
    public override string[] Flag => ["--m2"];
    public override LocT LocT => LocT.M2;

    public M2() {
        name = "M2";
        clue = "Seek the last square before the overflow.";
        help = $"{Flag[0]} [int]";
    }

    public override void Initialize() {
        int upperBound = GD.RandRange(2, 65536);
        int root = Mathf.Sqrt(upperBound) == Mathf.Floor(Mathf.Sqrt(upperBound)) ? (int)Mathf.Sqrt(upperBound) - 1 : (int)Mathf.Floor(Mathf.Sqrt(upperBound));
        ans = [$"{root * root}"];
        inp = $"{upperBound}";
        base.Initialize();
    }
}












public class P90 : Lock {
    public override string[] Flag => ["--p90"];
    public override LocT LocT => LocT.P90;

    readonly string[] intPool = ["2", "3", "5", "7", "11", "13", "17", "19", "23", "29", "31", "37", "41", "43", "47", "53", "59", "61", "67", "71", "73", "79", "83", "89"];

    public P90() {
        name = "P90";
        clue = "Only the sentinels stand tall—their mind is themselves and their origin. Find the true guards among them.";
        help = $"{Flag[0]} [int]";
        inp = "";
    }

    public override void Initialize() {
        ans = [$"{intPool[GD.RandRange(0, 7)]}"];
        base.Initialize();
    }
}
public class P16X : Lock {
    public override string[] Flag => ["--p16", "--4xtract"];
    public override LocT LocT => LocT.P16X;

    readonly string[] intPool = ["2", "3", "5", "7", "11", "13", "17", "19", "23", "29", "31", "37", "41", "43", "47", "53"];
    readonly string[] actionPool = ["fl1p", "2bin", "der3f", "bl4nk"];
    public P16X() {
        name = "P16X";
        clue = "Gates only yield to those with indivisible will. Fight them all; the resolute will answer";
        help = $"{Flag[0]} [int] {Flag[1]} [string]";
        inp = "";
    }

    public override void Initialize() {
        ans = [$"{intPool[GD.RandRange(0, 15)]}", actionPool[GD.RandRange(0, 3)]];
        base.Initialize();
    }
}
public class M3 : Lock {
    public override string[] Flag => ["--m3"];
    public override LocT LocT => LocT.M3;

    public M3() {
        name = "M3";
        clue = "Before comprehension dissolves, a final vessel holds the chaos—a perfect cube, find it.";
        help = $"{Flag[0]} [int]";
    }

    public override void Initialize() {
        int upperBound = GD.RandRange(2, 512);
        int root = Mathf.Pow(upperBound, 1.0 / 3.0) == Mathf.Floor(Mathf.Pow(upperBound, 1.0 / 3.0)) ? (int)Mathf.Pow(upperBound, 1.0 / 3.0) + 1 : (int)Mathf.Ceil(Mathf.Pow(upperBound, 1.0 / 3.0));
        ans = [$"{root * root * root}"];
        inp = $"{upperBound}";
        base.Initialize();
    }
}
public class I16 : Lock {
    public override string[] Flag => ["--i16"];
    public override LocT LocT => LocT.I16;

    public I16() {
        name = "I16";
        clue = "The system flooded with noise. As we descent the axe upon each one, our time shorten.";
        help = $"{Flag[0]} [int]";
        inp = "";
    }

    public override void Initialize() {
        ans = [$"{GD.RandRange(1, 16)}"];
        base.Initialize();
    }
}
public class F8X : Lock {
    public override string[] Flag => ["--f8", "--3xtract"];
    public override LocT LocT => LocT.F8X;

    readonly string[] intPool = ["1", "2", "3", "5", "8", "13", "21", "34"];
    readonly string[] actionPool = ["fl1p", "2bin", "der3f", "bl4nk"];
    public F8X() {
        name = "F8";
        clue = "In this world, growth follows memory. From the flicker to the fire storm, each step remembers the two before it.";
        help = $"{Flag[0]} [int] {Flag[1]} [string]";
        inp = "";
    }
    public override void Initialize() {
        ans = [intPool[GD.RandRange(0, 7)], actionPool[GD.RandRange(0, 3)]];
        base.Initialize();
    }
}
