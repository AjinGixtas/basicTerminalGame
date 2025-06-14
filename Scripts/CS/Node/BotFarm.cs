using Godot;
using System.Collections.Generic;
using System.Linq;

public class BotFarm {
	const double baseHack = 10.1, baseGrow = .1, baseTime = 60.0;
	const double MAX_LVL = 255;
	public (double, double, double, double) HackCostCurve { get; init; }
	public (double, double, double, double) GrowCostCurve { get; init; }
	public (double, double, double, double) TimeCostCurve { get; init; }
	public (double, double, double, double) HackValuCurve { get; init; }
	public (double, double, double, double) GrowValuCurve { get; init; }
	public (double, double, double, double) TimeValuCurve { get; init; }
	double _hackVal = baseHack, _growVal = baseGrow, _timeVal = baseTime;
	public double BatchSize {
		get => _hackVal;
		private set {
			_hackVal = value;
		}
	}
	public double MineSpeed {
		get => _growVal;
		private set {
			_growVal = value;
		}
	}
	public double XferDelay {
		get => _timeVal;
		private set {
			_timeVal = value;
		}
	}
	int _hackLvl = 1, _growLvl = 1, _timeLvl = 1;
	public int BatchSizeLVL {
		get => _hackLvl;
		private set {
			_hackLvl = value; _hackVal = GetValu(HackValuCurve, _hackLvl); // Use the same curve as cost, just scaled down.
		}
	}
	public int MineSpeedLVL {
		get => _growLvl;
		private set {
			_growLvl = value;
			_growVal =
				GetValu(HackValuCurve, _growLvl * GrowValuCurve.Item3) /
				GetTime(TimeValuCurve, _growLvl * GrowValuCurve.Item4);
		}
	}
	public int XferDelayLVL {
		get => _timeLvl;
		private set {
			_timeLvl = value; _timeVal = GetTime(TimeValuCurve, _timeLvl);
		}
	}
	static double GetTime((double, double, double, double) c, double x) => c.Item1 / (c.Item4 * Mathf.Pow(c.Item2, x) + c.Item3 * x);
	static double GetValu((double, double, double, double) c, double x) => c.Item1 * Mathf.Pow(x, 3) + c.Item2 * Mathf.Pow(x, 2) + c.Item3 * Mathf.Pow(x, 1) + c.Item4 * Mathf.Pow(x, 0);

