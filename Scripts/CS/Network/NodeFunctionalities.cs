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
    int _hackLvl = 1, _timeLvl = 1, _growLvl = 1;
    double InitialHackPercent { get; init; }
    double InitialHackTime { get; init; }
    double InitialGrowRate { get; init; }
    public int HackLevel { get => _hackLvl; private set => _hackLvl = value; }
    public int TimeLevel { get => _timeLvl; private set => _timeLvl = value; }
    public int GrowLevel { get => _growLvl; private set => _growLvl = value; }
    public double HackPercent { get; private set; }
    public double HackTime { get; private set; }
    public double GrowRate { get; private set; }
    public HackFarm(int secLvl, double indexRatio, double depthRatio) {
        InitialHackPercent = 11.0 - Math.Pow(Math.E, Math.PI * secLvl * 0.0588235294118);
        InitialHackTime = (10.0 + 4.32 * secLvl + Math.Pow(1.23374, depthRatio)) * .5;
        InitialGrowRate = 128.0 - Math.Pow(Math.PI * .97, secLvl * 0.309214594929) / indexRatio;
    }
    // Method to calculate the hack time
    public void CalcHackTime() {
        // Example formula for hack time: time decreases as level increases
        HackTime = 100 - (10 * _timeLvl);  // Hack time reduces by 10 for each level
        if (HackTime < 20) HackTime = 20; // Minimum hack time is capped at 20
    }

    // Method to calculate the hack percent
    public void CalcHackPercent() {
        // Example formula for hack percent: increase by 5% per level
        HackPercent = 10 + (5 * _hackLvl);  // Hack percent starts at 10% and increases by 5% per level
    }

    // Method to calculate the growth rate
    public void CalcGrowRate() {
        // Example formula for grow rate: growth increases by 2% per level
        GrowRate = 1 + (0.02 * _growLvl);  // Starting at 1, and increases by 2% per level
    }

    // Method to calculate the cost for upgrading hack time
    public void CalcHackTimeUpgradeCost() {
        // Example formula for hack time upgrade cost (exponential growth for cost)
        double upgradeCost = 50 * Math.Pow(1.2, _timeLvl);  // Example: 50 * (1.2 ^ level)
        Console.WriteLine($"Upgrade Cost for Hack Time: {upgradeCost}");
    }

    // Method to calculate the cost for upgrading hack percent
    public void CalcHackPercentUpgradeCost() {
        // Example formula for hack percent upgrade cost
        double upgradeCost = 100 * Math.Pow(1.3, _hackLvl);  // Example: 100 * (1.3 ^ level)
        Console.WriteLine($"Upgrade Cost for Hack Percent: {upgradeCost}");
    }

    // Method to calculate the cost for upgrading grow rate
    public void CalcGrowRateUpgradeCost() {
        // Example formula for grow rate upgrade cost
        double upgradeCost = 80 * Math.Pow(1.25, _growLvl);  // Example: 80 * (1.25 ^ level)
        Console.WriteLine($"Upgrade Cost for Grow Rate: {upgradeCost}");
    }
}
public class Stock {
    public string Name { get; init; }
    public double Price { get; private set; }
    public double PastPrice { get; private set; }
    public double Drift { get; private set; }
    public double Volatility { get; private set; }
    public Stock(string Name, double Price, double Drift, double Volatility) {
        this.Name = Name; this.Price = Price; this.Drift = Drift; this.Volatility = Volatility;
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
    public Faction(string Name, string Desc) {
        this.Name = Name; this.Desc = Desc;
        this.Favor = 0; this.Reputation = 0;
    }
}