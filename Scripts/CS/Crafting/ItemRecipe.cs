using Godot;
public partial class ItemRecipe : MineralProfile {
    public Ingredient[] RequiredIngredients { get; init; }
    public double CraftTime { get; init; }
    const double CRAFT_TIME_MULT = 15;
    public ItemRecipe(string name, string shorthand, int id, Cc colorCode, double craftTime, double value, Ingredient[] requiredIngredients) 
        : base(name, shorthand, id, value, colorCode) {
        CraftTime = craftTime * CRAFT_TIME_MULT; RequiredIngredients = requiredIngredients;
    }
}