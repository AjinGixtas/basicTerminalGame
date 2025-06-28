using Godot;

public partial class ItemSellerUserInterface : Control {
    [Export] PackedScene ItemSaleBoxScene;
    ItemSaleBox[] saleBoxes;
    [Export] Control saleBoxContainer;
    public override void _Ready() {
        saleBoxes = new ItemSaleBox[ItemCrafter.ALL_ITEMS.Length];
        for (int i = 60; i < 75; i++) {
            saleBoxes[i] = ItemSaleBoxScene.Instantiate<ItemSaleBox>();
            saleBoxes[i].Init(i);
            saleBoxContainer.AddChild(saleBoxes[i]);
        }
    }
    public void OnPriceUpdated() {
        ItemSeller.UpdateMineralPrices(.05);
        for (int i = 60; i < 75; ++i) {
            saleBoxes[i].RenderGraph();
        }
    }
}
