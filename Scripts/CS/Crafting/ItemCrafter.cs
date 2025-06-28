using Godot;
using System;
using System.Linq;

public static class ItemCrafter {
    public static readonly ItemData[] MINERALS = [
        new ItemData("Datacite",  "DC", 0, 1, Cc.gB),
        new ItemData("Bitron",    "BT", 1, 2, Cc.LM),
        new ItemData("Cachelium", "CH", 2, 3, Cc.bG),
        new ItemData("Ferrosync", "FS", 3, 4, Cc.LM),
        new ItemData("Nexium",    "NX", 4, 6, Cc.r),
        new ItemData("Fractyl",   "FT", 5, 8, Cc.LB),
        new ItemData("Algorite",  "AG", 6, 12, Cc.LY),
        new ItemData("Quantar",   "QT", 7, 16, Cc.G),
        new ItemData("Synthite",  "SY", 8, 24, Cc.LC),
        new ItemData("Oblivium",  "OB", 9, 32, Cc.gR),
    ];
    public static readonly CraftableItemData[] REFINED_MATERIALS = [
        new CraftableItemData("RefinedDatacite"            , "DC&" , 10, Cc.gB,  1.000000,       2, [new(0, 5)]),
        new CraftableItemData("RefinedBitron"              , "BT&" , 11, Cc.LM,  1.200000,       4, [new(1, 5)]),
        new CraftableItemData("RefinedCachelium"           , "CH&" , 12, Cc.r,   1.400000,       6, [new(2, 5)]),
        new CraftableItemData("RefinedFerrosync"           , "FS&" , 13, Cc.LM,  1.600000,       8, [new(3, 5)]),
        new CraftableItemData("RefinedNexium"              , "NX&" , 14, Cc.G,   1.800000,      12, [new(4, 5)]),
        new CraftableItemData("RefinedFractyl"             , "FT&" , 15, Cc.bG,  2.000000,      16, [new(5, 5)]),
        new CraftableItemData("RefinedAlgorite"            , "AG&" , 16, Cc.LY,  2.200000,      24, [new(6, 5)]),
        new CraftableItemData("RefinedQuantar"             , "QT&" , 17, Cc.LB,  2.400000,      32, [new(7, 5)]),
        new CraftableItemData("RefinedSynthite"            , "SY&" , 18, Cc.LC,  2.600000,      48, [new(8, 5)]),
        new CraftableItemData("RefinedOblivium"            , "OB&" , 19, Cc.gR,  2.800000,      64, [new(9, 5)]),
    ];
    public static readonly CraftableItemData[] COMPONENTS = [
        new CraftableItemData("ExploitTemplate"            , "XTP^", 20, Cc.gB,  3.015830,      28, [new(10, 1)]),
        new CraftableItemData("BitCore"                    , "BIT^", 21, Cc.B ,  3.394329,      33, [new(11, 1)]),
        new CraftableItemData("SynthBlock"                 , "SYN^", 22, Cc.LR,  4.100987,      79, [new(11, 1), new(10, 1), new(20, 1)]),
        new CraftableItemData("PatchKernel"                , "PAT^", 23, Cc.LM,  2.893386,      38, [new(11, 1), new(10, 1)]),
        new CraftableItemData("ChainFiber"                 , "CHA^", 24, Cc.M ,  2.942595,      39, [new(12, 1)]),
        new CraftableItemData("HashAlloy"                  , "HAS^", 25, Cc.LY,  6.677259,     180, [new(11, 1), new(12, 1), new(22, 1)]),
        new CraftableItemData("CryptoMesh"                 , "CRY^", 26, Cc.bG,  2.651109,      50, [new(11, 1), new(12, 1), new(10, 1)]),
        new CraftableItemData("FirewallShell"              , "FIR^", 27, Cc.c ,  3.249908,      46, [new(13, 1)]),
        new CraftableItemData("HashVeil"                   , "VEI^", 28, Cc.Y ,  2.633581,      55, [new(12, 1), new(13, 1)]),
        new CraftableItemData("ForkedPatch"                , "FOK^", 29, Cc.LG,  5.502313,     223, [new(23, 1), new(22, 1)]),
        new CraftableItemData("ChainLattice"               , "LAT^", 30, Cc.LC,  5.083777,     126, [new(12, 1), new(26, 1)]),
        new CraftableItemData("NeuroChip"                  , "NEU^", 31, Cc.bR,  2.608146,      56, [new(14, 1)]),
        new CraftableItemData("EncryptedMatrix"            , "ENC^", 32, Cc.LB,  5.753381,     210, [new(26, 1), new(28, 1)]),
        new CraftableItemData("SecurityWeave"              , "SEC^", 33, Cc.m ,  5.531502,     256, [new(13, 1), new(30, 1)]),
        new CraftableItemData("BreachNet"                  , "BRE^", 34, Cc.LM, 10.720541,     728, [new(13, 1), new(30, 1), new(29, 1)]),
        new CraftableItemData("NeuralCloak"                , "NCL^", 35, Cc.g ,  5.627896,     152, [new(28, 1), new(14, 1)]),
        new CraftableItemData("FluxThread"                 , "FLU^", 36, Cc.rG,  2.669393,      68, [new(15, 1)]),
        new CraftableItemData("LatticeFork"                , "LFK^", 37, Cc.LB, 10.848897,     720, [new(30, 1), new(29, 1)]),
        new CraftableItemData("SchrodingerProcess"         , "SRP^", 38, Cc.G ,  7.807893,     390, [new(15, 1), new(28, 1), new(30, 1)]),
        new CraftableItemData("ProxyArmor"                 , "PRX^", 39, Cc.M ,  7.328460,     802, [new(13, 1), new(32, 1), new(29, 1)]),
        new CraftableItemData("IntrusionPlate"             , "INP^", 40, Cc.Y ,  9.802084,    1641, [new(30, 1), new(34, 1)]),
        new CraftableItemData("ScriptKiddieKit"            , "SKK^", 41, Cc.C ,  4.187388,     159, [new(15, 1), new(28, 1)]),
        new CraftableItemData("AugmentModule"              , "AUG^", 42, Cc.b ,  3.010945,      88, [new(16, 1)]),
        new CraftableItemData("GuardianThread"             , "GUT^", 43, Cc.gR,  8.855684,     559, [new(16, 1), new(33, 1)]),
        new CraftableItemData("CanaryMimicry"              , "CMY^", 44, Cc.rB,  9.800926,     599, [new(16, 1), new(33, 1), new(14, 1)]),
        new CraftableItemData("StealthCradle"              , "SCR^", 45, Cc.LR,  8.721571,    1769, [new(39, 1), new(35, 1)]),
        new CraftableItemData("NeuralPhantom"              , "NPH^", 46, Cc.B ,  8.183034,     347, [new(35, 1), new(14, 1)]),
        new CraftableItemData("ExoPhantom"                 , "EXP^", 47, Cc.c ,  9.000472,    2508, [new(39, 1), new(41, 1), new(38, 1)]),
        new CraftableItemData("InhumaneHumanPretender"     , "IHP^", 48, Cc.y ,  2.681638,     106, [new(17, 1)]),
        new CraftableItemData("PhantomFork"                , "PFK^", 49, Cc.LM, 13.199542,    2276, [new(37, 1), new(46, 1)]),
        new CraftableItemData("ZalgoScript"                , "ZSC^", 50, Cc.m ,  6.617419,     421, [new(16, 1), new(41, 1), new(17, 1)]),
        new CraftableItemData("StealthBranch"              , "STB^", 51, Cc.rG, 14.948797,    7115, [new(37, 1), new(47, 1)]),
        new CraftableItemData("SentinelMind"               , "SEM^", 52, Cc.gB, 10.315860,    2917, [new(43, 1), new(44, 1), new(46, 1)]),
        new CraftableItemData("GuardianEcho"               , "GUE^", 53, Cc.LY, 15.567694,    6885, [new(43, 1), new(47, 1)]),
        new CraftableItemData("SysCore"                    , "SYS^", 54, Cc.G ,  2.800820,     136, [new(18, 1)]),
        new CraftableItemData("SeedScript"                 , "SEE^", 55, Cc.C , 10.273040,    5634, [new(16, 1), new(52, 1)]),
        new CraftableItemData("GhostShard"                 , "GHS^", 56, Cc.bG, 10.947519,    1962, [new(44, 1), new(46, 1), new(17, 1)]),
        new CraftableItemData("CyberWarden"                , "CYW^", 57, Cc.LR, 17.190877,   32725, [new(53, 1), new(51, 1), new(18, 1)]),
        new CraftableItemData("TypoSquatter"               , "TSQ^", 58, Cc.LC, 12.537649,    9802, [new(47, 1), new(49, 1)]),
        new CraftableItemData("PhantomPrism"               , "PHP^", 59, Cc.LM, 13.221346,   20339, [new(44, 1), new(53, 1), new(49, 1)]),
        new CraftableItemData("ObsidianBit"                , "OBI^", 60, Cc.Y ,  3.401000,     169, [new(19, 1)]),
        new CraftableItemData("RegexUnderstander"          , "RUS^", 61, Cc.LM, 19.922977,  106269, [new(57, 1), new(58, 1), new(17, 1)]),
        new CraftableItemData("MindConflux"                , "MCO^", 62, Cc.LG, 13.980728,   75774, [new(52, 1), new(57, 1)]),
        new CraftableItemData("EncryptedEcho"              , "ENE^", 63, Cc.M , 15.205651,   60558, [new(59, 1), new(51, 1), new(17, 1)]),
        new CraftableItemData("SysFork"                    , "SYF^", 64, Cc.B , 12.252784,    4765, [new(49, 1), new(18, 1)]),
        new CraftableItemData("ShadowBranch"               , "SHB^", 65, Cc.LB, 15.944953,   27216, [new(49, 1), new(58, 1)]),
        new CraftableItemData("NexusSentinel"              , "NES^", 66, Cc.g , 16.493059,   87573, [new(55, 1), new(57, 1)]),
        new CraftableItemData("EchoEngine"                 , "ECE^", 67, Cc.c , 14.923621,  202072, [new(63, 1), new(65, 1), new(64, 1)]),
        new CraftableItemData("SuperEvilVirus.exe"         , "SEV^", 68, Cc.b , 20.065295,  317122, [new(59, 1), new(61, 1)]),
        new CraftableItemData("GhostCrucible"              , "GCR^", 69, Cc.C , 13.038487,   60469, [new(56, 1), new(65, 1), new(19, 1)]),
        new CraftableItemData("CyberLink"                  , "CYL^", 70, Cc.y , 16.714432,  137632, [new(57, 1), new(65, 1)]),
        new CraftableItemData("VulnerabilityReportScraper" , "VRS^", 71, Cc.m , 21.672533,  983769, [new(63, 1), new(68, 1)]),
        new CraftableItemData("SpectralTunnel"             , "SPT^", 72, Cc.LY, 21.550311,  314385, [new(63, 1), new(69, 1)]),
        new CraftableItemData("ReflectionCore"             , "RFC^", 73, Cc.LR, 18.227775, 1240569, [new(68, 1), new(67, 1)]),
        new CraftableItemData("NexusAnchor"                , "NEA^", 74, Cc.LM, 20.348825,  730682, [new(66, 1), new(67, 1), new(19, 1)]),
        new CraftableItemData("WardenFrame"                , "WAF^", 75, Cc.B , 19.925959, 1323959, [new(62, 1), new(68, 1), new(70, 1)]),
    ];
    public static readonly CraftableItemData[] ALL_RECIPES = [ .. REFINED_MATERIALS, .. COMPONENTS];
    public static readonly ItemData[] ALL_ITEMS = [.. MINERALS, .. ALL_RECIPES];

