using Godot;
public partial class ItemRecipe : MineralProfile {
    public int[] RequiredItemID { get; init; }
    public double CraftTime { get; init; }
    public ItemRecipe(string name, string shorthand, int id, Cc colorCode, double craftTime, double value, int[] requiredItemID) 
        : base(name, shorthand, id, value, colorCode) {
        CraftTime = craftTime; RequiredItemID = requiredItemID;
    }
}