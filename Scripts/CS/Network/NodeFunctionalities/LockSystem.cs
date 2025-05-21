using System.Collections.Generic;

public class LockSystem {
	List<Lock> activeLocks = [];
	List<Lock> lockPool = [new I5(), new P23(), new I13(), new P16(), new C0(), new C1(), new C3(), new M2(), new M3()];
	public LockSystem() { }
	public int LockIntialization(int secLvl) {
		activeLocks.Clear(); int usedLvl = 0;
		lockPool = Util.Shuffle<Lock>(lockPool);
		for (int i = 0; i < lockPool.Count; i++) {
			if (usedLvl + lockPool[i].Cost <= secLvl && lockPool[i].MinLvl <= secLvl) {
				activeLocks.Add(lockPool[i]);
				usedLvl += lockPool[i].Cost;
			}
		}
		return 0;
	}
	double timeStamp = -1;
	// 0 - Success; 1 - Missing flag; 2 - Missing key; 3 - Incorrect key; 4 - Timeout
	public int CrackAttempt(Dictionary<string, string> ans, double timeStamp) {
		if (this.timeStamp != timeStamp) { 
			this.timeStamp = timeStamp;
			for (int i = 0; i < activeLocks.Count; ++i) { activeLocks[i].Intialize(); }
		}
		for (int i = 0; i < activeLocks.Count; i++) {
			string[] flags = activeLocks[i].Flag;
			for (int j = 0; j < flags.Length; ++j) {
				if (ans.TryGetValue(flags[j], out string key)) {
					if (key == "") {
						TerminalProcessor.Say($"[{Util.Format("N0VALUE", StrType.ERROR)}] Denied access by {Util.Format(activeLocks[i].Name, StrType.CMD_FLAG)}");
						TerminalProcessor.Say("-r", $"Missing key for {Util.Format(flags[j], StrType.CMD_FLAG)}");
						if (activeLocks[i].Clue.Length > 0) { TerminalProcessor.Say($"[color={Util.CC(Cc.rgb)}]{activeLocks[i].Inp}"); }
						return 2;
					} else if (!activeLocks[i].UnlockAttempt(key, j)) {
						TerminalProcessor.Say($"[{Util.Format("WRON6KY", StrType.ERROR)}] Denied access by {Util.Format(activeLocks[i].Name, StrType.CMD_FLAG)}");
						TerminalProcessor.Say("-r", $"Incorrect key for {Util.Format(flags[j], StrType.CMD_FLAG)}");
						if (activeLocks[i].Clue.Length > 0) { TerminalProcessor.Say($"[color={Util.CC(Cc.rgb)}]{activeLocks[i].Inp}"); }
						return 3;
					}
					continue;
				} else {
					TerminalProcessor.Say($"[{Util.Format("MI55ING", StrType.ERROR)}] Denied access by {Util.Format(activeLocks[i].Name, StrType.CMD_FLAG)}");
					TerminalProcessor.Say("-r", $"Missing flag {Util.Format(flags[j], StrType.CMD_FLAG)}");
					return 1;
				}
			}
			TerminalProcessor.Say($"[{Util.Format("SUCCESS", StrType.PART_SUCCESS)}] {Util.Format("Bypassed", StrType.DECOR)} {Util.Format(activeLocks[i].Name, StrType.CMD_FLAG)}");
		}
		lockPool = []; activeLocks = []; // Destroy all security system
		TerminalProcessor.Say($"{Util.Format("Node defense cracked.", StrType.FULL_SUCCESS)} All security system [color={Util.CC(Cc.gR)}]destroyed[/color].");
		TerminalProcessor.Say($"Run {Util.Format("analyze", StrType.CMD_FUL)} for all new information.");
		return 0;
	}
}
