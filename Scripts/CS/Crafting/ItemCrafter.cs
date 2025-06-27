using Godot;
using System;
using System.Linq;

public static class ItemCrafter {
    public static readonly MineralProfile[] MINERALS = [
        new MineralProfile("Datacite",  "DC", 0, 1, Cc.gB),
        new MineralProfile("Bitron",    "BT", 1, 2, Cc.LM),
        new MineralProfile("Cachelium", "CH", 2, 3, Cc.bG),
        new MineralProfile("Ferrosync", "FS", 3, 4, Cc.LM),
        new MineralProfile("Nexium",    "NX", 4, 6, Cc.r),
        new MineralProfile("Fractyl",   "FT", 5, 8, Cc.LB),
        new MineralProfile("Algorite",  "AG", 6, 12, Cc.LY),
        new MineralProfile("Quantar",   "QT", 7, 16, Cc.G),
        new MineralProfile("Synthite",  "SY", 8, 24, Cc.LC),
        new MineralProfile("Oblivium",  "OB", 9, 32, Cc.gR),
    ];
    public static readonly ItemRecipe[] REFINED_MATERIALS = [
        new ItemRecipe("RefinedDatacite"            , "DC&" , 10, Cc.gB,   1.000000,        2, [new(0, 5)]),
        new ItemRecipe("RefinedBitron"              , "BT&" , 11, Cc.LM,   1.200000,        4, [new(1, 5)]),
        new ItemRecipe("RefinedCachelium"           , "CH&" , 12, Cc.r,    1.400000,        6, [new(2, 5)]),
        new ItemRecipe("RefinedFerrosync"           , "FS&" , 13, Cc.LM,   1.600000,        8, [new(3, 5)]),
        new ItemRecipe("RefinedNexium"              , "NX&" , 14, Cc.G,    1.800000,       12, [new(4, 5)]),
        new ItemRecipe("RefinedFractyl"             , "FT&" , 15, Cc.bG,   2.000000,       16, [new(5, 5)]),
        new ItemRecipe("RefinedAlgorite"            , "AG&" , 16, Cc.LY,   2.200000,       24, [new(6, 5)]),
        new ItemRecipe("RefinedQuantar"             , "QT&" , 17, Cc.LB,   2.400000,       32, [new(7, 5)]),
        new ItemRecipe("RefinedSynthite"            , "SY&" , 18, Cc.LC,   2.600000,       48, [new(8, 5)]),
        new ItemRecipe("RefinedOblivium"            , "OB&" , 19, Cc.gR,   2.800000,       64, [new(9, 5)]),
    ];
    public static readonly ItemRecipe[] COMPONENTS = [
        new ItemRecipe("ExploitTemplate"            , "XTP^", 20, Cc.gB,   2.804499 ,      23, [new(10, 1)]),
        new ItemRecipe("BitCore"                    , "BIT^", 21, Cc.B,    3.244930 ,      27, [new(11, 1)]),
        new ItemRecipe("SynthBlock"                 , "SYN^", 22, Cc.LR,   4.628695 ,      67, [new(11, 1), new(10, 1), new(20, 1)]),
        new ItemRecipe("PatchKernel"                , "PAT^", 23, Cc.LM,   2.845187 ,      32, [new(11, 1), new(10, 1)]),
        new ItemRecipe("ChainFiber"                 , "CHA^", 24, Cc.M,    2.580463 ,      33, [new(12, 1)]),
        new ItemRecipe("HashAlloy"                  , "HAS^", 25, Cc.LY,   7.694453 ,     169, [new(11, 1), new(12, 1), new(22, 1)]),
        new ItemRecipe("CryptoMesh"                 , "CRY^", 26, Cc.bG,   3.111943 ,      43, [new(11, 1), new(12, 1), new(10, 1)]),
        new ItemRecipe("FirewallShell"              , "FIR^", 27, Cc.c,    3.292019 ,      39, [new(13, 1)]),
        new ItemRecipe("HashVeil"                   , "VEI^", 28, Cc.Y,    2.603971 ,      47, [new(12, 1), new(13, 1)]),
        new ItemRecipe("ForkedPatch"                , "FOK^", 29, Cc.LG,   5.867867 ,     195, [new(23, 1), new(22, 1)]),
        new ItemRecipe("ChainLattice"               , "LAT^", 30, Cc.LC,   5.147444 ,     109, [new(12, 1), new(26, 1)]),
        new ItemRecipe("NeuroChip"                  , "NEU^", 31, Cc.bR,   3.067208 ,      49, [new(14, 1)]),
        new ItemRecipe("EncryptedMatrix"            , "ENC^", 32, Cc.LB,   4.365931 ,     169, [new(26, 1), new(28, 1)]),
        new ItemRecipe("SecurityWeave"              , "SEC^", 33, Cc.m,    5.864398 ,     229, [new(13, 1), new(30, 1)]),
        new ItemRecipe("BreachNet"                  , "BRE^", 34, Cc.LM,   9.680644 ,     680, [new(13, 1), new(30, 1), new(29, 1)]),
        new ItemRecipe("NeuralCloak"                , "NCL^", 35, Cc.g,    5.549031 ,     133, [new(28, 1), new(14, 1)]),
        new ItemRecipe("FluxThread"                 , "FLU^", 36, Cc.rG,   2.637579 ,      59, [new(15, 1)]),
        new ItemRecipe("LatticeFork"                , "LFK^", 37, Cc.LB,   9.784620 ,     670, [new(30, 1), new(29, 1)]),
        new ItemRecipe("SchrodingerProcess"         , "SRP^", 38, Cc.G,    5.896840 ,     326, [new(15, 1), new(28, 1), new(30, 1)]),
        new ItemRecipe("ProxyArmor"                 , "PRX^", 39, Cc.M,    8.738781 ,     772, [new(13, 1), new(32, 1), new(29, 1)]),
        new ItemRecipe("IntrusionPlate"             , "INP^", 40, Cc.Y,   12.518310 ,    1907, [new(30, 1), new(34, 1)]),
        new ItemRecipe("ScriptKiddieKit"            , "SKK^", 41, Cc.C,    5.286944 ,     144, [new(15, 1), new(28, 1)]),
        new ItemRecipe("AugmentModule"              , "AUG^", 42, Cc.b,    3.072247 ,      77, [new(16, 1)]),
        new ItemRecipe("GuardianThread"             , "GUT^", 43, Cc.gR,  10.372139 ,     586, [new(16, 1), new(33, 1)]),
        new ItemRecipe("CanaryMimicry"              , "CMY^", 44, Cc.rB,   7.959417 ,     545, [new(16, 1), new(33, 1), new(14, 1)]),
        new ItemRecipe("StealthCradle"              , "SCR^", 45, Cc.LR,  11.364378 ,    2077, [new(39, 1), new(35, 1)]),
        new ItemRecipe("NeuralPhantom"              , "NPH^", 46, Cc.B,    6.216372 ,     295, [new(35, 1), new(14, 1)]),
        new ItemRecipe("ExoPhantom"                 , "EXP^", 47, Cc.c,   12.213654 ,    2946, [new(39, 1), new(41, 1), new(38, 1)]),
        new ItemRecipe("InhumaneHumanPretender"     , "IHP^", 48, Cc.y,    2.745785 ,      93, [new(17, 1)]),
        new ItemRecipe("PhantomFork"                , "PFK^", 49, Cc.LM,   9.989393 ,    2076, [new(37, 1), new(46, 1)]),
        new ItemRecipe("ZalgoScript"                , "ZSC^", 50, Cc.m,    6.328821 ,     395, [new(16, 1), new(41, 1), new(17, 1)]),
        new ItemRecipe("StealthBranch"              , "STB^", 51, Cc.rG,  15.639465 ,    9788, [new(37, 1), new(47, 1)]),
        new ItemRecipe("SentinelMind"               , "SEM^", 52, Cc.gB,  10.147684 ,    3071, [new(43, 1), new(44, 1), new(46, 1)]),
        new ItemRecipe("GuardianEcho"               , "GUE^", 53, Cc.LY,  13.702389 ,    8845, [new(43, 1), new(47, 1)]),
        new ItemRecipe("SysCore"                    , "SYS^", 54, Cc.G,    2.567979 ,     120, [new(18, 1)]),
        new ItemRecipe("SeedScript"                 , "SEE^", 55, Cc.C,   11.406034 ,    7014, [new(16, 1), new(52, 1)]),
        new ItemRecipe("GhostShard"                 , "GHS^", 56, Cc.bG,  10.749422 ,    1958, [new(44, 1), new(46, 1), new(17, 1)]),
        new ItemRecipe("CyberWarden"                , "CYW^", 57, Cc.LR,  14.306274 ,   47736, [new(53, 1), new(51, 1), new(18, 1)]),
        new ItemRecipe("TypoSquatter"               , "TSQ^", 58, Cc.LC,  14.573853 ,   13018, [new(47, 1), new(49, 1)]),
        new ItemRecipe("PhantomPrism"               , "PHP^", 59, Cc.LM,  18.323344 ,   34161, [new(44, 1), new(53, 1), new(49, 1)]),
        new ItemRecipe("ObsidianBit"                , "OBI^", 60, Cc.Y,    3.079496 ,     150, [new(19, 1)]),
        new ItemRecipe("RegexUnderstander"          , "RUS^", 61, Cc.LM,  15.775161 ,  164574, [new(57, 1), new(58, 1), new(17, 1)]),
        new ItemRecipe("MindConflux"                , "MCO^", 62, Cc.LG,  14.214242 ,  129241, [new(52, 1), new(57, 1)]),
        new ItemRecipe("EncryptedEcho"              , "ENE^", 63, Cc.M,   16.794116 ,  123801, [new(59, 1), new(51, 1), new(17, 1)]),
        new ItemRecipe("SysFork"                    , "SYF^", 64, Cc.B,   13.371390 ,    5279, [new(49, 1), new(18, 1)]),
        new ItemRecipe("ShadowBranch"               , "SHB^", 65, Cc.LB,  14.295538 ,   38573, [new(49, 1), new(58, 1)]),
        new ItemRecipe("NexusSentinel"              , "NES^", 66, Cc.g,   13.801363 ,  136897, [new(55, 1), new(57, 1)]),
        new ItemRecipe("EchoEngine"                 , "ECE^", 67, Cc.c,   16.971760 ,  474869, [new(63, 1), new(65, 1), new(64, 1)]),
        new ItemRecipe("SuperEvilVirus.exe"         , "SEV^", 68, Cc.b,   18.268275 ,  589950, [new(59, 1), new(61, 1)]),
        new ItemRecipe("GhostCrucible"              , "GCR^", 69, Cc.C,   19.365599 ,  125242, [new(56, 1), new(65, 1), new(19, 1)]),
        new ItemRecipe("CyberLink"                  , "CYL^", 70, Cc.y,   13.815484 ,  215900, [new(57, 1), new(65, 1)]),
        new ItemRecipe("VulnerabilityReportScraper" , "VRS^", 71, Cc.m,   21.215791 , 2339506, [new(63, 1), new(68, 1)]),
        new ItemRecipe("SpectralTunnel"             , "SPT^", 72, Cc.LY,  15.964101 ,  679024, [new(63, 1), new(69, 1)]),
        new ItemRecipe("ReflectionCore"             , "RFC^", 73, Cc.LR,  17.168535 , 3037681, [new(68, 1), new(67, 1)]),
        new ItemRecipe("NexusAnchor"                , "NEA^", 74, Cc.LM,  17.152620 , 1744420, [new(66, 1), new(67, 1), new(19, 1)]),
        new ItemRecipe("WardenFrame"                , "WAF^", 75, Cc.B,   23.486174 , 3287903, [new(62, 1), new(68, 1), new(70, 1)]),
    ];
    public static readonly ItemRecipe[] ALL_RECIPES = [ .. REFINED_MATERIALS, .. COMPONENTS];

