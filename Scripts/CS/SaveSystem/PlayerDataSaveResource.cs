using Godot;

public partial class PlayerDataSaveResource : Resource {
    [Export] public string username = "UN1NTIALiZED_USER";
    [Export] public long GC_cur = 0;
    [Export] public long GC_max = long.MaxValue; // Deperacated
    [Export] public long[] MineralInventory = new long[76];
    [Export] public bool CompletedTutorial = false;
}