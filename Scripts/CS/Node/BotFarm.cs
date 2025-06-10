using Godot;
using System.Collections.Generic;
using System.Linq;

public class BotFarm {
	const double baseHack = 10.1, baseGrow = .1, baseTime = 60.0;
	const double MAX_LVL = 64;
	public (double, double, double, double) HackCostCurve { get; init; }
    public (double, double, double, double) GrowCostCurve { get; init; }
    public (double, double, double, double) TimeCostCurve { get; init; }
	double _hackVal = baseHack, _growVal = baseGrow, _timeVal = baseTime;
	public double HackVal {
		get => _hackVal;
		private set {
			_hackVal = value;
		}
	}
	public double GrowVal {
		get => _growVal;
		private set {
			_growVal = value;
		}
	}
	public double TimeVal {
		get => _timeVal;
		private set {
			_timeVal = value;
		}
	}
	int _hackLvl = 1, _growLvl = 1, _timeLvl = 1;
	public int HackLVL {
		get => _hackLvl;
		private set {
			_hackLvl = value; _hackVal = GetVal(HackCostCurve, _hackLvl);
		}
	}
	public int GrowLVL {
		get => _growLvl;
		private set {
			_growLvl = value; _growVal = GetVal(GrowCostCurve, _growLvl);
		}
	}
	public int TimeLVL {
		get => _timeLvl;
		private set {
			_timeLvl = value; _timeVal = GetVal(TimeCostCurve, 64 - _timeLvl);
		}
	}
	static double GetVal((double, double, double, double) c, double x) => c.Item1 * Mathf.Pow(x, 3) + c.Item2 * Mathf.Pow(x, 2) + c.Item3 * Mathf.Pow(x, 1) + c.Item4 * Mathf.Pow(x, 0);

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
		List<int> mineralType = []; for (int i = Mathf.Max(defLvl - 2, 1); i <= Mathf.Min(10, defLvl + 1); ++i) { mineralType.Add(i); }
		mineralType.Remove(defLvl); mineralType = Util.Shuffle<int>(mineralType);
		List<(int, double)> cacheResult = [];
		double weight1 = Mathf.Clamp(.02 * GD.RandRange(20, 60), .4, 1.0);
		cacheResult.Add((defLvl, weight1));
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

		HackCostCurve = (RandRange(1e-4, 2e-4), RandRange(1e-3, 5e-3), RandRange(3.0, 4.0), RandRange(5.0, 12.0));
		GrowCostCurve = (RandRange(1e-3, 2.5e-3), RandRange(0.1, 0.2), RandRange(0.0, 1.0), RandRange(3.0, 4.0));
		TimeCostCurve = (RandRange(1e-5, 1e-4), RandRange(1e-3, 1e-2), RandRange(1e-3, 1e-2), 0.05);
		double RandRange(double min, double max) => GD.Randf() * (max - min) + min;
	}
	~BotFarm() {
		if (Util.HaveFinalWord)
			GD.Print($"HackFarm {HostName} is being destroyed");
	}
	public void UpgradeHack() {
		HackLVL += 1;
	}
	public void UpgradeGrow() {
		GrowLVL += 1;
	}
	public void UpgradeTime() {
		TimeLVL += 1;
	}
	double _mineralBacklog, _cycleTimeRemain;
	public double MBacklog { get => _mineralBacklog; }
    public double CycleTimeRemain { get => _cycleTimeRemain; }
    public double[] ProcessMinerals(double delta) {
		double[] output = new double[mineralDistribution.Length];
		_mineralBacklog += _growVal * delta; _cycleTimeRemain -= delta;
		if (_cycleTimeRemain <= 0) {
			_cycleTimeRemain = _timeVal;
			_mineralBacklog += _hackVal;
		}
		for (int i = 0; i < mineralDistribution.Length; ++i) {
			output[i] = _mineralBacklog * mineralDistribution[i].Item2;
			_mineralBacklog -= output[i];
		}
		LifeTime -= delta;
		return output;
	}
	public BotFarm(HackFarmDataSaveResource res) {
        HackCostCurve = (res.HackA, res.HackB, res.HackC, res.HackD);
        GrowCostCurve = (res.GrowA, res.GrowB, res.GrowC, res.GrowD);
        TimeCostCurve = (res.TimeA, res.TimeB, res.TimeC, res._timeD);
        HackLVL = (int)res.HackLvl; GrowLVL = (int)res.GrowLvl; TimeLVL = (int)res.TimeLvl;
        HostName = res.HostName; DisplayName = res.DisplayName; IP = res.IP;
        LifeTime = res.LifeTime; _mineralBacklog = res.MineralBacklog; _cycleTimeRemain = res.CycleTimeRemain;
        mineralDistribution = [.. res.MineralType.Zip(res.MineralDistribution, (i, d) => (i, d))];
		MAX_LIFE_TIME = res.MAX_LIFE_TIME;
    }
    public static HackFarmDataSaveResource SerializeBotnet(BotFarm obj) {
		return new() {
            HackA = obj.HackCostCurve.Item1, HackB = obj.HackCostCurve.Item2, HackC = obj.HackCostCurve.Item3, HackD = obj.HackCostCurve.Item4,
            GrowA = obj.GrowCostCurve.Item1, GrowB = obj.GrowCostCurve.Item2, GrowC = obj.GrowCostCurve.Item3, GrowD = obj.GrowCostCurve.Item4,
            TimeA = obj.TimeCostCurve.Item1, TimeB = obj.TimeCostCurve.Item2, TimeC = obj.TimeCostCurve.Item3, _timeD = obj.TimeCostCurve.Item4,
            HackLvl = obj.HackLVL, GrowLvl = obj.GrowLVL, TimeLvl = obj.TimeLVL,
            HostName = obj.HostName, DisplayName = obj.DisplayName, IP = obj.IP,
            MineralBacklog = obj.MBacklog, CycleTimeRemain = obj.CycleTimeRemain, LifeTime = obj.LifeTime,
            MineralType = obj.mineralDistribution.Select(x => x.Item1).ToArray(),
            MineralDistribution = obj.mineralDistribution.Select(x => x.Item2).ToArray(),
            MAX_LIFE_TIME = obj.MAX_LIFE_TIME
        };
	}
}