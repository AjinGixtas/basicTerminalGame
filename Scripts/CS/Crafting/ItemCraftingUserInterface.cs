using Godot;

public partial class ItemCraftingUserInterface : MarginContainer {
	[Export] PackedScene craftThreadButtonScene, craftRecipeButtonScene;
	CraftThreadButton[] craftThreadButtons;
	[Export] HFlowContainer craftThreadButtonContainer;
	[Export] Container craftRecipeButtonContainer;
	[Export] ScrollContainer craftRecipeScrollContainer;
	public override void _Ready() {
		craftThreadButtons = new CraftThreadButton[ItemCrafter.MAX_THREADS];
		for (int i = 0; i < ItemCrafter.MAX_THREADS; ++i) {
			craftThreadButtons[i] = craftThreadButtonScene.Instantiate<CraftThreadButton>();
			craftThreadButtons[i].Initialize(i, this);
			craftThreadButtonContainer.AddChild(craftThreadButtons[i]);
		}
        CraftRecipeButton recipeButton = craftRecipeButtonScene.Instantiate<CraftRecipeButton>();
        recipeButton.Intialize(-1, this);
        craftRecipeButtonContainer.AddChild(recipeButton);

        for (int i = 0; i < ItemCrafter.ALL_RECIPES.Length; ++i) {
			recipeButton = craftRecipeButtonScene.Instantiate<CraftRecipeButton>();
			recipeButton.Intialize(ItemCrafter.ALL_RECIPES[i].ID, this);
			craftRecipeButtonContainer.AddChild(recipeButton);
		}
	}
	public override void _Process(double delta) {
		ItemCrafter.ProcessThreads(delta);
    }

    int selectedThreadID = -1;
    public void PullupCraftRecipeWindow(int requested_ID) {
		craftRecipeScrollContainer.Visible = true;
		selectedThreadID = requested_ID;
    }
	public void PulldownCraftRecipeWindow(int specifiedItem_ID) {
        craftRecipeScrollContainer.Visible = false;
		if (specifiedItem_ID != -1) ItemCrafter.AddItemCraft(ItemCrafter.ALL_RECIPES[specifiedItem_ID - 10], selectedThreadID);
		else ItemCrafter.AddItemCraft(null, selectedThreadID);
            selectedThreadID = -1; // Reset selected thread ID after crafting
    }
}
