using Acornima.Ast;
using Godot;

public partial class ItemCraftingUserInterface : MarginContainer {
	[Export] PackedScene craftThreadButtonScene, craftRecipeButtonScene;
	CraftThreadButton[] craftThreadButtons;
	[Export] HFlowContainer craftThreadButtonContainer;
	[Export] Container craftRecipeButtonContainer;
	[Export] ScrollContainer craftRecipeScrollContainer;
	[Export] RichTextLabel WindowDescriptionLabel;
	[Export] MenuDirector menuDirector;

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
		ItemCrafter.ThreadAmountChanged += OnCraftThreadCountChange;
		OnCraftThreadCountChange(ItemCrafter.CurThreads);
		WindowDescriptionLabel.Text = @$"Each thread help craft items. Items increase in value as you go deeper into the crafting system. See {Util.Format("CraftModule", StrSty.CODE_MODULE)} in scripting to see how you can automate it.";
    }
	public override void _Process(double delta) {
		if (menuDirector.MenuWindowIndex != MenuDirector.CRAFTER_INDEX) return;
		for (int i = 0; i < craftThreadButtons.Length; ++i) {
			craftThreadButtons[i].Update();
        }
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
	[Export] RichTextLabel threadCountUpgradeButtonText;
	public void OnCraftThreadCountChange(int thread) {
		threadCountUpgradeButtonText.Text = $"Increase thread count - {Util.Format($"{ItemCrafter.GetUpgradeCraftThreadsCost()}", StrSty.MONEY)}";
	}
	public void OnUpgradeThreadCountButtonPressed() {
		CError cer = ItemCrafter.UpgradeCraftThreadCount();
		switch (cer) {
			case CError.OK: break;
			case CError.REDUNDANT: RuntimeDirector.MakeNotification(Util.Format("Max thread reached", StrSty.WARNING)); break;
			case CError.INSUFFICIENT: RuntimeDirector.MakeNotification(Util.Format("Not enough money", StrSty.ERROR)); break;
		}
	}
}
