using Godot;

public partial class PlayerDataSaveResource : Resource {
    [Export] public string username = "UN1NTIALiZED_USER";
    [Export] public double GC_cur = 0;
    [Export] public double GC_max = 2_000_000;
    [Export] public double[] MineralInventory = new double[10];
    [Export] public bool CompletedTutorial = false;
}