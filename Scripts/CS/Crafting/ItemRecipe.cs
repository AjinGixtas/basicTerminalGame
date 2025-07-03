using Godot;

public partial class CraftableItemData : ItemData {
    public Ingredient[] RequiredIngredients { get; init; }
    public double CraftTime { get; init; }
    const double CRAFT_TIME_MULT = .3;
    public CraftableItemData(string name, string shorthand, int id, Cc colorCode, double craftTime, double value, Ingredient[] requiredIngredients) 
        : base(name, shorthand, id, value, colorCode) {
        CraftTime = craftTime * CRAFT_TIME_MULT; RequiredIngredients = requiredIngredients;
        return;
        string output = $"new(\"{name}\", \"{shorthand}\", {id-10}, Cc.{colorCode}, {craftTime:F6}, {value}, [";
        for (int i = 0; i < requiredIngredients.Length; i++) {
            output += $"new({requiredIngredients[i].ID-10}, {requiredIngredients[i].Amount})";
            if (i < requiredIngredients.Length - 1) output += ", ";
        }
        output += "]),";
        GD.Print(output);
    }
}