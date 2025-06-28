using System.Linq;

public class CraftModule {
    public long[] GetInventory() { 
        return PlayerDataManager.MineInv; 
    }
    public string[] GetAllItemsSymbol() {
        return ItemCrafter.ALL_ITEMS.Select(x => x.Shorthand).ToArray();
    }
    
    public int[][] GetItemRecipeByID(int itemID) {
        CraftableItemData recipe = null;
        for (int i = 0; i < ItemCrafter.ALL_RECIPES.Length; i++) {
            if (itemID != ItemCrafter.ALL_RECIPES[i].ID) continue;
            recipe = ItemCrafter.ALL_RECIPES[i];
        }
        if (recipe == null) return [];
        return recipe.RequiredIngredients.Select(x => new int[] { x.ID, x.Amount}).ToArray();
    }
    public int[][] GetItemRecipeBySymbol(string itemSymbol) {
        CraftableItemData recipe = null;
        for (int i = 0; i < ItemCrafter.ALL_RECIPES.Length; i++) {
            if (itemSymbol != ItemCrafter.ALL_RECIPES[i].Shorthand) continue;
            recipe = ItemCrafter.ALL_RECIPES[i];
        }
        if (recipe == null) return [];
        return recipe.RequiredIngredients.Select(x => new int[] { x.ID, x.Amount}).ToArray();
    }
    
    public long GetItemAmountByID(int itemID) {
        if (ItemCrafter.MINERALS.Length <= itemID) return -1;
        return PlayerDataManager.MineInv[itemID];
    }
    public long GetItemAmountBySymbol(string itemSymbol) {
        int id = -1;
        for (int i = 0; i < ItemCrafter.ALL_ITEMS.Length; i++) {
            if (itemSymbol == ItemCrafter.ALL_ITEMS[i].Shorthand) {
                id = ItemCrafter.ALL_ITEMS[i].ID;
            }
        }
        return PlayerDataManager.MineInv[id];
    }
    
    public CError SetThreadToCraftItemByID(int threadID, int itemID) {
        if (threadID >= ItemCrafter.CurThreads) return CError.NO_PERMISSION;
        CraftableItemData recipe = null;
        for (int i = 0; i < ItemCrafter.ALL_RECIPES.Length; ++i) {
            if (itemID != ItemCrafter.ALL_RECIPES[i].ID) continue;
            recipe = null;
        }
        if (recipe == null) return CError.NOT_FOUND;
        return ItemCrafter.AddItemCraft(recipe, threadID);
    }
    public CError SetThreadToCraftItemBySymbol(int threadID, string itemSymbol) {
        if (threadID >= ItemCrafter.CurThreads) return CError.NO_PERMISSION;
        CraftableItemData recipe = null;
        for (int i = 0; i < ItemCrafter.ALL_RECIPES.Length; ++i) {
            if (itemSymbol != ItemCrafter.ALL_RECIPES[i].Shorthand) continue;
            recipe = null;
        }
        if (recipe == null) return CError.NOT_FOUND;
        return ItemCrafter.AddItemCraft(recipe, threadID);
    }

    public CError SetThreadToCraftNothing(int threadID) {
        return ItemCrafter.AddItemCraft(null, threadID);
    }
}