using Godot;
using System.Collections.Generic;
using System.Linq;

public class BotFarm {
	const double base_BatchSize = 10.1, base_MineSpeed = .1, base_XferDelay = 60.0;
	const double MAX_LVL = 255;
	public (double, double, double, double) BatchSizeCostCurve { get; init; }
	public (double, double, double, double) MineSpeedCostCurve { get; init; }
	public (double, double, double, double) XferDelayCostCurve { get; init; }
	public (double, double, double, double) BatchSizeValuCurve { get; init; }
	public (double, double, double, double) MineSpeedValuCurve { get; init; }
	public (double, double, double, double) XferDelayValuCurve { get; init; }
	double _batchSize = base_BatchSize, _mineSpeed = base_MineSpeed, _xferDelay = base_XferDelay;
	public double BatchSize {
		get => _batchSize;
		private set {
			_batchSize = value;
		}
	}
	public double MineSpeed {
		get => _mineSpeed;
		private set {
			_mineSpeed = value;
		}
	}
	public double XferDelay {
		get => _xferDelay;
		private set {
			_xferDelay = value;
		}
	}
	int _hackLvl = 1, _growLvl = 1, _timeLvl = 1;
	public int BatchSizeLVL {
		get => _hackLvl;
		private set {
			_hackLvl = value; _batchSize = GetValu(BatchSizeValuCurve, _hackLvl); // Use the same curve as cost, just scaled down.
		}
	}
	public int MineSpeedLVL {
		get => _growLvl;
		private set {
			_growLvl = value;
			_mineSpeed =
				GetValu(BatchSizeValuCurve, _growLvl * MineSpeedValuCurve.Item3) /
				GetTime(XferDelayValuCurve, _growLvl * MineSpeedValuCurve.Item4);
		}
	}
	public int XferDelayLVL {
		get => _timeLvl;
		private set {
			_timeLvl = value; _xferDelay = GetTime(XferDelayValuCurve, _timeLvl);
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
	public int DefLvl { get; init; }
	public BotFarm(int defLvl, DriftNode driftNode) {
		DefLvl = defLvl;
        
		// defLvl in [1, 10] range, mineralTier in [0, 9] range.
        mineralDistribution = GenerateMiningWeightDistribution(defLvl-1);
		HostName = driftNode.HostName; DisplayName = driftNode.DisplayName; IP = driftNode.IP;
		MAX_LIFE_TIME = 600 * (.2 + Mathf.Log((defLvl+1)/2.0) + GD.Randf());
		LifeTime = MAX_LIFE_TIME;
		BatchSizeCostCurve = (GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0), GD.RandRange(10.0, 25.0));
		MineSpeedCostCurve = (GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0), GD.RandRange(10.0, 25.0));
		XferDelayCostCurve = (GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0), GD.RandRange(1.0, 10.0), GD.RandRange(10.0, 25.0));
		
