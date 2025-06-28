using Godot;
using System.Composition.Hosting.Core;
using System.Linq;

public partial class CraftRecipeButton : MarginContainer {
	int _id = -1; public int ID { get => _id; set { if (_id >= 0) return; _id = value; } }
	CraftableItemData _recipe;
	[Export] RichTextLabel itemLabel, ingredientsLabel, timeCraftLabel, priceLabel;
	[Export] ColorRect notEnoughMaterialOverlay;
	ItemCraftingUserInterface owner;
	public void Intialize(int ID, ItemCraftingUserInterface owner) {
		this.ID = ID; this.owner = owner;
		if (ID == -1) {
			_recipe = null;
			itemLabel.Text = "null";
			ingredientsLabel.Text = "Nothing :)";
            timeCraftLabel.Text =  $" Time: {Util.Format($"0", StrSty.UNIT, "s")}";
			priceLabel.Text = $"Value: {Util.Format($"0", StrSty.MONEY)}";
			return;
		}

		for (int i = 0; i < ItemCrafter.ALL_RECIPES.Length; i++) {
			if (ItemCrafter.ALL_RECIPES[i].ID != ID) continue;
			_recipe = ItemCrafter.ALL_RECIPES[i];
		}
		ItemData[] materials = _recipe.RequiredIngredients.Select(ing => ItemCrafter.ALL_ITEMS.FirstOrDefault(r => r.ID == ing.ID)).Where(r => r != null).ToArray();
		string ingredientsText = "";
		for (int i = 0; i < materials.Length; i++) {
			if (ingredientsText.Length > 0) ingredientsText += ", ";
			ingredientsText += $"[color={Util.CC(materials[i].ColorCode)}]{materials[i].Shorthand}[/color] x{_recipe.RequiredIngredients[i].Amount}";
        }


        itemLabel.Text = $"([color={Util.CC(_recipe.ColorCode)}]{_recipe.Shorthand:4}[/color])\n[color={Util.CC(_recipe.ColorCode)}]{_recipe.Name}[/color]";
		ingredientsLabel.Text = ingredientsText;


		string value = $"{Util.Format($"{Mathf.RoundToInt(_recipe.Value)}", StrSty.MONEY)}";
		int len = 11 + (value.Length - Util.RemoveBBCode(value).Length);
        timeCraftLabel.Text = $" Time: {Util.Format($"{_recipe.CraftTime}", StrSty.UNIT, "s")}";
		priceLabel.Text =     $"Value: {Util.Format($"{Mathf.RoundToInt(_recipe.Value)}", StrSty.MONEY)}";
	}
    public override void _Process(double delta) {
        if (!Visible) return;
		if (_recipe == null) return;
		bool enoughMaterial = false;
		for (int i = 0; i < _recipe.RequiredIngredients.Length; ++i) {
			if (_recipe.RequiredIngredients[i].Amount <= PlayerDataManager.MineInv[_recipe.RequiredIngredients[i].ID]) continue;
			enoughMaterial = true; break;
		}
		notEnoughMaterialOverlay.Visible = enoughMaterial;
    }
    public void OnPress() {
		owner.PulldownCraftRecipeWindow(ID);
	}
}