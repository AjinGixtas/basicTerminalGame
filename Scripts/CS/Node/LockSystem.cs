using Godot;
using System.Collections.Generic;

public class LockSystem {
	List<Lock> activeLocks = [];
	List<Lock> lockPool = [new I4X(), new F8X(), new I16(), new P16X(), new P90(), new M2(), new M3(), new C0(), new C1(), new C3()];
	public LockSystem(int lockCode=0) {
		if (lockCode != 0) {
			for (int i = lockPool.Count - 1; i >= 0; --i) {
				if (((1 << i) & lockCode) == 0) continue;
				activeLocks.Add(lockPool[i]);
				lockPool.RemoveAt(i);
			}
			activeLocks = Util.Shuffle<Lock>(activeLocks);
        }
    }
    public int LockIntialization(int secLvl) {
		int usedLvl = 0;
		for (int i = activeLocks.Count; i < lockPool.Count; i++) {
			if (usedLvl + lockPool[i].Cost <= secLvl && lockPool[i].MinLvl <= secLvl) {
				activeLocks.Add(lockPool[i]);
				usedLvl += lockPool[i].Cost;
			}
		}
		activeLocks = Util.Shuffle<Lock>(activeLocks);
		lockPool.Clear();
        return 0;
	}
	double endEpoch = -1;
	public (CError, string, string, string)[] CrackAttempt(Dictionary<string, string> ans, double endEpoch)
	{
		if (this.endEpoch != endEpoch) { 
			this.endEpoch = endEpoch;
			for (int i = 0; i < activeLocks.Count; ++i) { activeLocks[i].Intialize(); }
		}
		List<(CError, string, string, string)> errors = [];
        for (int i = 0; i < activeLocks.Count; i++) {
			string[] flags = activeLocks[i].Flag;
			for (int j = 0; j < flags.Length; j++) {
				if (ans.TryGetValue(flags[j], out string key)) {
					if (key == "") {
						errors.Add((CError.MISSING, activeLocks[i].Name, flags[j], activeLocks[i].Inp));
						return [.. errors]; // If any key is empty, return immediately
					}
					if (!activeLocks[i].UnlockAttempt(key, j)) {
						errors.Add((CError.INCORRECT, activeLocks[i].Name, flags[j], activeLocks[i].Inp));
						return [.. errors]; // If any key is incorrect, return immediately
					}
				} else {
					errors.Add((CError.MISSING, activeLocks[i].Name, flags[j], ""));
					return [.. errors]; // If any key is missing, return immediately
				}
			}
			errors.Add((CError.OK, activeLocks[i].Name, "", ""));
		}
		lockPool = []; activeLocks = []; // Destroy all security system
		errors.Add((CError.OK, "", "", ""));
        return [.. errors];
    }
}
