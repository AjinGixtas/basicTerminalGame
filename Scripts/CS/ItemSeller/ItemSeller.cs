using Godot;
using System.Linq;

public static class ItemSeller {
    public static readonly ItemPriceInfo[] ItemPricesModel = Enumerable.Range(0, 75).Select(id => new ItemPriceInfo { ID = id, sigma = GD.Randf() * 0.2 }).ToArray();

    static double marketCap = 0;
    public static void Ready() {
        for (int i = 0; i < ItemPricesModel.Length; i++) { 
            ItemPricesModel[i].pastValue = ItemCrafter.ALL_ITEMS[i].Value;
            ItemPricesModel[i].value = ItemCrafter.ALL_ITEMS[i].Value;
            // Lower bound range smaller so player don't get fucked as often as they benefit.
            ItemPricesModel[i].miVal  = ItemCrafter.ALL_ITEMS[i].Value *  .80;
            ItemPricesModel[i].dMiVal = ItemCrafter.ALL_ITEMS[i].Value *  .78; // Use for display purpose
            ItemPricesModel[i].maVal  = ItemCrafter.ALL_ITEMS[i].Value * 1.33;
            ItemPricesModel[i].dMaVal = ItemCrafter.ALL_ITEMS[i].Value * 1.35;
            ItemPricesModel[i].raVal = ItemPricesModel[i].maVal - ItemPricesModel[i].miVal;
            ItemPricesModel[i].dRaVal = ItemPricesModel[i].dMaVal - ItemPricesModel[i].dMiVal;
            marketCap += ItemCrafter.ALL_ITEMS[i].Value;
        }
    }
    const double PULL_ALPHA = .01;
    public static void UpdateMineralPrices(double delta) {
        double sqrtD = Mathf.Sqrt(delta);
        double curMarketCap = 0;
        for (int i = 0; i < ItemPricesModel.Length; i++) {
            ItemPricesModel[i].pastValue = ItemPricesModel[i].value;
            ItemPricesModel[i].sigma = Mathf.Clamp(ItemPricesModel[i].sigma + (GD.Randf()-.5) * .15, 0.0, 0.33);
            double shock = sqrtD * ItemPricesModel[i].sigma * GD.Randfn(0.0, 1.0);
            double pull = sqrtD * PULL_ALPHA * (ItemCrafter.ALL_ITEMS[i].Value - ItemPricesModel[i].value);
            ItemPricesModel[i].value = ItemPricesModel[i].value * (1 + shock + pull);
            curMarketCap += ItemPricesModel[i].value;
        }
    }

    public struct ItemPriceInfo {
        public int ID;
        public double sigma;
        public double pastValue, value;
        public double miVal, maVal, raVal;
        public double dMiVal, dMaVal, dRaVal;
    }
}