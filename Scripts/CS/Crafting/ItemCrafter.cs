using Godot;
using System;
using System.Linq;

public static class ItemCrafter {
    /*
     *  MineralAccessLevel                                                      HighestCraftableIDs
                      0                                                                     [20]
                      1                                                                     [21]
                      2                                                             [21, 24, 25]
                      3                                                     [21, 24, 25, 27, 40]
                      4                                             [21, 24, 25, 27, 31, 40, 45]
                      5                                         [21, 24, 25, 27, 31, 36, 40, 45]
                      6                                     [21, 24, 25, 27, 31, 36, 40, 42, 45]
                      7                             [21, 24, 25, 27, 31, 36, 40, 42, 45, 48, 50]
                      8             [21, 24, 25, 27, 31, 36, 40, 42, 45, 48, 50, 54, 71, 73, 75]
                      9 [21, 24, 25, 27, 31, 36, 40, 42, 45, 48, 50, 54, 60, 71, 72, 73, 74, 75]
     */
    public static readonly ItemData[] MINERALS = [
        new ItemData("Datacite",  "DC!", 0, 1, Cc.r),
        new ItemData("Bitron",    "BT!", 1, 2, Cc.g),
        new ItemData("Cachelium", "CH!", 2, 3, Cc.b),
        new ItemData("Ferrosync", "FS!", 3, 4, Cc.c),
        new ItemData("Nexium",    "NX!", 4, 6, Cc.m),
        new ItemData("Fractyl",   "FT!", 5, 8, Cc.y),
        new ItemData("Algorite",  "AG!", 6, 12, Cc.gB),
        new ItemData("Quantar",   "QT!", 7, 16, Cc.bG),
        new ItemData("Synthite",  "SY!", 8, 24, Cc.rB),
        new ItemData("Oblivium",  "OB!", 9, 32, Cc.gR),
    ];
    public static readonly CraftableItemData[] REFINED_MATERIALS = new[] {
        new CraftableItemData("RefinedDatacite"    , "DC&", 10, Cc.r , 1.000000,  10, [new(0, 5)]),
        new CraftableItemData("RefinedBitron"      , "BT&", 11, Cc.g , 1.200000,  20, [new(1, 5)]),
        new CraftableItemData("RefinedCachelium"   , "CH&", 12, Cc.b , 1.400000,  30, [new(2, 5)]),
        new CraftableItemData("RefinedFerrosync"   , "FS&", 13, Cc.c , 1.600000,  40, [new(3, 5)]),
        new CraftableItemData("RefinedNexium"      , "NX&", 14, Cc.m , 1.800000,  60, [new(4, 5)]),
        new CraftableItemData("RefinedFractyl"     , "FT&", 15, Cc.y , 2.000000,  80, [new(5, 5)]),
        new CraftableItemData("RefinedAlgorite"    , "AG&", 16, Cc.gB, 2.200000, 120, [new(6, 5)]),
        new CraftableItemData("RefinedQuantar"     , "QT&", 17, Cc.bG, 2.400000, 160, [new(7, 5)]),
        new CraftableItemData("RefinedSynthite"    , "SY&", 18, Cc.rB, 2.600000, 240, [new(8, 5)]),
        new CraftableItemData("RefinedOblivium"    , "OB&", 19, Cc.gR, 2.800000, 320, [new(9, 5)]),
    };

