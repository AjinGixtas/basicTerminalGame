using Godot;
using System.Linq;

public partial class CraftRecipeButton : Button {
    int _id = -1; public int ID { get => _id; set { if (_id >= 0) return; _id = value; } }
    ItemRecipe _recipe;
    RichTextLabel title;
    public void Intialize(int ID) {
        this.ID = ID;
        for (int i = 0; i < ItemCrafter.ALL_RECIPES.Length; i++) {
            if (ItemCrafter.ALL_RECIPES[i].ID != ID) continue;
            _recipe = ItemCrafter.ALL_RECIPES[i];
        }
        ItemRecipe[] ingredients = _recipe.RequiredIngredients
        .Select(ing => ItemCrafter.ALL_RECIPES.FirstOrDefault(r => r.ID == ing.ID))
        .Where(r => r != null).ToArray();
        string ingredientsText = "";
        for (int i = 0; i < ingredients.Length; i++) {
            if (i > 0) ingredientsText += ", ";
            ingredientsText += $"{ingredients[i].Shorthand} x{_recipe.RequiredIngredients[i].Amount}";
        }
        title.Text = $"{_recipe.Name:27} ({_recipe.Shorthand:4}) | " + ingredientsText;
    }
}