	public (int, double)[] mineralDistribution { get; init; }
	public string HostName { get; init; }
	public string DisplayName { get; init; }
	public string IP { get; init; }
	double _lifetime = 0;
	public double MAX_LIFE_TIME { get; init; }
	public double LifeTime {
		get => _lifetime;
		private set {
			_lifetime = value;
		}
	}
	public BotFarm(int defLvl, DriftNode driftNode) {
		List<int> mineralType = []; for (int i = Mathf.Max(defLvl - 2, 0); i <= Mathf.Min(9, defLvl + 1); ++i) { mineralType.Add(i); }
		mineralType.Remove(Mathf.Clamp(defLvl, 0, 9)); mineralType = Util.Shuffle<int>(mineralType);
		List<(int, double)> cacheResult = [];
		double weight1 = Mathf.Clamp(.02 * GD.RandRange(20, 60), .4, 1.0);
		cacheResult.Add((Mathf.Clamp(defLvl, 0, 9), weight1));
		double remainW = 1.0 - weight1;
		for (int i = 0; i < Mathf.Min(2, mineralType.Count) && 0.0 < remainW; ++i) {
			double W = Mathf.Clamp(.02 * GD.RandRange(17, 25), Mathf.Min(.2, remainW), remainW);
			cacheResult.Add((mineralType[i], W));
			remainW -= W;
		}
		cacheResult[0] = (cacheResult[0].Item1, cacheResult[0].Item2 + remainW);
		mineralDistribution = [.. cacheResult];
		mineralDistribution = Util.Shuffle<(int, double)>(mineralDistribution);
		HostName = driftNode.HostName; DisplayName = driftNode.DisplayName; IP = driftNode.IP;
		MAX_LIFE_TIME = 3600 * Mathf.Pow(Mathf.E / 2.5, -5.57180 * defLvl) * Mathf.Log(defLvl) + 1800 * GD.Randf();
		LifeTime = MAX_LIFE_TIME;
		HackCostCurve = (GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0),
						 GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0));
		GrowCostCurve = (GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0),
						 GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0));
		TimeCostCurve = (GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0),
						 GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0));
		HackValuCurve = (GD.RandRange(1e-5, 1e-4), GD.RandRange(1e-4, 1e-3),
						 GD.RandRange(4.0, 5.0), GD.RandRange(5, 10));
		// This is essentially stores the offset of hackValu curve level and timeValu curve level to calc its own value, thus, it will relies on the other 2 curve being accurate.
		GrowValuCurve = (0, 0, GD.RandRange(.8, 1.2), GD.RandRange(.8, 1.2));
		// This one essentially a speed increase algorithm, with v = d*(b^level)+c*level
		// a being the distance
		TimeValuCurve = (GD.RandRange(10.0, 60.0), 1.0 + GD.RandRange(.01, .02),
						 GD.RandRange(.5, 1.0), 1.0);
		BatchSizeLVL = MineSpeedLVL = XferDelayLVL = 1;

    }
	~BotFarm() {
		if (Util.HaveFinalWord)
			GD.Print($"HackFarm {HostName} is being destroyed");
	}
	public int UpgradeHack() {
		if (PlayerDataManager.WithdrawGC(GetBatchSizeCost()) != 0) return 1;
		BatchSizeLVL += 1; return 0;
	}
	public int UpgradeGrow() {
		if (PlayerDataManager.WithdrawGC(GetMineSpeedCost()) != 0) return 1;
		MineSpeedLVL += 1; return 0;
	}
	public int UpgradeTime() {
		if (PlayerDataManager.WithdrawGC(GetXferDelayCost()) != 0) return 1;
		XferDelayLVL += 1; return 0;
	}

	public double GetBatchSizeCost() => GetValu(HackCostCurve, BatchSizeLVL);
	public double GetMineSpeedCost() => GetValu(GrowCostCurve, MineSpeedLVL);
	public double GetXferDelayCost() => GetValu(TimeCostCurve, XferDelayLVL);
	double _mineralBacklog, _cycleTimeRemain;
	public double MBacklog { get => _mineralBacklog; private set => _mineralBacklog = value; }
	public double CycleTimeRemain { get => _cycleTimeRemain; private set => _cycleTimeRemain = value; }
	public (int, double)[] ProcessMinerals(double delta) {
		(int, double)[] output = new (int, double)[mineralDistribution.Length];
		CycleTimeRemain -= delta; LifeTime -= delta;
		MBacklog += MineSpeed * delta;
		if (CycleTimeRemain > 0) { return output; }

		double batch = Mathf.Min(MBacklog, BatchSize);
		for (int i = 0; i < mineralDistribution.Length; ++i) {
			output[i] = (mineralDistribution[i].Item1, mineralDistribution[i].Item2 * batch);
		}
		MBacklog -= batch;
		CycleTimeRemain += XferDelay;

		return output;
	}
	public BotFarm(HackFarmDataSaveResource res) {
		HackCostCurve = (res.cHackA, res.cHackB, res.cHackC, res.cHackD);
		GrowCostCurve = (res.cGrowA, res.cGrowB, res.cGrowC, res.cGrowD);
		TimeCostCurve = (res.cTimeA, res.cTimeB, res.cTimeC, res.cTimeD);
		HackValuCurve = (res.vHackA, res.vHackB, res.vHackC, res.vHackD);
		GrowValuCurve = (res.vGrowA, res.vGrowB, res.vGrowC, res.vGrowD);
		TimeValuCurve = (res.vTimeA, res.vTimeB, res.vTimeC, res.vTimeD);
		BatchSizeLVL = (int)res.HackLvl; MineSpeedLVL = (int)res.GrowLvl; XferDelayLVL = (int)res.TimeLvl;
		HostName = res.HostName; DisplayName = res.DisplayName; IP = res.IP;
		LifeTime = res.LifeTime; _mineralBacklog = res.MineralBacklog; _cycleTimeRemain = Mathf.Min(res.CycleTimeRemain, XferDelay);
		mineralDistribution = [.. res.MineralType.Zip(res.MineralDistribution, (i, d) => (i, d))];
		MAX_LIFE_TIME = res.MAX_LIFE_TIME;
	}
	public static HackFarmDataSaveResource SerializeBotnet(BotFarm obj) {
		return new() {
			cHackA = obj.HackCostCurve.Item1, cHackB = obj.HackCostCurve.Item2, cHackC = obj.HackCostCurve.Item3, cHackD = obj.HackCostCurve.Item4,
			cGrowA = obj.GrowCostCurve.Item1, cGrowB = obj.GrowCostCurve.Item2, cGrowC = obj.GrowCostCurve.Item3, cGrowD = obj.GrowCostCurve.Item4,
			cTimeA = obj.TimeCostCurve.Item1, cTimeB = obj.TimeCostCurve.Item2, cTimeC = obj.TimeCostCurve.Item3, cTimeD = obj.TimeCostCurve.Item4,

			vHackA = obj.HackValuCurve.Item1, vHackB = obj.HackValuCurve.Item2, vHackC = obj.HackValuCurve.Item3, vHackD = obj.HackValuCurve.Item4,
			vGrowA = obj.GrowValuCurve.Item1, vGrowB = obj.GrowValuCurve.Item2, vGrowC = obj.GrowValuCurve.Item3, vGrowD = obj.GrowValuCurve.Item4,
			vTimeA = obj.TimeValuCurve.Item1, vTimeB = obj.TimeValuCurve.Item2, vTimeC = obj.TimeValuCurve.Item3, vTimeD = obj.TimeValuCurve.Item4,

			HackLvl = obj.BatchSizeLVL, GrowLvl = obj.MineSpeedLVL, TimeLvl = obj.XferDelayLVL,
			HostName = obj.HostName, DisplayName = obj.DisplayName, IP = obj.IP,
			MineralBacklog = obj.MBacklog, CycleTimeRemain = obj.CycleTimeRemain, LifeTime = obj.LifeTime,
			MineralType = obj.mineralDistribution.Select(x => x.Item1).ToArray(),
			MineralDistribution = obj.mineralDistribution.Select(x => x.Item2).ToArray(),
			MAX_LIFE_TIME = obj.MAX_LIFE_TIME
		};
	}
}