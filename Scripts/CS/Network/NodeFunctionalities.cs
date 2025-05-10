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
    // 0 - Success; 1 - Missing flag; 2 - Missing key; 3 - Incorrect key; 4 - Timeout
    public int CrackAttempt(Dictionary<string, string> ans) {
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
                        if (activeLocks[i].Clue.Length > 0) { TerminalProcessor.Say($"[color={Util.CC(Cc.rgb)}]{activeLocks[i].Clue}"); }
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
    // First is the base value, second is the cost of the upgrade
    double BaseHack { get; init; } double BaseCostHack { get; init; }
    double BaseTime { get; init; } double BaseCostTime { get; init; }
    double BaseGrow { get; init; } double BaseCostGrow { get; init; }
    int _hackLvl = 1; double _curHack; public double CurHack { get => _curHack; set => _curHack = value; } 
    int _timeLvl = 1; double _curTime; public double CurTime { get => _curTime; set => _curTime = value; } 
    int _growLvl = 1; double _curGrow; public double CurGrow { get => _curGrow; set => _curGrow = value; } 
    public int HackLevel {
        get => _hackLvl; private set {
            if (_hackLvl == value) return;
            _hackLvl = value;
            CurHack = BaseHack * Math.Pow(1.02, _hackLvl);
        }
    }
    public int TimeLevel {
        get => _timeLvl; private set {
            if (_timeLvl == value) return;
            _timeLvl = value;
            CurTime = BaseTime * Math.Pow(.98, _timeLvl);
        }
    }
    public int GrowLevel {
        get => _growLvl; private set {
            if (_growLvl == value) return;
            _growLvl = value;
            CurGrow = BaseGrow * Math.Pow(1.07, _growLvl);
        }
    }
    public HackFarm(int secLvl, double indexRatio, double depthRatio, int HackLevel=1, int TimeLevel=1, int GrowLevel=1) {
        BaseHack = 8; BaseCostHack = 30;
        BaseTime = 20; BaseCostTime = 30;
        BaseGrow = 3; BaseCostGrow = 30;
        this.HackLevel = GD.RandRange(1, 100); this.TimeLevel = GD.RandRange(1, 100); this.GrowLevel = GD.RandRange(1, 100);
        //this.HackLevel = HackLevel; this.TimeLevel = TimeLevel; this.GrowLevel = GrowLevel;
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
    public void UpgradeHackLevel() { ++HackLevel; }
    public void UpgradeTimeLevel() { ++TimeLevel; }
    public void UpgradeGrowLevel() { ++GrowLevel; }
    public double GetHackCost(int level) { return BaseCostHack * Math.Pow(1.02, level); }
    public double GetTimeCost(int level) { return BaseCostTime * Math.Pow(1.02, level); }
    public double GetGrowCost(int level) { return BaseCostGrow * Math.Pow(1.02, level); }
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