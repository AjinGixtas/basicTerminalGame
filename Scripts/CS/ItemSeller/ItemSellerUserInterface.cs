using Godot;
using System;
using System.Linq;
using System.Text;

public partial class ItemSellerUserInterface : Control {
    [Export] PackedScene ItemSaleBoxScene;
    ItemSaleBox[] saleBoxes;
    [Export] Control saleBoxContainer;
    public override void _Ready() {
        saleBoxes = new ItemSaleBox[ItemCrafter.ALL_ITEMS.Length];
        for (int i = 0; i < saleBoxes.Length; i++) {
            saleBoxes[i] = ItemSaleBoxScene.Instantiate<ItemSaleBox>();
            saleBoxes[i].Init(i);
            saleBoxContainer.AddChild(saleBoxes[i]);
            LifeCycleDirector.FinishScene += saleBoxes[i].Render;
        }
    }
}