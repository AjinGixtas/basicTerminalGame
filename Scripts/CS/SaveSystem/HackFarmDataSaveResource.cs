using Godot;
using System.Linq;

public partial class HackFarmDataSaveResource : Resource {
    [Export] public double HackA, HackB, HackC, HackD;
    [Export] public double GrowA, GrowB, GrowC, GrowD;
    [Export] public double TimeA, TimeB, TimeC, TimeD;
    [Export] public double HackLvl, GrowLvl, TimeLvl;
    [Export] public string HostName, DisplayName, IP;
    [Export] public double MineralBacklog, CycleTimeRemain, LifeTime;
    [Export] public double[] MineralDistribution;
    [Export] public int[] MineralType;
    [Export] public double MAX_LIFE_TIME;
}