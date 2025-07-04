using System.Collections.Generic;
using System.Linq;

public class LockSystem {
	List<Lock> activeLocks = [];
	public LockSystem(LocT[] lockCode) {
		for(int i = 0; i < lockCode.Length; i++) {
            Lock lockObj = LockEnumMapper(lockCode[i]);
			activeLocks.Add(lockObj);
            activeLocks = Util.Shuffle<Lock>(activeLocks);
        }
    }
    static readonly Dictionary<SecLvl, LocT[]> LocMap = new(){
		{ SecLvl.NOSEC, [] }, // Placeholder
		{ SecLvl.LOSEC, [LocT.I4X, LocT.M2, LocT.C0, LocT.C1, LocT.C3]},
		{ SecLvl.MISEC, [LocT.FRAJ, LocT.M1NV, LocT.S3M, LocT.VSP, LocT.XRL]},
		{ SecLvl.HISEC, [LocT.FRAJ, LocT.M1NV, LocT.S3M, LocT.VSP, LocT.XRL]},
		{ SecLvl.MASEC, [LocT.I4X, LocT.F8X, LocT.I16, LocT.P16X, LocT.P90, LocT.M2, LocT.M3, LocT.C0, LocT.C1, LocT.C3]},
	};
    public int LockIntialization(SecLvl secLvl, int defLvl) {
		if (secLvl == SecLvl.NOSEC) { activeLocks = []; return 0; }
		List<LocT> locksPossible = [.. LocMap[secLvl]];
        if (activeLocks.Count > 0) { // Prevent duplicate locks
            HashSet<LocT> activeCodes = [.. activeLocks.Select(l => l.LocT)];
			locksPossible.RemoveAll(l => activeCodes.Contains(l));
        }

        locksPossible = Util.Shuffle<LocT>(locksPossible);
		for (int i = activeLocks.Count; i < defLvl; ++i) {
            if (locksPossible.Count == 0) { throw new System.Exception("No more locks available for the given security level and difficulty level. LockSystem.cs"); }
			LocT locType = locksPossible[0]; locksPossible.RemoveAt(0);
            activeLocks.Add(LockEnumMapper(locType));
        }
		// Double shuffle because first shuffle only affect the draw pile. This is the "play" pile.
		activeLocks = Util.Shuffle<Lock>(activeLocks);
        return 0;
	}
	double endEpoch = -1;
	public (CError, string, string, string)[] CrackAttempt(Dictionary<string, string> ans, double endEpoch)
	{
		if (this.endEpoch != endEpoch) { 
			this.endEpoch = endEpoch;
			for (int i = 0; i < activeLocks.Count; ++i) { activeLocks[i].Initialize(); }
		}
		List<(CError, string, string, string)> errors = [];
        for (int i = 0; i < activeLocks.Count; i++) {
            string[] flags = activeLocks[i].Flag;
            string[] keys = new string[flags.Length];
            bool failed = false;
            for (int j = 0; j < flags.Length; j++) {
                if (ans.TryGetValue(flags[j], out string key)) {
                    keys[j] = key;
                } else {
                    errors.Add((CError.MISSING, activeLocks[i].Name, flags[j], ""));
                    return [.. errors];
                }
            }
            bool[] result = activeLocks[i].UnlockAttempt(keys);
            for (int j = 0; j < result.Length; j++) {
                if (result[j]) continue;
                errors.Add((CError.INCORRECT, activeLocks[i].Name, flags[j], activeLocks[i].Inp));
                failed = true;
            }
            if (failed) return [.. errors];
            else errors.Add((CError.OK, activeLocks[i].Name, "", ""));
        }
        // Won't be used in the current implementation, but can be used to reset the system
        // lockPool = []; activeLocks = []; // Destroy all security system
        errors.Add((CError.OK, "", "", ""));
        return [.. errors];
    }
    public static Lock LockEnumMapper(LocT type) {
        return type switch {
            LocT.T0 => new T0Puzzle(),
            LocT.I4X => new I4X(),
            LocT.F8X => new F8X(),
            LocT.I16 => new I16(),
            LocT.P16X => new P16X(),
            LocT.P90 => new P90(),
            LocT.M2 => new M2(),
            LocT.M3 => new M3(),
            LocT.C0 => new C0(),
            LocT.C1 => new C1(),
            LocT.C3 => new C3(),
            LocT.T1 => new T1Puzzle(),
            LocT.XRL => new XRL(),
            LocT.S3M => new S3M(),
            LocT.VSP => new VSP(),
            LocT.M1NV => new M1NV(),
            LocT.FRAJ => new FRAJ(),
            LocT.T2 => new T2Puzzle(),
            LocT.T3 => new T3Puzzle(),
            _ => throw new System.NotImplementedException($"Lock type {type} is not implemented.")
        };
    }
}
