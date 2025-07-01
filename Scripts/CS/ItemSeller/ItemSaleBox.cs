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
		string gcStr = Util.Format($"{ItemCrafter.ALL_ITEMS[itemID].Value}", StrSty.MONEY);
		CurrentPriceDisplay.Text = gcStr.PadLeft(gcStr.Length + (17 - Util.RemoveBBCode(gcStr).Length));
	}
    public override void _Process(double delta) {
        InStockAmountDisplay.Text = Util.Format($"{PlayerDataManager.MineInv[itemID]}", StrSty.NUMBER, "0");
    }
    public void OnSellItems() {
		if (!long.TryParse(sellAmountInput.Text, out long amount)) {RuntimeDirector.MakeNotification(Util.Format("Invalid amount provided!", StrSty.ERROR)); return; }
		if (PlayerDataManager.WithdrawItem(itemID, amount) != CError.OK) { RuntimeDirector.MakeNotification(Util.Format("Not enough in stock to sell!", StrSty.ERROR)); return; }
		
		PlayerDataManager.DepositGC((long)(ItemCrafter.ALL_ITEMS[itemID].Value * amount));
	}
}
