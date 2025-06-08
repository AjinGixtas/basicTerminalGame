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
	public CError CrackAttempt(Dictionary<string, string> ans, double endEpoch) {
		if (this.endEpoch != endEpoch) { 
			this.endEpoch = endEpoch;
			for (int i = 0; i < activeLocks.Count; ++i) { activeLocks[i].Intialize(); }
		}
		for (int i = 0; i < activeLocks.Count; i++) {
			string[] flags = activeLocks[i].Flag;
			for (int j = 0; j < flags.Length; ++j) {
				if (ans.TryGetValue(flags[j], out string key)) {
					if (key == "") {
						TerminalProcessor.Say($"[{Util.Format("N0VALUE", StrType.ERROR)}] {Util.Format("Denied access by", StrType.DECOR)} {Util.Format(activeLocks[i].Name, StrType.CMD_FLAG)}");
						TerminalProcessor.Say("-r", $"Missing key for {Util.Format(flags[j], StrType.CMD_FLAG)}");
						if (activeLocks[i].Clue.Length > 0) { TerminalProcessor.Say($"[color={Util.CC(Cc.W)}]{activeLocks[i].Inp}[/color]"); }
						return CError.MISSING;
					}
					if (!activeLocks[i].UnlockAttempt(key, j)) {
						TerminalProcessor.Say($"[{Util.Format("WRON6KY", StrType.ERROR)}] {Util.Format("Denied access by", StrType.DECOR)} {Util.Format(activeLocks[i].Name, StrType.CMD_FLAG)}");
						TerminalProcessor.Say("-r", $"Incorrect key for {Util.Format(flags[j], StrType.CMD_FLAG)}");
						if (activeLocks[i].Clue.Length > 0) { TerminalProcessor.Say($"[color={Util.CC(Cc.W)}]{activeLocks[i].Inp}[/color]"); }
						return CError.INCORRECT;
					}
					continue;
				} else {
					TerminalProcessor.Say($"[{Util.Format("MI55ING", StrType.ERROR)}] {Util.Format("Denied access by", StrType.DECOR)} {Util.Format(activeLocks[i].Name, StrType.CMD_FLAG)}");
					TerminalProcessor.Say("-r", $"Missing flag {Util.Format(flags[j], StrType.CMD_FLAG)}");
					return CError.MISSING;
				}
			}
			TerminalProcessor.Say($"[{Util.Format("SUCCESS", StrType.PART_SUCCESS)}] {Util.Format("Bypassed", StrType.DECOR)} {Util.Format(activeLocks[i].Name, StrType.CMD_FLAG)}");
		}
		lockPool = []; activeLocks = []; // Destroy all security system
		TerminalProcessor.Say($"{Util.Format("Node defense cracked.", StrType.FULL_SUCCESS)} All security system [color={Util.CC(Cc.gR)}]destroyed[/color].");
		TerminalProcessor.Say($"Run {Util.Format("analyze", StrType.CMD_FUL)} for all new information.");
		return CError.OK;
	}
}
