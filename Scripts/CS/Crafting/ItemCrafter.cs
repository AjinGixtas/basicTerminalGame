using System;

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
        new ItemRecipe("RefinedDatacite"   , "DC&", 10, Cc.gB,  .2,  2, [new(0, 5)]),
        new ItemRecipe("RefinedBitron"     , "BT&", 11, Cc.LM, 1.0,  3, [new(1, 5)]),
        new ItemRecipe("RefinedCachelium"  , "CH&", 12, Cc.r,  1.2,  4, [new(2, 5)]),
        new ItemRecipe("RefinedFerrosync"  , "FS&", 13, Cc.LM,  .5,  5, [new(3, 5)]),
        new ItemRecipe("RefinedNexium"     , "NX&", 14, Cc.G,  2.0,  7, [new(4, 5)]),
        new ItemRecipe("RefinedFractyl"    , "FT&", 15, Cc.bG,  .8,  9, [new(5, 5)]),
        new ItemRecipe("RefinedAlgorite"   , "AG&", 16, Cc.LY, 1.8, 13, [new(6, 5)]),
        new ItemRecipe("RefinedQuantar"    , "QT&", 17, Cc.LB, 1.5, 17, [new(7, 5)]),
        new ItemRecipe("RefinedSynthite"   , "SY&", 18, Cc.LC, 2.5, 25, [new(8, 5)]),
        new ItemRecipe("RefinedOblivium"   , "OB&", 19, Cc.gR, 3.0, 33, [new(9, 5)]),
    ];
    public static readonly ItemRecipe[] COMPONENTS = [
        new ItemRecipe("ExploitTemplate",            "XTP^", 20, Cc.gB,   3.032515,    22.060650, [new(10, 1)]),
        new ItemRecipe("BitCore",                    "BIT^", 21, Cc.B,    3.019767,    25.120791, [new(11, 1)]),
        new ItemRecipe("SynthBlock",                 "SYN^", 22, Cc.LR,   4.803151,    51.408446, [new(11, 1), new(10, 1), new(20, 1)]),
        new ItemRecipe("PatchKernel",                "PAT^", 23, Cc.r,    3.305056,    29.198303, [new(11, 1), new(10, 1)]),
        new ItemRecipe("ChainFiber",                 "CHA^", 24, Cc.M,    2.913746,    30.174825, [new(12, 1)]),
        new ItemRecipe("HashAlloy",                  "HAS^", 25, Cc.LY,   6.317294,    90.287798, [new(11, 1), new(12, 1), new(22, 1)]),
        new ItemRecipe("CryptoMesh",                 "CRY^", 26, Cc.bG,   2.880179,    38.345621, [new(11, 1), new(12, 1), new(10, 1)]),
        new ItemRecipe("FirewallShell",              "FIR^", 27, Cc.c,    3.000442,    35.240035, [new(13, 1)]),
        new ItemRecipe("HashVeil",                   "VEI^", 28, Cc.Y,    3.450334,    42.483047, [new(12, 1), new(13, 1)]),
        new ItemRecipe("ForkedPatch",                "FOK^", 29, Cc.LG,   5.581186,   114.105562, [new(23, 1), new(22, 1)]),
        new ItemRecipe("ChainLattice",               "LAT^", 30, Cc.LC,   5.086344,    76.601193, [new(12, 1), new(26, 1)]),
        new ItemRecipe("NeuroChip",                  "NEU^", 31, Cc.bR,   2.706793,    43.324815, [new(14, 1)]),
        new ItemRecipe("EncryptedMatrix",            "ENC^", 32, Cc.LB,   5.322571,   117.130831, [new(26, 1), new(28, 1)]),
        new ItemRecipe("SecurityWeave",              "SEC^", 33, Cc.m,    8.370616,   124.682833, [new(13, 1), new(30, 1)]),
        new ItemRecipe("BreachNet",                  "BRE^", 34, Cc.LM,   8.515495,   249.627618, [new(13, 1), new(30, 1), new(29, 1)]),
        new ItemRecipe("NeuralCloak",                "NCL^", 35, Cc.g,    5.636729,    92.554108, [new(28, 1), new(14, 1)]),
        new ItemRecipe("FluxThread",                 "FLU^", 36, Cc.rG,   3.320753,    52.531321, [new(15, 1)]),
        new ItemRecipe("LatticeFork",                "LFK^", 37, Cc.LB,   9.480447,   245.786608, [new(30, 1), new(29, 1)]),
        new ItemRecipe("SchrodingerProcess",         "SRP^", 38, Cc.G,    7.773203,   183.584611, [new(15, 1), new(28, 1), new(30, 1)]),
        new ItemRecipe("ProxyArmor",                 "PRX^", 39, Cc.M,   10.852072,   304.198497, [new(13, 1), new(32, 1), new(29, 1)]),
        new ItemRecipe("IntrusionPlate",             "INP^", 40, Cc.Y,   12.808785,   408.014756, [new(30, 1), new(34, 1)]),
        new ItemRecipe("ScriptKiddieKit",            "SKK^", 41, Cc.C,    5.734800,   102.836932, [new(15, 1), new(28, 1)]),
        new ItemRecipe("AugmentModule",              "AUG^", 42, Cc.b,    2.761930,    66.662863, [new(16, 1)]),
        new ItemRecipe("GuardianThread",             "GUT^", 43, Cc.gR,   8.625756,   204.507852, [new(16, 1), new(33, 1)]),
        new ItemRecipe("CanaryMimicry",              "CMY^", 44, Cc.rB,   9.522826,   219.984380, [new(16, 1), new(33, 1), new(14, 1)]),
        new ItemRecipe("StealthCradle",              "SCR^", 45, Cc.LR,  11.777012,   488.478208, [new(39, 1), new(35, 1)]),
        new ItemRecipe("NeuralPhantom",              "NPH^", 46, Cc.B,    5.831913,   156.651613, [new(35, 1), new(14, 1)]),
        new ItemRecipe("ExoPhantom",                 "EXP^", 47, Cc.c,   12.484368,   711.355220, [new(39, 1), new(41, 1), new(38, 1)]),
        new ItemRecipe("InhumaneHumanPretender",     "IHP^", 48, Cc.y,    3.457287,    81.106332, [new(17, 1)]),
        new ItemRecipe("PhantomFork",                "PFK^", 49, Cc.LM,  11.760284,   498.766097, [new(37, 1), new(46, 1)]),
        new ItemRecipe("ZalgoScript",                "ZSC^", 50, Cc.m,    6.258437,   218.777642, [new(16, 1), new(41, 1), new(17, 1)]),
        new ItemRecipe("StealthBranch",              "STB^", 51, Cc.rG,  14.714942,  1148.984692, [new(37, 1), new(47, 1)]),
        new ItemRecipe("SentinelMind",               "SEM^", 52, Cc.gB,  10.126077,   691.990918, [new(43, 1), new(44, 1), new(46, 1)]),
        new ItemRecipe("GuardianEcho",               "GUE^", 53, Cc.LY,  14.963897,  1105.911878, [new(43, 1), new(47, 1)]),
        new ItemRecipe("SysCore",                    "SYS^", 54, Cc.G,    2.959775,   103.420692, [new(18, 1)]),
        new ItemRecipe("SeedScript",                 "SEE^", 55, Cc.C,   10.326236,   844.925830, [new(16, 1), new(52, 1)]),
        new ItemRecipe("GhostShard",                 "GHS^", 56, Cc.bG,  12.217434,   514.560826, [new(44, 1), new(46, 1), new(17, 1)]),
        new ItemRecipe("CyberWarden",                "CYW^", 57, Cc.LR,  13.073530,  2660.966454, [new(53, 1), new(51, 1), new(18, 1)]),
        new ItemRecipe("TypoSquatter",               "TSQ^", 58, Cc.LC,  15.104397,  1450.902847, [new(47, 1), new(49, 1)]),
        new ItemRecipe("PhantomPrism",               "PHP^", 59, Cc.R,   12.371526,  2109.400928, [new(44, 1), new(53, 1), new(49, 1)]),
        new ItemRecipe("ObsidianBit",                "OBI^", 60, Cc.Y,    3.029882,   125.939124, [new(19, 1)]),
        new ItemRecipe("RegexUnderstander",          "RUS^", 61, Cc.LM,  15.304243,  4839.057126, [new(57, 1), new(58, 1), new(17, 1)]),
        new ItemRecipe("MindConflux",                "MCO^", 62, Cc.LG,  16.767762,  3977.173286, [new(52, 1), new(57, 1)]),
        new ItemRecipe("EncryptedEcho",              "ENE^", 63, Cc.M,   20.336853,  4022.546518, [new(59, 1), new(51, 1), new(17, 1)]),
        new ItemRecipe("SysFork",                    "SYF^", 64, Cc.B,   10.840952,   670.040745, [new(49, 1), new(18, 1)]),
        new ItemRecipe("ShadowBranch",               "SHB^", 65, Cc.LB,  13.813480,  2283.986072, [new(49, 1), new(58, 1)]),
        new ItemRecipe("NexusSentinel",              "NES^", 66, Cc.g,   18.499714,  4220.472318, [new(55, 1), new(57, 1)]),
        new ItemRecipe("EchoEngine",                 "ECE^", 67, Cc.r,   20.844046,  8497.773482, [new(63, 1), new(65, 1), new(64, 1)]),
        new ItemRecipe("SuperEvilVirus.exe",         "SEV^", 68, Cc.b,   17.630678,  8241.518354, [new(59, 1), new(61, 1)]),
        new ItemRecipe("GhostCrucible",              "GCR^", 69, Cc.C,   16.663028,  3408.533886, [new(56, 1), new(65, 1), new(19, 1)]),
        new ItemRecipe("CyberLink",                  "CYL^", 70, Cc.y,   13.923977,  5703.486576, [new(57, 1), new(65, 1)]),
        new ItemRecipe("VulnerabilityReportScraper", "SID^", 71, Cc.m,   20.275170, 14821.624845, [new(63, 1), new(68, 1)]),
        new ItemRecipe("SpectralTunnel",             "SPT^", 72, Cc.LY,  17.259360,  8785.637352, [new(63, 1), new(69, 1)]),
        new ItemRecipe("ReflectionCore",             "RFC^", 73, Cc.LR,  24.005042, 20830.565812, [new(68, 1), new(67, 1)]),
        new ItemRecipe("NexusAnchor",                "NEA^", 74, Cc.LM,  16.306412, 14940.571511, [new(66, 1), new(67, 1), new(19, 1)]),
        new ItemRecipe("WardenFrame",                "WAF^", 75, Cc.B,   18.057136, 21233.410278, [new(62, 1), new(68, 1), new(70, 1)]),
    ];

    const int MAX_CRAFTS = 32; // Maximum number of crafts that can be in progress at the same time
    static CraftThread[] CraftThreads = new CraftThread[MAX_CRAFTS]; // Probably lower, but 32 is a good number to start with
    static int CRAFT_THREAD_COUNT = 1;
    public static void AddItemCraft(ItemRecipe recipe, int amount = 1) {
        amount = Math.Max(amount, 1); // Ensure at least 1 item is crafted
        for (int i = 0; i < recipe.RequiredIngredients.Length; ++i) {
            if (PlayerDataManager.MineInv[recipe.RequiredIngredients[i].ID] < recipe.RequiredIngredients[i].Amount) {
                ShellCore.Say("-r", $"Not enough {MINERALS[recipe.RequiredIngredients[i].ID]} to craft {recipe.Name}");
                return;
            }
        }
        for (int i = 0; i < CRAFT_THREAD_COUNT; ++i) {
            if (CraftThreads[i].RemainTime <= 0) {
                CraftThreads[i] = new CraftThread() { Recipe = recipe, TotalTime = recipe.CraftTime, RemainTime = recipe.CraftTime};
                return;
            }
        }
    }
    public static void ProcessThreads(double delta) {
        for (int i = 0; i < CRAFT_THREAD_COUNT; ++i) {
            if (CraftThreads[i].Amount <= 0) continue;
            CraftThreads[i].RemainTime -= delta;
            if (CraftThreads[i].RemainTime > 0) continue;
            
            for (int j = 0; j < CraftThreads[i].Recipe.RequiredIngredients.Length; ++j) 
                PlayerDataManager.MineInv[CraftThreads[i].Recipe.RequiredIngredients[j].ID] -= CraftThreads[i].Recipe.RequiredIngredients[j].Amount;
            ++PlayerDataManager.MineInv[CraftThreads[i].Recipe.ID];
            
            CraftThreads[i].Amount -= 1;
            CraftThreads[i].RemainTime += CraftThreads[i].TotalTime; // Use += in case delta is too big.
        }
    }
    public static CError UpgradeCraftThreadCost() {
        if (CRAFT_THREAD_COUNT >= MAX_CRAFTS) return CError.REDUNDANT;

        long cost = GetUpgradeCraftThreadsCost();
        if (PlayerDataManager.GC_Cur < cost) return CError.INSUFFICIENT;

        PlayerDataManager.WithdrawGC(cost); ++CRAFT_THREAD_COUNT;
        return CError.OK;
    }
    public static long GetUpgradeCraftThreadsCost() {
        return (CRAFT_THREAD_COUNT + 1) * (CRAFT_THREAD_COUNT + 1) * 1000; // Example cost formula, can be adjusted
    }
    struct CraftThread {
        public ItemRecipe Recipe { get; init; }
        public int Amount { get; set; }
        public double TotalTime { get; init; }
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