		BatchSizeValuCurve = (GD.RandRange(1e-5, 1e-4), GD.RandRange(1e-4, 1e-3), GD.RandRange(1.0, 3.0), GD.RandRange(1.0, 2.0));
		// This is essentially stores the offset of hackValu curve level and timeValu curve level to calc its own value, thus, it will relies on the other 2 curve being accurate.
		MineSpeedValuCurve = (0, 0, GD.RandRange(.8, 1.2), GD.RandRange(.8, 1.2));
		// This one essentially a speed increase algorithm, with v = d*(b^level)+c*level
		// a being the distance
		XferDelayValuCurve = (GD.RandRange(16.0, 24.0), 1.0 + GD.RandRange(.01, .05), GD.RandRange(.1, 1.0), 1.0);
		BatchSizeLVL = MineSpeedLVL = XferDelayLVL = 1;
		CycleTimeRemain = 2.0;
    }
	~BotFarm() {
		if (Util.HaveFinalWord)
			GD.Print($"HackFarm {HostName} is being destroyed");
	}
	public CError UpgradeHack() {
		CError cError = PlayerDataManager.WithdrawGC(GetBatchSizeCost());
		if (cError != CError.OK) return cError;
		BatchSizeLVL += 1; return CError.OK;
	}
	public CError UpgradeGrow() {
		CError cError = PlayerDataManager.WithdrawGC(GetMineSpeedCost());
		if (cError != CError.OK) return cError;
		MineSpeedLVL += 1; return CError.OK;
    }
	public CError UpgradeTime() {
		CError cError = PlayerDataManager.WithdrawGC(GetXferDelayCost());
		if (cError != CError.OK) return cError;
		XferDelayLVL += 1; return CError.OK;
    }

	public long GetBatchSizeCost() => (long)Mathf.Ceil(GetValu(BatchSizeCostCurve, BatchSizeLVL)/10.0);
	public long GetMineSpeedCost() => (long)Mathf.Ceil(GetValu(MineSpeedCostCurve, MineSpeedLVL)/10.0);
	public long GetXferDelayCost() => (long)Mathf.Ceil(GetValu(XferDelayCostCurve, XferDelayLVL)/10.0);
	double _mineralBacklog, _cycleTimeRemain, _transferBacklog;
	public double MBacklog { get => _mineralBacklog; private set => _mineralBacklog = value; }
	public double CycleTimeRemain { get => _cycleTimeRemain; private set => _cycleTimeRemain = value; }
	public (int, long)[] ProcessMinerals(double delta) {
		(int, long)[] output = new (int, long)[mineralDistribution.Length];
		CycleTimeRemain -= delta; LifeTime -= delta;
		MBacklog += MineSpeed * delta;
		if (CycleTimeRemain > 0) { return output; }
		
		_transferBacklog += BatchSize;
		double batch = Mathf.Min(MBacklog, _transferBacklog);
		for (int i = 0; i < mineralDistribution.Length; ++i) {
			output[i] = (mineralDistribution[i].Item1, (long)Mathf.Ceil(mineralDistribution[i].Item2 * batch));
			_transferBacklog -= (long)Mathf.Ceil(mineralDistribution[i].Item2 * batch);
        }
		MBacklog -= batch;
		CycleTimeRemain += XferDelay;

		return output;
	}
	public BotFarm(HackFarmDataSaveResource res) {
		DefLvl = res.DefLvl;
		BatchSizeCostCurve = (res.cHackA, res.cHackB, res.cHackC, res.cHackD);
		MineSpeedCostCurve = (res.cGrowA, res.cGrowB, res.cGrowC, res.cGrowD);
		XferDelayCostCurve = (res.cTimeA, res.cTimeB, res.cTimeC, res.cTimeD);
		BatchSizeValuCurve = (res.vHackA, res.vHackB, res.vHackC, res.vHackD);
		MineSpeedValuCurve = (res.vGrowA, res.vGrowB, res.vGrowC, res.vGrowD);
		XferDelayValuCurve = (res.vTimeA, res.vTimeB, res.vTimeC, res.vTimeD);
		BatchSizeLVL = (int)res.HackLvl; MineSpeedLVL = (int)res.GrowLvl; XferDelayLVL = (int)res.TimeLvl;
		HostName = res.HostName; DisplayName = res.DisplayName; IP = res.IP;
		LifeTime = res.LifeTime; _mineralBacklog = res.MineralBacklog; _cycleTimeRemain = Mathf.Min(res.CycleTimeRemain, XferDelay);
		mineralDistribution = [.. res.MineralType.Zip(res.MineralDistribution, (i, d) => (i, d))];
		MAX_LIFE_TIME = res.MAX_LIFE_TIME;
	}
	public static HackFarmDataSaveResource SerializeBotnet(BotFarm obj) {
		return new() {
			DefLvl = obj.DefLvl,

            cHackA = obj.BatchSizeCostCurve.Item1, cHackB = obj.BatchSizeCostCurve.Item2, cHackC = obj.BatchSizeCostCurve.Item3, cHackD = obj.BatchSizeCostCurve.Item4,
			cGrowA = obj.MineSpeedCostCurve.Item1, cGrowB = obj.MineSpeedCostCurve.Item2, cGrowC = obj.MineSpeedCostCurve.Item3, cGrowD = obj.MineSpeedCostCurve.Item4,
			cTimeA = obj.XferDelayCostCurve.Item1, cTimeB = obj.XferDelayCostCurve.Item2, cTimeC = obj.XferDelayCostCurve.Item3, cTimeD = obj.XferDelayCostCurve.Item4,

			vHackA = obj.BatchSizeValuCurve.Item1, vHackB = obj.BatchSizeValuCurve.Item2, vHackC = obj.BatchSizeValuCurve.Item3, vHackD = obj.BatchSizeValuCurve.Item4,
			vGrowA = obj.MineSpeedValuCurve.Item1, vGrowB = obj.MineSpeedValuCurve.Item2, vGrowC = obj.MineSpeedValuCurve.Item3, vGrowD = obj.MineSpeedValuCurve.Item4,
			vTimeA = obj.XferDelayValuCurve.Item1, vTimeB = obj.XferDelayValuCurve.Item2, vTimeC = obj.XferDelayValuCurve.Item3, vTimeD = obj.XferDelayValuCurve.Item4,

			HackLvl = obj.BatchSizeLVL, GrowLvl = obj.MineSpeedLVL, TimeLvl = obj.XferDelayLVL,
			HostName = obj.HostName, DisplayName = obj.DisplayName, IP = obj.IP,
			MineralBacklog = obj.MBacklog, CycleTimeRemain = obj.CycleTimeRemain, LifeTime = obj.LifeTime,
			MineralType = obj.mineralDistribution.Select(x => x.Item1).ToArray(),
			MineralDistribution = obj.mineralDistribution.Select(x => x.Item2).ToArray(),
			MAX_LIFE_TIME = obj.MAX_LIFE_TIME
		};
	}
	static (int, double)[] GenerateMiningWeightDistribution(int baseLvl) {
		double[] initialRand = [GD.Randf(), GD.Randf(), GD.Randf()]; System.Array.Sort(initialRand);
		double sum = initialRand.Sum();
		double[] randDist = sum == 0 ? [1.0/3.0, 1.0/3.0, 1.0/3.0] : initialRand.Select(x => x / sum).ToArray();

		int swap = GD.Randf() < .5 ? 1 : 0;
        List<(int, double)> result = [(baseLvl-1, randDist[swap]), (baseLvl, randDist[2]), (baseLvl+1, randDist[1-swap])];
        if (baseLvl < 0) result[1] = (baseLvl, result[1].Item2 + result[0].Item2);
		if (baseLvl > 9) result[1] = (baseLvl, result[1].Item2 + result[2].Item2);
        return result.Where(item => item.Item1 >= 0 && item.Item1 <= 9).OrderByDescending(v => v.Item2).ToArray();
    }
}