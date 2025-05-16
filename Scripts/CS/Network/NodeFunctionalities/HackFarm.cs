using System;

public class HackFarm {
    const int MAX_LVL = 255;
    const double BASE_HACK = 12, POWR_HACK = 1.01, SHIF_HACK = 0, BASE_COST_HACK = 35;
    const double BASE_TIME = 60, POWR_TIME = Math.E, SHIF_TIME = .065, BASE_COST_TIME = 30;
    const double BASE_GROW = .2, POWR_GROW = 6.7, SHIF_GROW = 2.4, BASE_COST_GROW = 32;
    double HackFactor { get; init; } // GC transfer to host per "hack"
    double TimeFactor { get; init; } // Interval between "hacks"
    double GrowFactor { get; init; } // GC create per second
    int _hackLvl = -1; double _curHack; public double CurHack { get => _curHack; set => _curHack = value; }
    int _timeLvl = -1; double _curTime; public double CurTime { get => _curTime; set => _curTime = value; }
    int _growLvl = -1; double _curGrow; public double CurGrow { get => _curGrow; set => _curGrow = value; }
    public int HackLvl {
        get => _hackLvl; private set {
            if (_hackLvl == value) return; _hackLvl = value;
            CurHack = GetHackValue(_hackLvl);
        }
    }
    public int TimeLvl {
        get => _timeLvl; private set {
            if (_timeLvl == value) return; _timeLvl = value;
            CurTime = GetTimeValue(_timeLvl);
        }
    }
    public int GrowLvl {
        get => _growLvl; private set {
            if (_growLvl == value) return; _growLvl = value;
            CurGrow = GetGrowValue(_growLvl);
        }
    }
    public HackFarm(double indexRatio, double depthRatio, int HackLevel = 1, int TimeLevel = 1, int GrowLevel = 1) {
        double MAX_GROW = 1_000 * 0.7958800173440752 * Math.Log10(indexRatio + .6);
        double MIN_TIME = 0.05;
        double MAX_HACK = MAX_GROW * MIN_TIME;
        HackFactor = +(MAX_HACK - BASE_HACK) / Math.Pow(POWR_HACK, MAX_LVL);
        TimeFactor = (BASE_TIME - MIN_TIME + SHIF_TIME * MAX_LVL) / Math.Log2(MAX_LVL);
        GrowFactor = +(MAX_GROW - BASE_GROW - SHIF_GROW * MAX_LVL) / Math.Pow(MAX_LVL, POWR_GROW);

        //this.HackLvl = HackLevel; this.TimeLvl = TimeLevel; this.GrowLvl = GrowLevel;
        this.HackLvl = 255; this.TimeLvl = 255; this.GrowLvl = 255;
        timeRemain = CurTime;
    }
    double currencyPile = 0, timeRemain = 0;
    public double CurrencyPile { get => currencyPile; }
    public double Process(double delta) {
        // Time related mechanism
        timeRemain -= delta; currencyPile += CurGrow * delta;
        // Hack related mechanism
        if (timeRemain > 0) return 0;
        double totalHackAmount = 0;
        while (timeRemain < 0) {
            double hackedAmount = Math.Max(currencyPile, CurHack);
            currencyPile -= hackedAmount;
            totalHackAmount += hackedAmount;
            timeRemain += CurTime;
        }
        return totalHackAmount; // Return the amount of money hacked
    }

    public void UpgradeHackLevel() { ++HackLvl; }
    public void UpgradeTimeLevel() { ++TimeLvl; }
    public void UpgradeGrowLevel() { ++GrowLvl; }
    public double GetHackValue(int level) { return BASE_HACK + HackFactor * Math.Pow(POWR_HACK, level) + .01; } // Add a hard coded amount to account for floaty error
    public double GetTimeValue(int level) { return BASE_TIME - TimeFactor * Math.Log2(level) + SHIF_TIME * level; }
    public double GetGrowValue(int level) { return BASE_GROW + GrowFactor * Math.Pow(level, POWR_GROW) + level * SHIF_GROW; }
    public double GetHackCost(int level) { return BASE_COST_HACK * Math.Pow(1.10, level) + 44 * Math.Pow(level, 2.30); }
    public double GetTimeCost(int level) { return BASE_COST_TIME * Math.Pow(1.09, level) + 30 * Math.Pow(level, 2.45); }
    public double GetGrowCost(int level) { return BASE_COST_GROW * Math.Pow(1.08, level) + 25 * Math.Pow(level, 2.50); }
}