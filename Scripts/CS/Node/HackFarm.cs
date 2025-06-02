using Godot;
using System.Collections.Generic;

public class HackFarm {
    const double baseHack = 10.1, baseGrow = .1, baseTime = 60.0;
    const double upgradeHack = 1.02469507659596, upgradeGrow = 1.01, upgradeTime = 1.02469507659596;
    
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
            _hackLvl = value;
            _hackVal = baseHack * Mathf.Pow(upgradeHack, HackLVL);
        }
    }
    public int GrowLVL {
        get => _growLvl;
        private set {
            _growLvl = value;
            _growVal = baseGrow * Mathf.Pow(upgradeGrow, GrowLVL);
        }
    }
    public int TimeLVL {
        get => _timeLvl;
        private set {
            _timeLvl = value;
            _timeVal = baseTime / Mathf.Pow(upgradeTime, TimeLVL);
        }
    }

    public (int, double)[] mineralDistribution { get; init; }
    public string HostName { get; init; }
    public string DisplayName { get; init; }
    public string IP { get; init; }
    double _lifetime = 0;
    public double LifeTime { get => _lifetime;
        private set {
            _lifetime = value;
            if (_lifetime < 0) { NetworkManager.QueueRemoveHackFarm(this); }
        }
    }
    public HackFarm(int defLvl, DriftNode driftNode) {
        List<int> mineralType = []; for(int i = Mathf.Max(defLvl-2, 1); i <= Mathf.Min(10, defLvl+1); ++i) { mineralType.Add(i); }
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
        timeRemains = GD.RandRange(10800.0, 86400) + 3600 * Mathf.Pow(Mathf.E/2, 0.763891 * defLvl) * Mathf.Log(defLvl);
    }
    ~HackFarm() {
        if (Util.haveFinalWord)
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
    double mineralDeposit, timeRemains;
    public double[] ProcessMinerals(double delta) {
        double[] output = new double[mineralDistribution.Length];
        mineralDeposit += _growVal * delta; timeRemains -= delta;
        if (timeRemains <= 0) {
            timeRemains = _timeVal;
            mineralDeposit += _hackVal;
        }
        for (int i = 0; i < mineralDistribution.Length; ++i) {
            output[i] = mineralDeposit * mineralDistribution[i].Item2;
            mineralDeposit -= output[i];
        }
        LifeTime -= delta;
        return output;
    }
}