    public static readonly CraftableItemData[] COMPONENTS = new[] {
        new CraftableItemData("ExploitTemplate"            , "XTP^", 20, Cc.r ,  3.015830,     140, [new(10, 1)]),
        new CraftableItemData("BitCore"                    , "BIT^", 21, Cc.g ,  3.394329,     165, [new(11, 1)]),
        new CraftableItemData("SynthBlock"                 , "SYN^", 22, Cc.LR,  4.100987,     395, [new(11, 1), new(10, 1), new(20, 1)]),
        new CraftableItemData("PatchKernel"                , "PAT^", 23, Cc.LM,  2.893386,     190, [new(11, 1), new(10, 1)]),
        new CraftableItemData("ChainFiber"                 , "CHA^", 24, Cc.b ,  2.942595,     195, [new(12, 1)]),
        new CraftableItemData("HashAlloy"                  , "HAS^", 25, Cc.LY,  1.677259,     900, [new(11, 1), new(12, 1), new(22, 1)]),
        new CraftableItemData("CryptoMesh"                 , "CRY^", 26, Cc.bG,  2.651109,     250, [new(11, 1), new(12, 1), new(10, 1)]),
        new CraftableItemData("FirewallShell"              , "FIR^", 27, Cc.c ,  3.249908,     230, [new(13, 1)]),
        new CraftableItemData("HashVeil"                   , "VEI^", 28, Cc.Y ,  2.633581,     275, [new(12, 1), new(13, 1)]),
        new CraftableItemData("ForkedPatch"                , "FOK^", 29, Cc.LG,  5.502313,    1115, [new(23, 1), new(22, 1)]),
        new CraftableItemData("ChainLattice"               , "LAT^", 30, Cc.LC,  5.083777,     630, [new(12, 1), new(26, 1)]),
        new CraftableItemData("NeuroChip"                  , "NEU^", 31, Cc.m ,  2.608146,     280, [new(14, 1)]),
        new CraftableItemData("EncryptedMatrix"            , "ENC^", 32, Cc.LB,  5.753381,    1050, [new(26, 1), new(28, 1)]),
        new CraftableItemData("SecurityWeave"              , "SEC^", 33, Cc.m ,  5.531502,    1280, [new(13, 1), new(30, 1)]),
        new CraftableItemData("BreachNet"                  , "BRE^", 34, Cc.LM, 10.720541,    3640, [new(13, 1), new(30, 1), new(29, 1)]),
        new CraftableItemData("NeuralCloak"                , "NCL^", 35, Cc.g ,  5.627896,     760, [new(28, 1), new(14, 1)]),
        new CraftableItemData("FluxThread"                 , "FLU^", 36, Cc.y ,  2.669393,     340, [new(15, 1)]),
        new CraftableItemData("LatticeFork"                , "LFK^", 37, Cc.LB, 10.848897,    3600, [new(30, 1), new(29, 1)]),
        new CraftableItemData("SchrodingerProcess"         , "SRP^", 38, Cc.G ,  7.807893,    1950, [new(15, 1), new(28, 1), new(30, 1)]),
        new CraftableItemData("ProxyArmor"                 , "PRX^", 39, Cc.M ,  7.328460,    4010, [new(13, 1), new(32, 1), new(29, 1)]),
        new CraftableItemData("IntrusionPlate"             , "INP^", 40, Cc.Y ,  9.802084,    8205, [new(30, 1), new(34, 1)]),
        new CraftableItemData("ScriptKiddieKit"            , "SKK^", 41, Cc.C ,  4.187388,     795, [new(15, 1), new(28, 1)]),
        new CraftableItemData("AugmentModule"              , "AUG^", 42, Cc.gB,  3.010945,     440, [new(16, 1)]),
        new CraftableItemData("GuardianThread"             , "GUT^", 43, Cc.gR,  8.855684,    2795, [new(16, 1), new(33, 1)]),
        new CraftableItemData("CanaryMimicry"              , "CMY^", 44, Cc.rB,  9.800926,    2995, [new(16, 1), new(33, 1), new(14, 1)]),
        new CraftableItemData("StealthCradle"              , "SCR^", 45, Cc.LR,  8.721571,    8845, [new(39, 1), new(35, 1)]),
        new CraftableItemData("NeuralPhantom"              , "NPH^", 46, Cc.B ,  8.183034,    1735, [new(35, 1), new(14, 1)]),
        new CraftableItemData("ExoPhantom"                 , "EXP^", 47, Cc.c ,  9.000472,   12540, [new(39, 1), new(41, 1), new(38, 1)]),
        new CraftableItemData("InhumanPretender"           , "IHP^", 48, Cc.rB,  2.681638,     530, [new(17, 1)]),
        new CraftableItemData("PhantomFork"                , "PFK^", 49, Cc.LM, 13.199542,   11380, [new(37, 1), new(46, 1)]),
        new CraftableItemData("ZalgoScript"                , "ZSC^", 50, Cc.m ,  6.617419,   10545, [new(16, 1), new(41, 1), new(17, 1)]),
        new CraftableItemData("StealthBranch"              , "STB^", 51, Cc.rG, 14.948797,   35575, [new(37, 1), new(47, 1)]),
        new CraftableItemData("SentinelMind"               , "SEM^", 52, Cc.gB, 10.315860,   14585, [new(43, 1), new(44, 1), new(46, 1)]),
        new CraftableItemData("GuardianEcho"               , "GUE^", 53, Cc.LY, 15.567694,   34425, [new(43, 1), new(47, 1)]),
        new CraftableItemData("SysCore"                    , "SYS^", 54, Cc.rB,  2.800820,     680, [new(18, 1)]),
        new CraftableItemData("SeedScript"                 , "SEE^", 55, Cc.C , 10.273040,   28170, [new(16, 1), new(52, 1)]),
        new CraftableItemData("GhostShard"                 , "GHS^", 56, Cc.bG, 10.947519,    9810, [new(44, 1), new(46, 1), new(17, 1)]),
        new CraftableItemData("CyberWarden"                , "CYW^", 57, Cc.LR, 17.190877,  163625, [new(53, 1), new(51, 1), new(18, 1)]),
        new CraftableItemData("TypoSquatter"               , "TSQ^", 58, Cc.LC, 12.537649,   49010, [new(47, 1), new(49, 1)]),
        new CraftableItemData("PhantomPrism"               , "PHP^", 59, Cc.LM, 13.221346,  101695, [new(44, 1), new(53, 1), new(49, 1)]),
        new CraftableItemData("ObsidianBit"                , "OBI^", 60, Cc.gR,  3.401000,     845, [new(19, 1)]),
        new CraftableItemData("RegexUnderstander"          , "RUS^", 61, Cc.LM, 19.922977,  531345, [new(57, 1), new(58, 1), new(17, 1)]),
        new CraftableItemData("MindConflux"                , "MCO^", 62, Cc.LG, 13.980728,  378870, [new(52, 1), new(57, 1)]),
        new CraftableItemData("EncryptedEcho"              , "ENE^", 63, Cc.M , 15.205651,  302790, [new(59, 1), new(51, 1), new(17, 1)]),
        new CraftableItemData("SysFork"                    , "SYF^", 64, Cc.B , 12.252784,   23825, [new(49, 1), new(18, 1)]),
        new CraftableItemData("ShadowBranch"               , "SHB^", 65, Cc.LB, 15.944953,  136080, [new(49, 1), new(58, 1)]),
        new CraftableItemData("NexusSentinel"              , "NES^", 66, Cc.g , 16.493059,  437865, [new(55, 1), new(57, 1)]),
        new CraftableItemData("EchoEngine"                 , "ECE^", 67, Cc.c , 14.923621, 1010360, [new(63, 1), new(65, 1), new(64, 1)]),
        new CraftableItemData("SuperEvilVirus.exe"         , "SEV^", 68, Cc.b , 20.065295, 1585610, [new(59, 1), new(61, 1)]),
        new CraftableItemData("GhostCrucible"              , "GCR^", 69, Cc.C , 13.038487,  302345, [new(56, 1), new(65, 1), new(19, 1)]),
        new CraftableItemData("CyberLink"                  , "CYL^", 70, Cc.y , 16.714432,  688160, [new(57, 1), new(65, 1)]),
        new CraftableItemData("VulnerabilityReportScraper" , "VRS^", 71, Cc.m , 21.672533, 4918845, [new(63, 1), new(68, 1)]),
        new CraftableItemData("SpectralTunnel"             , "SPT^", 72, Cc.LY, 21.550311, 1571925, [new(63, 1), new(69, 1)]),
        new CraftableItemData("ReflectionCore"             , "RFC^", 73, Cc.LR, 18.227775, 6202845, [new(68, 1), new(67, 1)]),
        new CraftableItemData("NexusAnchor"                , "NEA^", 74, Cc.LM, 20.348825, 3653410, [new(66, 1), new(67, 1), new(19, 1)]),
        new CraftableItemData("WardenFrame"                , "WAF^", 75, Cc.B , 19.925959, 6619795, [new(62, 1), new(68, 1), new(69, 1)]),
    };

