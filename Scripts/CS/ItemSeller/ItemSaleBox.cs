using Godot;

public partial class ItemSaleBox : Control {
	public void Init(int id) { itemID = id; }
	[Export] LineEdit sellAmountInput;
	[Export] RichTextLabel CurrentPriceDisplay, InStockAmountDisplay, ItemSymbol;
	[Export] int itemID = -1;
    public override void _Ready() {
		ItemSymbol.Text = $"[color={Util.CC(ItemCrafter.ALL_ITEMS[itemID].ColorCode)}]{ItemCrafter.ALL_ITEMS[itemID].Shorthand}[/color]";
	}
    public void Render() {
		double percentDiffer = ((ItemSeller.ItemPricesModel[itemID].value - ItemCrafter.ALL_ITEMS[itemID].Value) / ItemCrafter.ALL_ITEMS[itemID].Value * 100);
		int deltaCode = ItemSeller.ItemPricesModel[itemID].value > ItemSeller.ItemPricesModel[itemID].pastValue ? 1 : ItemSeller.ItemPricesModel[itemID].value < ItemSeller.ItemPricesModel[itemID].pastValue ? -1 : 0;
		string arrow = deltaCode == 1 ? "▴" : deltaCode == -1 ? "▾" : "-";
		CurrentPriceDisplay.Text = Util.Format($"{ItemSeller.ItemPricesModel[itemID].value}", StrSty.NUMBER).PadLeft(35)
			+ $"[color={Util.CC(Cc.Y)}]GC[/color]"
			+ $"     [color={Util.CC(percentDiffer > 0 ? Cc.g : percentDiffer < 0 ? Cc.r : Cc.y)}]{percentDiffer,6:F2}%"
			+ $"[color={Util.CC(deltaCode == 1 ? Cc.LG : deltaCode == -1 ? Cc.LR : Cc.LY)}]{arrow}[/color][/color]";

        InStockAmountDisplay.Text = Util.Format($"{PlayerDataManager.MineInv[itemID]}", StrSty.NUMBER, "0");
	}
	public void OnSellItems() {
		if (!long.TryParse(sellAmountInput.Text, out long amount)) {RuntimeDirector.MakeNotification(Util.Format("Invalid amount provided!", StrSty.ERROR)); return; }
		if (PlayerDataManager.WithdrawMineral(itemID, amount) != CError.OK) { RuntimeDirector.MakeNotification(Util.Format("Not enough in stock to sell!", StrSty.ERROR)); return; }
		PlayerDataManager.DepositGC((long)(ItemSeller.ItemPricesModel[itemID].value * amount));
	}
}
