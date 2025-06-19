using Godot;
using System.Collections.Generic;

public class LockSystem {
	List<Lock> activeLocks = [];
	public LockSystem(LockType[] lockCode) {
		for(int i = 0; i < lockCode.Length; i++) {
            Lock lockObj = Util.LockEnumMapper(lockCode[i]);
			activeLocks.Add(lockObj);
            activeLocks = Util.Shuffle<Lock>(activeLocks);
        }
    }
    public int LockIntialization(int secLvl) {
		int usedLvl = 0;
		for (int i = activeLocks.Count; i < activeLocks.Count; i++) {
			if (usedLvl + activeLocks[i].Cost <= secLvl && activeLocks[i].MinLvl <= secLvl) {
				usedLvl += activeLocks[i].Cost; continue;
			}
			activeLocks.RemoveAt(i);
        }
		activeLocks = Util.Shuffle<Lock>(activeLocks);
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
						errors.Add((CError.INCORRECT, activeLocks[i].Name, flags[j], activeLocks[i].Inp));
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
        // Won't be used in the current implementation, but can be used to reset the system
        // lockPool = []; activeLocks = []; // Destroy all security system
        errors.Add((CError.OK, "", "", ""));
        return [.. errors];
    }
}
