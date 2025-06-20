using Godot;
public partial class ItemRecipe : MineralProfile {
    public Ingredient[] RequiredIngredients { get; init; }
    public double CraftTime { get; init; }
    public ItemRecipe(string name, string shorthand, int id, Cc colorCode, double craftTime, double value, Ingredient[] requiredIngredients) 
        : base(name, shorthand, id, value, colorCode) {
        CraftTime = craftTime; RequiredIngredients = requiredIngredients;
    }
}