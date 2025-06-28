using Godot;
public partial class CraftableItemData : ItemData {
    public Ingredient[] RequiredIngredients { get; init; }
    public double CraftTime { get; init; }
    const double CRAFT_TIME_MULT = 10;
    public CraftableItemData(string name, string shorthand, int id, Cc colorCode, double craftTime, double value, Ingredient[] requiredIngredients) 
        : base(name, shorthand, id, value, colorCode) {
        CraftTime = craftTime * CRAFT_TIME_MULT; RequiredIngredients = requiredIngredients;
    }
}