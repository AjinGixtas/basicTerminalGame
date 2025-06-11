using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static partial class NetworkManager {
    static PlayerNode _playerNode;
    public static PlayerNode PlayerNode { get => _playerNode; private set { _playerNode = value; } }
    static List<Sector> connectedSectors;
    static List<DriftSector> driftSectors;
    static List<StaticSector> staticSectors;
    static Dictionary<string, WeakReference<NetworkNode>> DNS;
    public static List<BotFarm> BotNet { get; private set; } = [];
    const int DRIFT_SECTOR_COUNT = 64;

    public static void Ready() {
        DNS = []; driftSectors = []; connectedSectors = []; staticSectors = []; BotNet = [];
        PlayerNode = new PlayerNode(GD.Load<NodeData>("res://Utilities/Resources/ScriptedNetworkNodes/PlayerNode.tres"));

        AssignDNS(PlayerNode);
        RegenerateDriftSector();
    }
    const double CYCLE_TIME = 60*15; // 15min in seconds
    static double TimeRemains = CYCLE_TIME;
    public static void Process(double delta) {
        ManageHackFarm(delta);
        MineralCollection(delta);
        TimeRemains -= delta;
        if (TimeRemains <= 0) {
            TimeRemains += CYCLE_TIME;
            RegenerateDriftSector((int)(DRIFT_SECTOR_COUNT * .33));
        }
    }

    public static string[] GetSectorNames(bool mustConnected=false, int level=-1) {
        List<string> names = [];
        List<Sector> nullSector = [];
        foreach (var sector in driftSectors) {
            if (sector == null) { 
                nullSector.Add(sector);
                continue; // Skip null sectors
            }
            if (level > 0 && sector.SectorLevel != level) continue; // Skip sectors that don't match the level
            if (mustConnected && !connectedSectors.Contains(sector)) continue; // Skip if mustConnected is true and sector is not connected
            names.Add(sector.Name);
        }
        foreach (DriftSector sector in nullSector) driftSectors.Remove(sector);
        nullSector.Clear();
        foreach (var sector in staticSectors) {
            if (sector == null) { nullSector.Add(sector); continue; } 
            if (mustConnected && !connectedSectors.Contains(sector)) continue; // Skip if mustConnected is true and sector is not connected
            names.Add(sector.Name);
        }
        foreach (StaticSector sector in nullSector) staticSectors.Remove(sector);
        nullSector.Clear();
        return [.. names];
    }
    
    public static string[] GetBotnetNames() {
        List<string> names = [];
        foreach(BotFarm hackFarm in BotNet) {
            if (hackFarm == null) { QueueRemoveHackFarm(hackFarm); continue; }
            names.Add(hackFarm.HostName);
        }
        return [.. names];
    }
    public static int GetBotFarmCount() {
        return BotNet.Count; // Count non-null bot farms
    }
    public static BotFarm GetBotFarm(string name) {
        foreach (BotFarm farm in BotNet) {
            if (farm == null) { QueueRemoveHackFarm(farm); continue; } // Remove null farms
            if (farm != null && farm.HostName == name) { return farm; }
        } return null;
    }
    public static BotFarm[] GetBotFarms() {
        return [.. BotNet];
    }

    public static void RegenerateDriftSector(int removalAmount=-1) {
        driftSectors = driftSectors.Where(s => s != null).ToList(); // Remove null sectors
        if (removalAmount < 0) removalAmount = driftSectors.Count; // Default to all of the current sectors
        removalAmount = Mathf.Clamp(removalAmount, 0, driftSectors.Count);
        if (removalAmount == 0) {
            while (driftSectors.Count < DRIFT_SECTOR_COUNT) { // Fill up to DRIFT_SECTOR_COUNT
                DriftSector newSector = new DriftSector();
                if (newSector == null) continue; // Skip if sector creation failed
                driftSectors.Add(newSector);
            }
            return; // Nothing to remove
        }
        driftSectors = Util.Shuffle<DriftSector>(driftSectors);
        int removedAmount = 0;
        while (removedAmount < removalAmount) {
            DisconnectFromSector(driftSectors[0]);
            RemoveSector(driftSectors[0]);
            removedAmount++;
        }
        while (driftSectors.Count < DRIFT_SECTOR_COUNT) { // Fill up to DRIFT_SECTOR_COUNT
            DriftSector newSector = new DriftSector();
            if (newSector == null) continue; // Skip if sector creation failed
            driftSectors.Add(newSector);
        }
        GC.Collect();                    // Try to collect unreachable objects
        GC.WaitForPendingFinalizers();   // Wait for destructors (~finalizers)
        GC.Collect();                    // Re-collect objects that were just finalized
    }

    public static void LoadStaticSector() {
        GD.Print("No static sector"); return;
        staticSectors = [];
        string[] sectorPaths = DirAccess.GetFilesAt("res://Utilities/Resources/ScriptedNetworkNodes/Sectors/");
        foreach (string path in sectorPaths) {
            SectorData sectorData = GD.Load<SectorData>($"res://Utilities/Resources/ScriptedNetworkNodes/Sectors/{path}");
            if (sectorData == null) continue;
            StaticSector sector = new(sectorData);
            staticSectors.Add(sector);
        }
    }
    public static int PrepareTutorial() {
        if (PlayerDataManager.CompletedTutorial) return 1;
        SectorData tutorialSecData = GD.Load<SectorData>($"res://Utilities/Resources/ScriptedNetworkNodes/Sectors/tutorialSector.tres");
        if (tutorialSecData == null) return 2;

        StaticSector tutorialSec = new(tutorialSecData);
        foreach(Sector sector in connectedSectors) DisconnectFromSector(sector);
        ConnectToSector(tutorialSec);
        return 0;
    }

    public static CError ConnectToSector(string sectorName) {
        List<Sector> nullSector = [];
        foreach (DriftSector sector in driftSectors) {
            if (sector == null) { nullSector.Add(sector); continue; }
            if (sector.Name == sectorName) return ConnectToSector(sector);
        }
        foreach(DriftSector sector in nullSector) driftSectors.Remove(sector); 
        nullSector.Clear();
        foreach (StaticSector sector in staticSectors) {
            if (sector == null) { nullSector.Add(sector); continue; }
            if (sector.Name == sectorName) return ConnectToSector(sector);
        }
        foreach (StaticSector sector in nullSector) staticSectors.Remove(sector);
        nullSector.Clear();
        return CError.NOT_FOUND;
    }
    public static CError DisconnectFromSector(string sectorName) {
        List<Sector> nullSector = [];
        foreach (DriftSector sector in driftSectors) {
            if (sector == null) { nullSector.Add(sector); continue; }
            if (sector.Name == sectorName) return DisconnectFromSector(sector);
        }
        foreach(DriftSector sector in nullSector) driftSectors.Remove(sector); 
        nullSector.Clear();
        foreach (StaticSector sector in staticSectors) {
            if (sector == null) { nullSector.Add(sector); continue; }
            if (sector.Name == sectorName) return DisconnectFromSector(sector);
        }
        foreach (StaticSector sector in nullSector) staticSectors.Remove(sector);
        nullSector.Clear();
        return CError.NOT_FOUND;
    }

    public static CError ConnectToSector(Sector sector) {
        if (sector == null) return CError.INVALID;
        if (connectedSectors.Contains(sector)) return CError.REDUCDANT;

        foreach (NetworkNode node in sector.GetSurfaceNodes()) node.ParentNode = PlayerNode;
        connectedSectors.Add(sector); return CError.OK;
    }
    public static CError DisconnectFromSector(Sector sector) {
        if (sector == null) return CError.INVALID;
        if (!connectedSectors.Contains(sector)) return CError.REDUCDANT;

        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            if (IsNodeOrDescendant(ShellCore.CurrNode, node)) {
                ShellCore.ToHome();
                break;
            }
            node.ParentNode = null;
        }

        connectedSectors.Remove(sector);
        return CError.OK;
    }

    static bool IsNodeOrDescendant(NetworkNode target, NetworkNode node) {
        if (target == null || node == null) return false;
        if (target == node) return true;
        foreach (NetworkNode child in node.ChildNode) {
            if (IsNodeOrDescendant(target, child)) return true;
        }
        return false;
    }

    public static CError RemoveSector(string sectorName) {
        foreach (DriftSector sector in driftSectors) {
            if (sector == null) continue;
            if (sector.Name == sectorName)
                return RemoveSector(sector);
        }
        foreach (StaticSector sector in staticSectors) {
            if (sector == null) continue;
            if (sector.Name == sectorName)
                return RemoveSector(sector);
        }
        return CError.NOT_FOUND;
    }
    public static CError RemoveSector(DriftSector sector) {
        if (sector == null) return CError.INVALID;
        if (!driftSectors.Contains(sector)) return CError.NOT_FOUND;
        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            RemoveNodeAndChildrenFromDNS(node); node.ParentNode = null;
        }
        driftSectors.Remove(sector);
        return CError.OK;
    }
    public static CError RemoveSector(StaticSector sector) {
        if (sector == null) return CError.INVALID;
        if (!staticSectors.Contains(sector)) return CError.NOT_FOUND;
        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            RemoveNodeAndChildrenFromDNS(node); node.ParentNode = null;
        }
        staticSectors.Remove(sector);
        return CError.OK;
    }

    static void RemoveNodeAndChildrenFromDNS(NetworkNode node) {
        if (node == null) return;
        if (!string.IsNullOrEmpty(node.IP)) {
            DNS.Remove(node.IP);
        }
        foreach (var child in node.ChildNode) {
            RemoveNodeAndChildrenFromDNS(child);
        }
    }

    readonly static Queue<BotFarm> AddQueue = [];
    public static void QueueAddHackFarm(BotFarm hackFarm) {
        GD.Print(hackFarm.HostName);
        AddQueue.Enqueue(hackFarm);
    }
    readonly static Queue<BotFarm> RemovalQueue = [];
    public static void QueueRemoveHackFarm(BotFarm hackFarm) {
        RemovalQueue.Enqueue(hackFarm);
    }
    static void ManageHackFarm(double delta) {
        while (AddQueue.Count > 0) {
            BotFarm farm = AddQueue.Dequeue();
            if (farm == null || BotNet.Contains(farm)) continue;
            BotNet.Add(farm);
        }
        while (RemovalQueue.Count > 0) {
            BotFarm farm = RemovalQueue.Dequeue();
            if (farm == null || !BotNet.Contains(farm)) continue;
            BotNet.Remove(farm);
        }
    }
    static void MineralCollection(double delta) {
        foreach (BotFarm h in BotNet) {
            if (h.LifeTime <= 0) { QueueRemoveHackFarm(h); continue; }
            double[] minerals = h.ProcessMinerals(delta);
            for (int i = 0; i < minerals.Length; ++i) {
                PlayerDataManager.DepositMineral(i, minerals[i]);
            }
        }
    }

    [Flags]
    public enum HackFarmSortType {
        HOSTNAME = 1<<0, // Sort by HostName
        LIFETIME = 1<<1, // Sort by LifeTime
        MBACKLOG = 1<<2, // Sort by MineralAmount
    }
    static readonly Comparison<BotFarm> HostnameSort = (a, b) => a.HostName.CompareTo(b.HostName);
    static readonly Comparison<BotFarm> LifetimeSort = (a, b) => a.LifeTime.CompareTo(b.LifeTime);
    static readonly Comparison<BotFarm> MBacklogSort = (a, b) => a.MBacklog.CompareTo(b.MBacklog);
    public static void SortHackFarm(HackFarmSortType sortType=HackFarmSortType.LIFETIME) {
        BotNet.RemoveAll(x => x == null); // Remove null entries
        switch (sortType) {
            case HackFarmSortType.HOSTNAME: BotNet.Sort(HostnameSort); break;
            case HackFarmSortType.LIFETIME: BotNet.Sort(LifetimeSort); break;
            case HackFarmSortType.MBACKLOG: BotNet.Sort(MBacklogSort); break;
        }
    }

    public static NetworkNode QueryDNS(string IP) {
        DNS.TryGetValue(IP, out var nodeRef);
        if (nodeRef != null && nodeRef.TryGetTarget(out NetworkNode node)) {
            return node;
        }
        DNS.Remove(IP); // Remove stale entry
        return null;
    }
    public static int AssignDNS(NetworkNode node) {
        if (string.IsNullOrEmpty(node.IP)) return 1;
        DNS[node.IP] = new WeakReference<NetworkNode>(node); return 0;
    }
    public static string GetRandomIP() {
        static string Generate() => $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}";
        DNS ??= [];
        string ip;
        do ip = Generate();
        while (DNS.ContainsKey(ip));
        return ip;
    }
    
    public static string GetSaveStatusMsg(int statusCode) {
        string[] SAVE_STATUS_MSG = [
            Util.Format("Saved botnet data successfully", StrType.FULL_SUCCESS),
            Util.Format("No botnet to save", StrType.WARNING),
        ];
        return (statusCode < SAVE_STATUS_MSG.Length) ? SAVE_STATUS_MSG[statusCode]
            : Util.Format($"{statusCode}", StrType.UNKNOWN_ERROR, "saving player data");
    }
    public static int SaveNetworkData(string filePath) {
        filePath = ProjectSettings.GlobalizePath(filePath);
        if (!DirAccess.DirExistsAbsolute(filePath)) DirAccess.MakeDirAbsolute(filePath);
        if (BotNet.Count == 0) return 1; // No bots to serialize
        HackFarmDataSaveResource[] botData = new HackFarmDataSaveResource[BotNet.Count];
        for (int i = 0; i < BotNet.Count; ++i) {
            if (BotNet[i] == null) continue;
            botData[i] = BotFarm.SerializeBotnet(BotNet[i]); ;
            Error error = ResourceSaver.Save(botData[i], StringExtensions.PathJoin(filePath, $"{botData[i].HostName}.tres"));
            if (error != Error.Ok) return (int)error; // Failed to save a botnet data file
        }
        return 0; // Successfully saved all botnet data
    }

    public static string GetLoadStatusMsg(int statusCode) {
        string[] LOAD_STATUS_MSG = [
            Util.Format("Loaded botnet data successfully", StrType.FULL_SUCCESS),
            Util.Format("No botnet data to load", StrType.WARNING),
            Util.Format("Failed to open directory with botnet data", StrType.ERROR),
            Util.Format("Failed to load a botnet data file", StrType.ERROR),
        ];
        return (statusCode < LOAD_STATUS_MSG.Length) ? LOAD_STATUS_MSG[statusCode]
            : Util.Format($"{statusCode}", StrType.UNKNOWN_ERROR, "loading player data");
    }
    public static int LoadNetworkData(string filePath) {
        filePath = ProjectSettings.GlobalizePath(filePath);
        if (!DirAccess.DirExistsAbsolute(filePath)) return 1; // No botnet data to load
        DirAccess dir = DirAccess.Open(filePath);
        if (dir == null) return 2; // Failed to open directory
        string[] files = dir.GetFiles();
        foreach (string file in files) {
            if (!file.EndsWith(".tres")) continue; // Only load .tres files
            GD.Print(StringExtensions.PathJoin(filePath, file));
            HackFarmDataSaveResource data = GD.Load<HackFarmDataSaveResource>(StringExtensions.PathJoin(filePath, file));
            if (data == null) continue; // Failed to load data
            BotFarm farm = new(data);
            QueueAddHackFarm(farm);
        }
        return 0; // Successfully loaded all botnet data
    }

    static void CleanNullValue() {
        driftSectors = driftSectors.Where(s => s != null).ToList();
        staticSectors = staticSectors.Where(s => s != null).ToList();
        connectedSectors = connectedSectors.Where(s => s != null).ToList();
        BotNet = BotNet.Where(b => b != null).ToList();
        DNS = DNS.Where(kv => kv.Value.TryGetTarget(out _)).ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
