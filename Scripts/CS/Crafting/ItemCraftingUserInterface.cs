using Godot;

public partial class ItemCraftingUserInterface : MarginContainer {
	[Export] PackedScene craftThreadButtonScene;
	CraftThreadButton[] craftThreadButtons;
	[Export] HFlowContainer craftThreadButtonContainer, craftRecipeButtonContainer;
	[Export] ScrollContainer craftRecipeScrollContainer;
	public override void _Ready() {
		craftThreadButtons = new CraftThreadButton[ItemCrafter.MAX_THREADS];
		for (int i = 0; i < ItemCrafter.MAX_THREADS; ++i) {
			craftThreadButtons[i] = craftThreadButtonScene.Instantiate<CraftThreadButton>();
			craftThreadButtons[i].Initialize(i, this);
			craftThreadButtonContainer.AddChild(craftThreadButtons[i]);
		}
	}
	public void PullupCraftRecipeWindow(int requested_ID) {
		
	}

}
