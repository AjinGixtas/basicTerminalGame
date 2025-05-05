using Godot;
using System;
using System.Collections.Generic;

public class LockSystem { 
    readonly List<Lock> locks, activeLocks = [];
    double startEpoch = 0, endEpoch = 0;
    public LockSystem(List<Lock> locks) {
        this.locks = locks;
    }
    public void ActivateLock(int secLvl) {
        activeLocks.Clear(); int usedLvl = 0;
        for (int i = 0; i < locks.Count; i++) {
            if (usedLvl + locks[i].Cost <= secLvl && locks[i].MinLvl <= secLvl) {
                activeLocks.Add(locks[i]); 
                usedLvl += locks[i].Cost;
            }
        }
    }
    public int BeginCrack() {
        if (Time.GetUnixTimeFromSystem() < endEpoch) { return 1; }
        startEpoch = Time.GetUnixTimeFromSystem();
        endEpoch = startEpoch + 90;
        for (int i = 0; i < activeLocks.Count; i++) {
            activeLocks[i].Intialize();
        }
        return 0;
    }
    public Tuple<bool, string> CrackAttempt(Dictionary<string, string> ans) {
        for (int i = 0; i < activeLocks.Count; i++) {
            if (ans.TryGetValue(activeLocks[i].Flag, out string key)) {
                if (key == activeLocks[i].Flag) return new(false, $"[color=red]Missing key {activeLocks[i].Flag}. {activeLocks[i].Question}[/color]");
                if (!activeLocks[i].UnlockAttempt(key)) return new(false, $"[color=red]Incorrect key {activeLocks[i].Flag}. {activeLocks[i].Question}[/color]");
                continue;
            } else { return new(false, $"[color=red]Missing flag {activeLocks[i].Flag}.[/color]"); }
        }
        return new(true, "[color=green]Success.[/color]");
    }
}
public class HackFarm {
    // First is the base value, second is the cost of the upgrade
    int _hackLvl = 1; double CurHack; double BaseHack { get; init; } double BaseCostHack { get; init; }
    int _timeLvl = 1; double CurTime; double BaseTime { get; init; } double BaseCostTime { get; init; }
    int _growLvl = 1; double CurGrow; double BaseGrow { get; init; } double BaseCostGrow { get; init; }
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
        BaseHack = .01;
        BaseTime = 20;
        BaseGrow = 3;
        BaseCostHack = 30;
        BaseCostTime = 30;
        BaseCostGrow = 30;
        this.HackLevel = HackLevel; this.TimeLevel = TimeLevel; this.GrowLevel = GrowLevel;
    }
    double currencyPile = 0, timeRemain = 0;
    public double Update(double delta) {
        // Time related mechanism
        timeRemain -= delta; currencyPile += CurGrow * delta;
        // Hack related mechanism
        if (timeRemain > 0) return 0;
        double hackAmount = CurHack * currencyPile;
        currencyPile -= hackAmount;
        return hackAmount; // Return the amount of money hacked
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