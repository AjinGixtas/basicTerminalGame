using Godot;
using System.Linq;
using System.Text;

public partial class ItemSellerUserInterface : Control {
    [Export] PackedScene ItemSaleBoxScene;
    ItemSaleBox[] saleBoxes;
    [Export] Control saleBoxContainer;
    public override void _Ready() {
        saleBoxes = new ItemSaleBox[ItemCrafter.ALL_ITEMS.Length];
        for (int i = 0; i < 75; i++) {
            saleBoxes[i] = ItemSaleBoxScene.Instantiate<ItemSaleBox>();
            saleBoxes[i].Init(i);
            saleBoxContainer.AddChild(saleBoxes[i]);
        }
        Overlay.Text = string.Concat(Enumerable.Repeat("·······························································\n", HALF_LINE_COUNT))
                     + string.Concat(Enumerable.Repeat("―――――――――――――――――――――――――――――――――――――――――――――――――――――――――――――――\n", 1))
                     + string.Concat(Enumerable.Repeat("·······························································\n", HALF_LINE_COUNT));
        GD.Print(this);
    }
    public void OnPriceUpdated() {
        ItemSeller.UpdateMineralPrices(.05);
        for (int i = 0; i < 75; ++i) {
            saleBoxes[i].Render();
        }
        RenderPriceGraph();
    }
    static (string, double)[] blocks = new (string, double)[] {
        (" ", .5 / 16 * 0),
        ("▁", .5 / 16 * 1),
        ("▂", .5 / 16 * 2),
        ("▃", .5 / 16 * 3),
        ("▄", .5 / 16 * 4),
        ("▅", .5 / 16 * 5),
        ("▆", .5 / 16 * 6),
        ("▇", .5 / 16 * 7),
        ("█", .5 / 16 * 8),
    };
    [Export] RichTextLabel Overlay, MidZ;
    [Export] RichTextLabel[] GrnZ, RedZ;
    [Export] int focusedID = 0;
    const int LINE_COUNT = 17, HALF_LINE_COUNT = 8;
    public void RenderPriceGraph() {
        double percentageDelta = (ItemSeller.ItemPricesModel[focusedID].value / ItemCrafter.ALL_ITEMS[focusedID].Value - 1.0);
        MidZ.AppendText($"[color={Util.CC(percentageDelta > 0 ? Cc.G : percentageDelta < 0 ? Cc.R : Cc.Y)}]█[/color]");
        double budgetPercent = Mathf.Abs(percentageDelta);
        int curCharI = blocks.Length - 1;
        if (percentageDelta > 0){
            for (int i = 0; i < HALF_LINE_COUNT; ++i) {
                while (budgetPercent < blocks[curCharI].Item2 && curCharI > 0) --curCharI;
                GrnZ[i].AddText(blocks[curCharI].Item1);
                RedZ[i].AddText(" ");
                budgetPercent -= blocks[curCharI].Item2;
            }
        }
        else {
            for (int i = 0; i < HALF_LINE_COUNT; ++i) {
                while (budgetPercent < blocks[curCharI].Item2 && curCharI > 0) --curCharI;
                RedZ[i].AddText(blocks[curCharI].Item1);
                GrnZ[i].AddText(" ");
                budgetPercent -= blocks[curCharI].Item2;
            }
        }
    }
}
