using Godot;
using System.Linq;

public partial class HackFarmDataSaveResource : Resource {
    [Export] public double cHackA, cHackB, cHackC, cHackD;
    [Export] public double cGrowA, cGrowB, cGrowC, cGrowD;
    [Export] public double cTimeA, cTimeB, cTimeC, cTimeD;
    [Export] public double vHackA, vHackB, vHackC, vHackD;
    [Export] public double vGrowA, vGrowB, vGrowC, vGrowD;
    [Export] public double vTimeA, vTimeB, vTimeC, vTimeD;
    [Export] public double HackLvl, GrowLvl, TimeLvl;
    [Export] public string HostName, DisplayName, IP;
    [Export] public double MineralBacklog, CycleTimeRemain, LifeTime;
    [Export] public double[] MineralDistribution;
    [Export] public int[] MineralType;
    [Export] public double MAX_LIFE_TIME;
}