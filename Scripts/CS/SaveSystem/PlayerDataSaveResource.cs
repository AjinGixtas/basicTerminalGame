using Godot;
public partial class PlayerDataSaveResource : Resource {
    [Export] public string username;
    [Export] public double GC_total;
    [Export] public double GC_max;
    [Export] public double[] mineralInventory;
}