    public static readonly CraftableItemData[] ALL_RECIPES = [.. REFINED_MATERIALS, .. COMPONENTS];
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
                return CError.INSUFFICIENT;
            }
        }
        CraftThreads[threadID] = new CraftThread() {
            Recipe = recipe,
            RemainTime = recipe.CraftTime
        };
        return CError.OK;
    }
    public static void Process(double delta) {
        for (int i = 0; i < CurThreads; ++i) {
            if (CraftThreads[i].Recipe == null) continue; // No recipe assigned to this thread

            CraftThreads[i].RemainTime -= delta; if (CraftThreads[i].RemainTime > 0) continue;
            bool enoughMaterial = true; for (int j = 0; j < CraftThreads[i].Recipe.RequiredIngredients.Length; ++j) {
                if (PlayerDataManager.MineInv[CraftThreads[i].Recipe.RequiredIngredients[j].ID] < CraftThreads[i].Recipe.RequiredIngredients[j].Amount) {
                    enoughMaterial = false; break;
                }
            }
            if (!enoughMaterial) { CraftThreads[i].Recipe = null; continue; }



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
            CraftThreads[i].Recipe = ALL_RECIPES.FirstOrDefault(r => r.ID == data.id[i]);
            CraftThreads[i].RemainTime = Mathf.Min(data.remainTime[i], CraftThreads[i].Recipe?.CraftTime ?? 0);
        }
        return 0;
    }

    public static CraftableItemData GetRecipe(int id) => ALL_RECIPES[id - 10];
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