    public const int MAX_THREADS = 32; // Maximum number of crafts that can be in progress at the same time
    public static CraftThread[] CraftThreads = new CraftThread[MAX_THREADS]; // Probably lower, but 32 is a good number to start with
    static int curThreads = 1;
    public static event Action<int> ThreadAmountChanged;
    public static int CurThreads { get => curThreads; private set { curThreads = value; ThreadAmountChanged?.Invoke(CurThreads); } }
    public static CError AddItemCraft(CraftableItemData recipe, int threadID) {
        if (recipe == null) {
            CraftThreads[threadID] = new CraftThread() {
                Recipe = recipe,
                RemainTime = 0
            };
            return CError.OK;
        }
        for (int i = 0; i < recipe.RequiredIngredients.Length; ++i) {
            if (PlayerDataManager.MineInv[recipe.RequiredIngredients[i].ID] < recipe.RequiredIngredients[i].Amount) {
                ShellCore.Say("-r", $"Not enough {MINERALS[recipe.RequiredIngredients[i].ID].Shorthand} to craft {recipe.Name}");
                return CError.INSUFFICIENT;
            }
        }
        CraftThreads[threadID] = new CraftThread() {
            Recipe = recipe,
            RemainTime = recipe.CraftTime
        };
        return CError.OK;
    }
    public static void ProcessThreads(double delta) {
        for (int i = 0; i < CurThreads; ++i) {
            if (CraftThreads[i].Recipe == null) continue; // No recipe assigned to this thread

            bool enoughMaterial = true;
            for (int j = 0; j < CraftThreads[i].Recipe.RequiredIngredients.Length; ++j)
                if (PlayerDataManager.MineInv[CraftThreads[i].Recipe.RequiredIngredients[j].ID] < CraftThreads[i].Recipe.RequiredIngredients[j].Amount) {
                    enoughMaterial = false; break;
                }
            if (!enoughMaterial) continue;


            CraftThreads[i].RemainTime -= delta; if (CraftThreads[i].RemainTime > 0) continue;
            
            for (int j = 0; j < CraftThreads[i].Recipe.RequiredIngredients.Length; ++j) 
                PlayerDataManager.MineInv[CraftThreads[i].Recipe.RequiredIngredients[j].ID] -= CraftThreads[i].Recipe.RequiredIngredients[j].Amount;
            
            ++PlayerDataManager.MineInv[CraftThreads[i].Recipe.ID];
            CraftThreads[i].RemainTime += CraftThreads[i].Recipe.CraftTime; // Use += in case delta is too big.
        }
    }

