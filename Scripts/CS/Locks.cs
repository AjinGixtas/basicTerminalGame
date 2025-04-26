using Godot;
using Godot.Collections;
using System;
using System.Linq;

public abstract class Lock {
    protected string ans, que = "<Empty>", cmd = "<Empty>", inp = "<Empty>";
    public abstract string Flag { get; }
    public abstract int Cost { get; }
    public abstract int MinLvl { get; }
    public abstract void Intialize();
    public virtual bool UnlockAttempt(string input) => input.Equals(ans);
    public string Ans => ans;
    public string Help => cmd;
    public string Input => inp;
    public string Question => que;
}
public class I5 : Lock {
    public override string Flag => "-i5";
    public override int Cost => 1;
    public override int MinLvl => 1;

    public I5() {
        que = "";
        cmd = $"{Flag} [int]";
    }

    public override void Intialize() {
        ans = $"{GD.RandRange(1, 5)}";
    }
}

public class P9 : Lock {
    public override string Flag => "-p9";
    public override int Cost => 1;
    public override int MinLvl => 3;

    readonly string[] ansPool = ["2", "3", "5", "7", "11", "13", "17", "19", "23"];

    public P9() {
        que = "";
        cmd = $"{Flag} [int]";
    }

    public override void Intialize() {
        ans = $"{ansPool[GD.RandRange(0, 8)]}";
    }
}

public class I13 : Lock {
    public override string Flag => "-i13";
    public override int Cost => 1;
    public override int MinLvl => 6;

    public I13() {
        que = "";
        cmd = $"{Flag} [int]";
    }

    public override void Intialize() {
        ans = $"{GD.RandRange(1, 13)}";
    }
}

public class P16 : Lock {
    public override string Flag => "-p16";
    public override int Cost => 1;
    public override int MinLvl => 9;

    readonly string[] ansPool = ["2", "3", "5", "7", "11", "13", "17", "19", "23", "29", "31", "37", "41", "43", "47", "53"];

    public P16() {
        que = "";
        cmd = $"{Flag} [int]";
    }

    public override void Intialize() {
        ans = $"{ansPool[GD.RandRange(0, 16)]}";
    }
}

public class C0 : Lock {
    public override string Flag => "-c0";
    public override int Cost => 1;
    public override int MinLvl => 1;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[,] colorOpPairs = { { "red", "cyan" }, { "green", "yellow" }, { "blue", "magenta" }, { "white", "black" } };

    public C0() {
        cmd = $"{Flag} [{colors.Join("|")}]";
    }

    public override void Intialize() {
        int pairIndex = GD.RandRange(0, 3), elementIndex = GD.RandRange(0, 1);
        ans = colorOpPairs[pairIndex, elementIndex];
        inp = colorOpPairs[pairIndex, 1 - elementIndex];
        que = $"\n{inp}";
    }
}

public class C1 : Lock {
    public override string Flag => "-c1";
    public override int Cost => 1;
    public override int MinLvl => 3;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[][] colorGroups = [
        ["white", "black"],
        ["red", "green", "blue"],
        ["cyan", "magenta", "yellow"]
    ];

    public C1() {
        cmd = $"{Flag} [{colors.Join("|")}]";
    }

    public override void Intialize() {
        string[] groupPile = Utilitiy.Shuffle(colorGroups[GD.RandRange(0, 2)].ToArray());
        string[] colorPile = Utilitiy.Shuffle(colors.ToArray());
        string[] group = [groupPile[0], groupPile[1], "null"];

        for (int i = 0; i < colorPile.Length; ++i) {
            if (groupPile.Contains(colorPile[i])) continue;
            group[2] = colorPile[i];
            ans = group[2];
            break;
        }

        group = Utilitiy.Shuffle(group.ToArray());
        inp = group.Join(" ");
        que = $"\n{inp}";
    }
}

public class C3 : Lock {
    public override string Flag => "-c3";
    public override int Cost => 1;
    public override int MinLvl => 3;

    readonly string[] colors = ["red", "cyan", "green", "yellow", "blue", "magenta", "white", "black"];
    readonly string[] groupNames = ["wb", "rgb", "cmy"];
    readonly string[][] colorGroups = [
        ["white", "black"],
        ["red", "green", "blue"],
        ["cyan", "magenta", "yellow"]
    ];

    public C3() {
        cmd = $"{Flag} [{groupNames.Join("|")}]";
    }

    public override void Intialize() {
        int index = GD.RandRange(0, 2);
        ans = groupNames[index];

        string[] groupPile = Utilitiy.Shuffle(colorGroups[index].ToArray());
        string[] colorPile = Utilitiy.Shuffle(colors.ToArray());
        string[] group = [groupPile[0], groupPile[1], "null"];

        for (int i = 0; i < colorPile.Length; ++i) {
            if (groupPile.Contains(colorPile[i])) continue;
            group[2] = colorPile[i];
            break;
        }

        group = Utilitiy.Shuffle(group.ToArray());
        inp = group.Join(" ");
        que = $"\n{inp}";
    }
}

public class M2 : Lock {
    public override string Flag => "-m2";
    public override int Cost => 1;
    public override int MinLvl => 3;

    public M2() {
        cmd = $"{Flag} [int]";
    }

    public override void Intialize() {
        int upperBound = GD.RandRange(2, 256);
        int root = Math.Sqrt(upperBound) == Math.Floor(Math.Sqrt(upperBound)) ? (int)Math.Sqrt(upperBound) - 1 : (int)Math.Floor(Math.Sqrt(upperBound));
        ans = $"{root * root}";
        inp = $"{upperBound}";
        que = $"\n{inp}";
    }
}

public class M3 : Lock {
    public override string Flag => "-m3";
    public override int Cost => 1;
    public override int MinLvl => 3;

    public M3() {
        cmd = $"{Flag} [int]";
    }

    public override void Intialize() {
        int upperBound = GD.RandRange(2, 512);
        int root = Math.Cbrt(upperBound) == Math.Floor(Math.Cbrt(upperBound)) ? (int)Math.Cbrt(upperBound) + 1 : (int)Math.Ceiling(Math.Cbrt(upperBound));
        ans = $"{root * root * root}";
        inp = $"{upperBound}";
        que = $"\n{inp}";
    }
}
