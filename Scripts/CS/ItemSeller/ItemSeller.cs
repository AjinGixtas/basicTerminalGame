using Godot;
using System.Linq;

public static class ItemSeller {
    public static readonly ItemPriceInfo[] PriceModels = Enumerable.Range(0, 76).Select(id => new ItemPriceInfo { ID = id, sigma = GD.Randf() * 0.1 }).ToArray();

    static double marketCap = 0;
    public static void Ready() {
        for (int i = 0; i < PriceModels.Length; i++) { 
            PriceModels[i].logMean = Mathf.Log(ItemCrafter.ALL_ITEMS[i].Value);

            PriceModels[i].pastValue = ItemCrafter.ALL_ITEMS[i].Value;
            PriceModels[i].value = ItemCrafter.ALL_ITEMS[i].Value;
            PriceModels[i].miVal  = ItemCrafter.ALL_ITEMS[i].Value *  .95;
            PriceModels[i].maVal  = ItemCrafter.ALL_ITEMS[i].Value * 1.1;
            PriceModels[i].raVal = PriceModels[i].maVal - PriceModels[i].miVal;
            
            // May use for display purpose
            PriceModels[i].dMiVal = ItemCrafter.ALL_ITEMS[i].Value *  .78; 
            PriceModels[i].dMaVal = ItemCrafter.ALL_ITEMS[i].Value * 1.35;
            PriceModels[i].dRaVal = PriceModels[i].dMaVal - PriceModels[i].dMiVal;
            // Evaluation
            marketCap += ItemCrafter.ALL_ITEMS[i].Value;
        }
        UpdateMineralPrices(1.0);
    }
    const double PULL_ALPHA = .2;
    public static void UpdateMineralPrices(double delta) {
        double sqrtD = Mathf.Sqrt(delta);
        double curMarketCap = 0;
        for (int i = 0; i < PriceModels.Length; i++) {
            PriceModels[i].pastValue = PriceModels[i].value;
            PriceModels[i].sigma = Mathf.Clamp(PriceModels[i].sigma + sqrtD * (GD.Randf()-.5) * .05, 0.0, .1);
            double valueLog = Mathf.Log(PriceModels[i].value);
            double pull = PULL_ALPHA * (PriceModels[i].logMean - valueLog) * sqrtD;
            double shock = PriceModels[i].sigma * sqrtD * GD.Randfn(0.0, 1.0);
            PriceModels[i].value = Mathf.Clamp(Mathf.Exp(valueLog + pull + shock), PriceModels[i].miVal, PriceModels[i].maVal);
            curMarketCap += PriceModels[i].value;
        }
    }

    public struct ItemPriceInfo {
        public int ID;
        public double sigma;
        public double logMean;
        public double pastValue, value;
        public double miVal, maVal, raVal;
        public double dMiVal, dMaVal, dRaVal;
    }
}