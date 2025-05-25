using Godot;

public class HackFarm {
    const int MAX_LVL = 255;
    const double BASE_HACK = 12, POWR_HACK = 1.01, SHIF_HACK = 0, BASE_COST_HACK = 35;
    const double BASE_TIME = 60, POWR_TIME = Mathf.E, SHIF_TIME = .065, BASE_COST_TIME = 30;
    const double BASE_GROW = .2, POWR_GROW = 6.7, SHIF_GROW = 2.4, BASE_COST_GROW = 32;
    double HackFactor { get; init; } // GC transfer to host per "hack"
    double TimeFactor { get; init; } // Interval between "hacks"
    double GrowFactor { get; init; } // GC create per second
    int _hackLvl = -1; double _curHack; public double CurHack { get => _curHack; set => _curHack = value; }
    int _timeLvl = -1; double _curTime; public double CurTime { get => _curTime; set => _curTime = value; }
    int _growLvl = -1; double _curGrow; public double CurGrow { get => _curGrow; set => _curGrow = value; }
    public int HackLvl {
        get => _hackLvl; private set {
            value = Mathf.Clamp(value, 1, 255);
            if (_hackLvl == value) return; _hackLvl = value;
            CurHack = GetHackValue(_hackLvl) + .05; // Add a hard coded amount to account for floaty error
        }
    }
    public int TimeLvl {
        get => _timeLvl; private set {
            value = Mathf.Clamp(value, 1, 255);
            if (_timeLvl == value) return; _timeLvl = value;
            CurTime = GetTimeValue(_timeLvl);
        }
    }
    public int GrowLvl {
        get => _growLvl; private set {
            value = Mathf.Clamp(value, 1, 255);
            if (_growLvl == value) return; _growLvl = value;
            CurGrow = GetGrowValue(_growLvl);
        }
    }
    public HackFarm(double MAX_GROW, int HackLvl = 1, int TimeLvl = 1, int GrowLvl = 1, MiningWeight[] miningWeights = null) {
        double MIN_TIME = 0.05;
        double MAX_HACK = MAX_GROW * MIN_TIME;
        HackFactor = (MAX_HACK - BASE_HACK) / Mathf.Pow(POWR_HACK, MAX_LVL);
        TimeFactor = (BASE_TIME - MIN_TIME + SHIF_TIME * MAX_LVL) / (Mathf.Log(MAX_LVL) / Mathf.Log(2));
        GrowFactor = (MAX_GROW - BASE_GROW - SHIF_GROW * MAX_LVL) / Mathf.Pow(MAX_LVL, POWR_GROW);

        //this.HackLvl = HackLevel; this.TimeLvl = TimeLevel; this.GrowLvl = GrowLevel;
        this.HackLvl = HackLvl; this.TimeLvl = TimeLvl; this.GrowLvl = GrowLvl;
        timeRemain = CurTime;
        MiningWeights = miningWeights ?? [];
    }
    public MiningWeight[] MiningWeights { get; set; }
    double currencyPile = 0, timeRemain = 0;
    public double CurrencyPile { get => currencyPile; }
    public double[] ProcessMinerals(double delta) {
        timeRemain -= delta; currencyPile += CurGrow * delta;
        if (timeRemain > 0) return new double[MiningWeights.Length];
        double totalHackAmount = 0;
        while (timeRemain < 0) {
            double hackedAmount = Mathf.Min(currencyPile, CurHack);
            currencyPile -= hackedAmount;
            totalHackAmount += hackedAmount;
            timeRemain += CurTime;
        }
        // Distribute totalHackAmount according to MiningWeights
        double[] minerals = new double[MiningWeights.Length];
        for (int i = 0; i < MiningWeights.Length; ++i) {
            minerals[i] = totalHackAmount * MiningWeights[i].weight;
        }
        return minerals;
    }

    public int UpgradeHackLevel() {
        int status = PlayerDataManager.WithdrawGC(GetHackCost(HackLvl+1));
        if (status != 0) return status;
        if (HackLvl >= MAX_LVL) return 3; // Already at max level
        ++HackLvl; return 0;
    }
    public int UpgradeTimeLevel() {
        int status = PlayerDataManager.WithdrawGC(GetTimeCost(TimeLvl+1));
        if (status != 0) return status;
        if (TimeLvl >= MAX_LVL) return 3; // Already at max level
        ++TimeLvl; return 0;
    }
    public int UpgradeGrowLevel() {
        int status = PlayerDataManager.WithdrawGC(GetGrowCost(GrowLvl));
        if (status != 0) return status;
        if (GrowLvl >= MAX_LVL) return 3; // Already at max level
        ++GrowLvl; return 0;
    }
    public double GetHackValue(int level) { return BASE_HACK + HackFactor * Mathf.Pow(POWR_HACK, level); } 
    public double GetTimeValue(int level) { return BASE_TIME - TimeFactor * Mathf.Log(level) / Mathf.Log(2) + SHIF_TIME * level; }
    public double GetGrowValue(int level) { return (BASE_GROW + GrowFactor * Mathf.Pow(level, POWR_GROW) + level * SHIF_GROW) / 10; }
    public double GetHackCost(int level) { 
        if (level > MAX_LVL) return 0;
        return BASE_COST_HACK * Mathf.Pow(1.10, level) + 44 * Mathf.Pow(level, 2.30);
    }
    public double GetTimeCost(int level) { 
        if (level > MAX_LVL) return 0;
        return BASE_COST_TIME * Mathf.Pow(1.09, level) + 30 * Mathf.Pow(level, 2.45); 
    }
    public double GetGrowCost(int level) {
        if (level > MAX_LVL) return 0;
        return BASE_COST_GROW * Mathf.Pow(1.08, level) + 25 * Mathf.Pow(level, 2.50); 
    }
}