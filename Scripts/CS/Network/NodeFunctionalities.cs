using Godot;
using System;
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
                        TerminalProcessor.Say($"[{Util.Format("N0VALUE", StrType.ERROR)}] Denied access by {Util.Format(activeLocks[i].Name, StrType.CMD_ARG)}");
                        TerminalProcessor.Say("-r", $"Missing key for {Util.Format(flags[j], StrType.CMD_ARG)}");
                        return 2;
                    } else if (!activeLocks[i].UnlockAttempt(key, j)) {
                        TerminalProcessor.Say($"[{Util.Format("WRON6KY", StrType.ERROR)}] Denied access by {Util.Format(activeLocks[i].Name, StrType.CMD_ARG)}");
                        TerminalProcessor.Say("-r", $"Incorrect key for {Util.Format(flags[j], StrType.CMD_ARG)}");
                        if (activeLocks[i].Clue.Length > 0) { TerminalProcessor.Say($"[color={Util.CC(Cc.rgb)}]{activeLocks[i].Inp}"); }
                        return 3;
                    }
                    continue;
                } else {
                    TerminalProcessor.Say($"[{Util.Format("MI55ING", StrType.ERROR)}] Denied access by {Util.Format(activeLocks[i].Name, StrType.CMD_ARG)}");
                    TerminalProcessor.Say("-r", $"Missing flag {Util.Format(flags[j], StrType.CMD_ARG)}");
                    return 1;
                }
            }
            TerminalProcessor.Say($"[{Util.Format("SUCCESS", StrType.PARTIAL_SUCCESS)}] {Util.Format("Bypassed", StrType.DECOR)} {Util.Format(activeLocks[i].Name, StrType.CMD_ARG)}");
        }
        lockPool = []; activeLocks = []; // Destroy all security system
        TerminalProcessor.Say($"{Util.Format("Node defense cracked.", StrType.FULL_SUCCESS)} All security system [color={Util.CC(Cc.gR)}]destroyed[/color].");
        TerminalProcessor.Say($"Run {Util.Format("analyze", StrType.CMD)} for all new information.");
        return 0;
    }
}
public class HackFarm {
    const int MAX_LVL = 255;
    const double BASE_HACK = 12, POWR_HACK = 1.1,    SHIF_HACK = 1.4, BASE_COST_HACK = 35;
    const double BASE_TIME = 60, POWR_TIME = Math.E, SHIF_TIME = .06, BASE_COST_TIME = 30;
    const double BASE_GROW = .2, POWR_GROW = 6.75,   SHIF_GROW = 2.4, BASE_COST_GROW = 32;
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
    public HackFarm(double indexRatio, double depthRatio, int HackLevel=1, int TimeLevel=1, int GrowLevel=1) {
        double MAX_GROW = 1_000_000_000 / indexRatio;
        double MIN_TIME = 0.05;
        double MAX_HACK = MAX_GROW * MIN_TIME;
        HackFactor = + (MAX_HACK - BASE_HACK - SHIF_HACK * MAX_LVL) / Math.Pow(POWR_HACK, MAX_LVL);
        TimeFactor = - (MIN_TIME - BASE_TIME - SHIF_TIME * MAX_LVL) / Math.Log(MAX_LVL, POWR_TIME);
        GrowFactor = + (MAX_GROW - BASE_GROW - SHIF_GROW * MAX_LVL) / Math.Pow(MAX_LVL, POWR_GROW);
     
        this.HackLvl = HackLevel; this.TimeLvl = TimeLevel; this.GrowLvl = GrowLevel;
        GD.Print(GetHackValue(1), ' ', GetHackValue(3), ' ', GetHackValue(5), ' ', GetHackValue(100));
        //this.HackLevel = GD.RandRange(1, 100); this.TimeLevel = GD.RandRange(1, 100); this.GrowLevel = GD.RandRange(1, 100);
    }
    double currencyPile = 0, timeRemain = 0;
    public double Update(double delta) {
        // Time related mechanism
        timeRemain -= delta; currencyPile += CurGrow * delta;
        // Hack related mechanism
        if (timeRemain > 0) return 0;
        double totalHackAmount = 0;
        while (timeRemain < 0) {
            double hackedAmount = currencyPile * CurHack;
            currencyPile -= hackedAmount;
            totalHackAmount += hackedAmount;
            timeRemain += CurTime;
        }
        return totalHackAmount; // Return the amount of money hacked
    }
    
    public void UpgradeHackLevel() { ++HackLvl; }
    public void UpgradeTimeLevel() { ++TimeLvl; }
    public void UpgradeGrowLevel() { ++GrowLvl; }
    public double GetHackValue(int level) { return BASE_HACK + HackFactor * Math.Pow(POWR_HACK, level) + SHIF_HACK * level; }
    public double GetTimeValue(int level) { return BASE_TIME - TimeFactor * Math.Log(level, POWR_TIME) - SHIF_TIME * level; }
    public double GetGrowValue(int level) { return BASE_GROW + GrowFactor * Math.Pow(level, POWR_GROW) + SHIF_GROW * level; }
    public double GetHackCost(int level) { return BASE_COST_HACK * Math.Pow(1.10, level) + 44 * Math.Pow(level, 2.30); }
    public double GetTimeCost(int level) { return BASE_COST_TIME * Math.Pow(1.09, level) + 30 * Math.Pow(level, 2.45); }
    public double GetGrowCost(int level) { return BASE_COST_GROW * Math.Pow(1.08, level) + 25 * Math.Pow(level, 2.50); }
}
public class Stock {
    public string Name { get; init; }
    public double Price { get; private set; }
    public double PastPrice { get; private set; }
    public double Drift { get; private set; }
    public double Volatility { get; private set; }
    public Stock(string Name, double Price, double Drift, double Volatility, double PastPrice) {
        this.Name = Name; this.Price = Price; this.Drift = Drift; this.Volatility = Volatility; this.PastPrice = PastPrice;
    }
    public void UpdateStock(double delta) {
        PastPrice = Price;
        // Component 1: Δt(μ−.5σ^2)
        double a = (Drift - .5 * Math.Pow(Volatility, 2.0)) * delta;
        // Component 2: σ⋅Δt^.5⋅Z
        double b = Volatility * Math.Pow(delta, .5) * GD.Randfn(0, 1);
        // Δt: delta; μ: Drift; σ: Volatility; Z: GD.Randfn(0, 1)
        Price *= Math.Pow(Math.E, a + b);
    }
}
public class Faction {
    public string Name { get; init; }
    public string Desc { get; init; }
    public int Favor { get; private set; }
    public int Reputation { get; init; }
    public Faction(string Name, string Desc, int Favor, int Reputation) {
        this.Name = Name; this.Desc = Desc;
        this.Favor = Favor; this.Reputation = Reputation;
    }
}