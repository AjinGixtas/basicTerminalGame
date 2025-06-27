using Godot;
using System;
using System.Linq;

public partial class ItemCrafterDataSaveResource : Resource {
    [Export] public int[] id = new int[ItemCrafter.MAX_THREADS];
    [Export] public double[] remainTime = new double[ItemCrafter.MAX_THREADS];
    [Export] public int CurThread = 1;
    public static ItemCrafterDataSaveResource SerializeItemCrafter() {
        ItemCrafterDataSaveResource result = new();
        result.id = ItemCrafter.CraftThreads.Select(x => x.Recipe?.ID ?? -1).ToArray();
        result.remainTime = ItemCrafter.CraftThreads.Select(x => x.RemainTime).ToArray();
        result.CurThread = ItemCrafter.CurThreads;
        return result;
    }
}