    public const int MAX_THREADS = 32; // Maximum number of crafts that can be in progress at the same time
    public static CraftThread[] CraftThreads = new CraftThread[MAX_THREADS]; // Probably lower, but 32 is a good number to start with
    static int curThreads = 1;
    public static event Action<int> ThreadAmountChanged;
    public static int CurThreads { get => curThreads; private set { curThreads = value; ThreadAmountChanged?.Invoke(CurThreads); } }
    public static void AddItemCraft(ItemRecipe recipe, int threadID) {
        if (recipe == null) {
            CraftThreads[threadID] = new CraftThread() {
                Recipe = recipe,
                RemainTime = 0
            };
            return;
        }
        for (int i = 0; i < recipe.RequiredIngredients.Length; ++i) {
            if (PlayerDataManager.MineInv[recipe.RequiredIngredients[i].ID] < recipe.RequiredIngredients[i].Amount) {
                ShellCore.Say("-r", $"Not enough {MINERALS[recipe.RequiredIngredients[i].ID].Shorthand} to craft {recipe.Name}");
                return;
            }
        }
        CraftThreads[threadID] = new CraftThread() {
            Recipe = recipe,
            RemainTime = recipe.CraftTime
        };  
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

    public static CError UpgradeCraftThreadCost() {
        if (CurThreads >= MAX_THREADS) return CError.REDUNDANT;

        long cost = GetUpgradeCraftThreadsCost();
        if (PlayerDataManager.GC_Cur < cost) return CError.INSUFFICIENT;

        PlayerDataManager.WithdrawGC(cost); ++CurThreads;
        
        return CError.OK;
    }
    public static long GetUpgradeCraftThreadsCost() {
        return (CurThreads + 1) * (CurThreads + 1) * 1000; // Example cost formula, can be adjusted
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
        public ItemRecipe Recipe { get; set; }
        public double RemainTime { get; set; }
    }
}

public struct Ingredient {
    public int ID { get; init; }
    public long Amount { get; init; }
    public Ingredient(int id, long amount) {
        ID = id; Amount = amount;
    }
}