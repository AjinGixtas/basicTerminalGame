using System;
using System.Linq;
using Godot;

public class T1Puzzle : Lock {
    public override string[] Flag => ["--t1"];
    public override LocT LocT => LocT.T1;
    public T1Puzzle() {
        name = "T1P";
        clue = "The first step is always the hardest. But first; what's the answer to life, the universe, and everything?";
        help = $"{Flag} [int]";
        inp = "42";
    }
    public override void Initialize() {
        ans = ["42"];
        base.Initialize();
    }
}
public class XRL : Lock {
    public override string[] Flag => ["--xla", "--xlb"];
    public override LocT LocT => LocT.XRL;
    public XRL() {
        name = "XL";
        clue = "";
        help = $"{Flag[0]} [int] {Flag[1]} [int]";
        inp = "XL";
    }
    int a = -1, b = -1, C = -1;
    public override void Initialize() {
        int A = GD.RandRange(0, 128);
        int B = GD.RandRange(128, 256);
        C = A ^ B;
        ans = [A.ToString(), B.ToString()];
        a = GD.RandRange(A, 256);
        b = GD.RandRange(0, B);
        inp = $"{a} {b} {C}";
        base.Initialize();
    }
    public override bool[] UnlockAttempt(string[] keys) {
        if (keys.Length != Flag.Length || keys.Length != 2) { throw new System.Exception("This never supposes to happen"); }
        bool[] result = new bool[Flag.Length];
        int[] pair = new int[2];
        for (int i = 0; i < 2; ++i) {
            if (!int.TryParse(keys[i], out pair[i])) return new bool[2];
        }

        result[0] = pair[0] <= a; result[1] = pair[1] >= b;
        if (!result[0] || !result[1]) return result;
        int C = pair[0] ^ pair[1];
        result[0] = C == this.C; result[1] = C == this.C;
        return result;
    }
}
public class S3M : Lock {
    public override string[] Flag => ["--s0m", "--s1m", "--s2m"];
    public override LocT LocT => LocT.S3M;
    public S3M() {
        name = "S3M";
        clue = "";
        help = $"{Flag[0]} [int] {Flag[1]} [int] {Flag[2]} [int]";
        inp = "";
    }
    int[] values;
    public override void Initialize() {
        values = new int[6];
        for (int i = 0; i < values.Length; ++i) { values[i] = GD.RandRange(1, 32); }
        int sum = values[0] + values[1] + values[2];
        ans = [$"{sum}", $"{sum}", $"{sum}"];

        values = Util.Shuffle<int>(values);
        inp = $"{sum}\n{values[0]} {values[1]} {values[2]} {values[3]} {values[4]} {values[5]}";
        base.Initialize();
    }
    public override bool[] UnlockAttempt(string[] keys) {
        if (keys.Length != Flag.Length) { throw new System.Exception("This never supposes to happen"); }
        int[] indexes = new int[Flag.Length];
        for (int i = 0; i < Flag.Length; ++i) {
            if (!int.TryParse(keys[i], out indexes[i])) { return new bool[Flag.Length]; }
            if (indexes[i] < 0 || indexes[i] >= values.Length) { return new bool[Flag.Length]; }
        }
        if (indexes.Length != indexes.Distinct().Count()) return new bool[Flag.Length];

        int sum = values[indexes[0]] + values[indexes[1]] + values[indexes[2]];
        if ($"{sum}" != ans[0]) return new bool[Flag.Length];
        bool[] result = new bool[3] { true, true, true };
        return result;
    }
}
public class VSP : Lock {
    public override string[] Flag => ["--vsp"];
    public override LocT LocT => LocT.VSP;
    public VSP() {
        name = "VSP";
        clue = "";
        help = $"{Flag[0]} [string]";
        inp = "";
    }
    public override void Initialize() {
        int shift = GD.RandRange(0, 4);
        inp = Util.GenerateRandomString(length: GD.RandRange(16, 32), chars: "abcdefghijklmnopqrstuvwxyz");
        char[] chars = inp.ToCharArray();
        string vowels = "aeiou";

        for (int i = 0; i < chars.Length; i++) {
            char c = chars[i];
            if (!vowels.Contains(c)) continue;

            int index = vowels.IndexOf(c);
            int newIndex = (index + shift) % vowels.Length;
            chars[i] = vowels[newIndex];
        }

        inp = $"{shift}{inp}"; // Better readability: space after shift
        ans = [new string(chars)];
        base.Initialize();
    }
    public override bool[] UnlockAttempt(string[] keys) {
        keys = keys.Select(k => k.ToLower()).ToArray();
        return base.UnlockAttempt(keys);
    }
}
public class M1NV : Lock {
    public override string[] Flag => ["--mix"];
    public override LocT LocT => LocT.M1NV;
    public M1NV() {
        name = "M1NV";
        clue = "Feed the beast until it knows itself.";
        help = $"{Flag[0]} [string]";
        inp = "";
    }
    
