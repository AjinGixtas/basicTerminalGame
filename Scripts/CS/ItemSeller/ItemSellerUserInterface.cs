using Godot;

public partial class ItemSellerUserInterface : Control {
    [Export] PackedScene ItemSaleBoxScene;
    ItemSaleBox[] saleBoxes;
    [Export] Control saleBoxContainer;
    [Export] MenuDirector menuDirector;
    public override void _Ready() {
        saleBoxes = new ItemSaleBox[ItemCrafter.ALL_ITEMS.Length];
        for (int i = 0; i < saleBoxes.Length; i++) {
            saleBoxes[i] = ItemSaleBoxScene.Instantiate<ItemSaleBox>();
            saleBoxes[i].Init(i);
            saleBoxContainer.AddChild(saleBoxes[i]);
            LifeCycleDirector.FinishScene += saleBoxes[i].Render;
        }
    }
    public override void _Process(double delta) {
        if (menuDirector.MenuWindowIndex != MenuDirector.SELL_INDEX) return;
        for (int i = 0; i < saleBoxes.Length; i++) {
            saleBoxes[i].Update();
        }
    }
}