using Godot;
using System.Collections.Generic;

public class HackFarm {
    const double baseHack = 10.1, baseGrow = .1, baseTime = 60.0;
    const double upgradeHack = 1.02469507659596, upgradeGrow = 1.01, upgradeTime = 1.02469507659596;
    
    double _hackVal = baseHack, _growVal = baseGrow, _timeVal = baseTime;
    int _hackLvl = 1, _growLvl = 1, _timeLvl = 1;
    int HackLVL {
        get => _hackLvl;
        set {
            _hackLvl = value;
            _hackVal = baseHack * Mathf.Pow(upgradeHack, HackLVL);
        }
    }
    int GrowLVL {
        get => _growLvl;
        set {
            _growLvl = value;
            _growVal = baseGrow * Mathf.Pow(upgradeGrow, GrowLVL);
        }
    }
    int TimeLVL {
        get => _timeLvl;
        set {
            _timeLvl = value;
            _timeVal = baseTime * Mathf.Pow(upgradeTime, TimeLVL);
        }
    }

    (int, double)[] mineralDistribution { get; init; }
    public HackFarm(int defLvl) {
        List<int> mineralType = []; for(int i = Mathf.Max(defLvl-2, 1); i <= Mathf.Min(10, defLvl+1); ++i) { mineralType.Add(i); }
        mineralType.Remove(defLvl); mineralType = Util.Shuffle<int>(mineralType);
        
        List<(int, double)> cacheResult = [];
        double weight1 = Mathf.Clamp(GD.Randfn(.7, 1), .4, 1.0);
        cacheResult.Add((defLvl, weight1));
        double remainW = 1.0 - weight1;
        for (int i = 0; i < Mathf.Min(3, mineralType.Count) && 0.0 < remainW; ++i) {
            double W = Mathf.Clamp(GD.Randfn(.3, 1),  .2, remainW);
            cacheResult.Add((mineralType[i], W));
            remainW -= W;
        }
        cacheResult[0] = (cacheResult[0].Item1, cacheResult[1].Item2 + remainW);
        mineralDistribution = [.. cacheResult];
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
        return output;
    }
}