    public override void Initialize() {
        uint start = (uint)GD.RandRange((long)uint.MinValue, (long)uint.MaxValue);
        byte[] hash = BitConverter.GetBytes(start);
        int count = GD.RandRange(1, 32);
        for (int i = 0; i < count; ++i) {
            hash = BitConverter.GetBytes(Util.GetFnv1aHash(hash));
        }
        uint end = BitConverter.ToUInt32(hash);
        inp = $"0x{start:X8} 0x{end:X8}";
        ans = [$"{count}"];
        base.Initialize();
    }
    
    public override bool[] UnlockAttempt(string[] keys) {
        keys = keys.Select(k => k.ToLower()).ToArray();
        return base.UnlockAttempt(keys);
    }
}
public class FRAJ : Lock {
    public override string[] Flag => ["--FRAJ"];
    public override LocT LocT => LocT.FRAJ;

    public FRAJ() {
        name = "FRAJ";
        clue = "A good shot .";
        help = $"{Flag[0]} [float]";
    }

    public override void Initialize() {
        double D = GD.RandRange(5.0, 50.0);   // distance (meters)
        double aH = GD.RandRange(0.0, 2.0);   // gun barrel height (meters)
        double bH = GD.RandRange(aH + 0.5, aH + 5);  // target head above gun (meters)

        double tanMin = -aH / D;
        double tanMax = (bH - aH) / D;

        double thetaMin = Math.Atan(tanMin) * (180.0 / Math.PI);
        double thetaMax = Math.Atan(tanMax) * (180.0 / Math.PI);

        double range = thetaMax - thetaMin;

        ans = [range.ToString("F3")];
        inp = $"{D:F2} {aH:F2} {bH:F2}";
        base.Initialize();
    }
    public override bool[] UnlockAttempt(string[] keys) {
        if (keys.Length != Flag.Length) { throw new System.Exception("This never supposes to happen"); }

        bool[] result = new bool[Flag.Length];
        string user = keys[0].Trim();
        string expected = ans[0].Trim();

        if (user.Equals(expected)) {
            result[0] = true;
            return result;
        }

        // Allow ',' instead of '.'
        user = user.Replace(',', '.');

        // Must contain exactly one '.'
        int dotIndex = user.IndexOf('.');
        if (dotIndex < 0) { result[0] = false; return result; }

        // Split
        string intPart = user.Substring(0, dotIndex), fracPart = user.Substring(dotIndex + 1, 3).PadRight(3, '0');
        if (fracPart.Length != 3) { result[0] = false; return result; }

        // Make sure both parts are numeric
        if (!int.TryParse(intPart, out _) || !int.TryParse(fracPart, out _)) { result[0] = false; return result; }

        // Recombine to match
        string normalized = $"{intPart}.{fracPart}";
        result[0] = normalized.Equals(expected); return result;
    }
}