    public static CError UpgradeCraftThreadCount() {
        if (CurThreads >= MAX_THREADS) return CError.REDUNDANT;

        long cost = GetUpgradeCraftThreadsCost();
        if (PlayerDataManager.GC_Cur < cost) return CError.INSUFFICIENT;

        PlayerDataManager.WithdrawGC(cost); ++CurThreads;
        
        return CError.OK;
    }
    public static long GetUpgradeCraftThreadsCost(int thread = -1) {
        thread = thread < 0 ? CurThreads + 1 : thread;
        return (long)(Mathf.Pow(thread, 2) * Mathf.Pow(10.0, Math.PI)); // Example cost formula, can be adjusted
    }

    public static int SaveItemCrafterData(string filePath) {
        ItemCrafterDataSaveResource data = ItemCrafterDataSaveResource.SerializeItemCrafter();
        Error error = ResourceSaver.Save(data, filePath);
        return (int)error;
    }
    public static int LoadItemCrafterData(string filePath) {
        if (!FileAccess.FileExists(filePath)) { return 1; }
        ItemCrafterDataSaveResource data;
        try { data = GD.Load<ItemCrafterDataSaveResource>(filePath); } catch { return 2; }
        if (data == null) return 3;
        CurThreads = data.CurThread;
        CraftThreads = new CraftThread[MAX_THREADS];
        for (int i = 0; i < data.CurThread; i++) {
            CraftThreads[i].RemainTime = data.remainTime[i];
            CraftThreads[i].Recipe = ALL_RECIPES.FirstOrDefault(r => r.ID == data.id[i]);
        }
        return 0;
    }


    public struct CraftThread {
        public CraftableItemData Recipe { get; set; }
        public double RemainTime { get; set; }
    }
}

public struct Ingredient {
    public int ID { get; init; }
    public int Amount { get; init; }
    public Ingredient(int id, int amount) {
        ID = id; Amount = amount;
    }
}