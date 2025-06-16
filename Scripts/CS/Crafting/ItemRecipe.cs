using System;

public class ItemRecipe {
    int[] _requiredItemID = []; public int[] RequiredItemID {
        get => _requiredItemID; init => _requiredItemID = value;
    }
    int _resultItemID; public int ResultItemID {
        get => _resultItemID; init => _resultItemID = value;
    }
    public ItemRecipe(int[] requiredItemID, int resultItemID) {
        Array.Sort(requiredItemID);
        RequiredItemID = requiredItemID;
        ResultItemID = resultItemID;
    }
}