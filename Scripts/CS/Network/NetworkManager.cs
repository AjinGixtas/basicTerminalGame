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
    static List<HackFarm> BotNet = [];
    const int DRIFT_SECTOR_COUNT = 128;

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

    public static string[] GetSectorNames() {
        List<string> names = [];
        foreach (var sector in driftSectors)
            if (sector != null) names.Add(sector.Name);
        foreach (var sector in staticSectors)
            if (sector != null) names.Add(sector.Name);
        return [.. names];
    }
    public static string[] GetConnectedSectorNames() {
        List<string> names = [];
        foreach (var sector in connectedSectors)
            if (sector != null) names.Add(sector.Name);
        return [.. names];
    }
    public static string[] GetBotnetNames() {
        List<string> names = [];
        GD.Print(BotNet.Count);
        foreach(HackFarm hackFarm in BotNet) {
            if (hackFarm != null) names.Add(hackFarm.HostName);
        }
        return [.. names];
    }

    public static HackFarm GetHackfarm(string name) {
        foreach (HackFarm farm in BotNet) {
            if (farm != null && farm.HostName == name) { return farm; }
        } return null;
    }
    public static void RegenerateDriftSector(int removalAmount=-1) {
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
            DisconnectFromSector(connectedSectors[0]);
            RemoveSector(driftSectors[0]);
            removedAmount++;
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

    public static int ConnectToSector(string sectorName) {
        foreach (DriftSector sector in driftSectors) {
            if (sector == null) continue;
            if (sector.Name == sectorName)
                return ConnectToSector(sector);
        }
        foreach (StaticSector sector in staticSectors) {
            if (sector == null) continue;
            if (sector.Name == sectorName)
                return ConnectToSector(sector);
        }
        return 3;
    }
    public static int DisconnectFromSector(string sectorName) {
        foreach (DriftSector sector in driftSectors) {
            if (sector == null) continue;
            if (sector.Name == sectorName)
                return DisconnectFromSector(sector);
        }
        foreach (StaticSector sector in staticSectors) {
            if (sector == null) continue;
            if (sector.Name == sectorName)
                return DisconnectFromSector(sector);
        }
        return 3;
    }

    public static int ConnectToSector(Sector sector) {
        if (sector == null) return 1;
        if (connectedSectors.Contains(sector)) return 2;

        foreach (NetworkNode node in sector.GetSurfaceNodes()) node.ParentNode = PlayerNode;
        connectedSectors.Add(sector); return 0;
    }
    public static int DisconnectFromSector(Sector sector) {
        if (sector == null) return 1;
        if (!connectedSectors.Contains(sector)) return 2;

        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            if (IsNodeOrDescendant(TerminalProcessor.CurrNode, node)) {
                TerminalProcessor.ToHome();
                break;
            }
            node.ParentNode = null;
        }

        connectedSectors.Remove(sector);
        return 0;
    }

    static bool IsNodeOrDescendant(NetworkNode target, NetworkNode node) {
        if (target == null || node == null) return false;
        if (target == node) return true;
        foreach (NetworkNode child in node.ChildNode) {
            if (IsNodeOrDescendant(target, child)) return true;
        }
        return false;
    }

    public static int RemoveSector(string sectorName) {
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
        return 3;
    }
    public static int RemoveSector(DriftSector sector) {
        if (sector == null) return 1;
        if (!driftSectors.Contains(sector)) return 2;
        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            RemoveNodeAndChildrenFromDNS(node); node.ParentNode = null;
        }
        driftSectors.Remove(sector);
        return 0;
    }
    public static int RemoveSector(StaticSector sector) {
        if (sector == null) return 1;
        if (!staticSectors.Contains(sector)) return 2;
        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            RemoveNodeAndChildrenFromDNS(node); node.ParentNode = null;
        }
        staticSectors.Remove(sector);
        return 0;
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

    readonly static Queue<HackFarm> AddQueue = [];
    public static void QueueAddHackFarm(HackFarm hackFarm) {
        GD.Print(hackFarm.HostName);
        AddQueue.Enqueue(hackFarm);
    }
    readonly static Queue<HackFarm> RemovalQueue = [];
    public static void QueueRemoveHackFarm(HackFarm hackFarm) {
        RemovalQueue.Enqueue(hackFarm);
    }
    static void ManageHackFarm(double delta) {
        while (AddQueue.Count > 0) {
            HackFarm farm = AddQueue.Dequeue();
            if (farm == null || BotNet.Contains(farm)) continue;
            BotNet.Add(farm);
        }
        while (RemovalQueue.Count > 0) {
            HackFarm farm = RemovalQueue.Dequeue();
            if (farm == null || !BotNet.Contains(farm)) continue;
            BotNet.Remove(farm);
        }
    }
    static void MineralCollection(double delta) {
        foreach (HackFarm h in BotNet) {
            if (h.LifeTime <= 0) { QueueRemoveHackFarm(h); continue; }
            double[] minerals = h.ProcessMinerals(delta);
            for (int i = 0; i < minerals.Length; ++i) {
                PlayerDataManager.DepositMineral(i, minerals[i]);
            }
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
            botData[i] = HackFarm.SerializeBotnet(BotNet[i]); ;
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
            HackFarm farm = new(data);
            QueueAddHackFarm(farm);
        }
        return 0; // Successfully loaded all botnet data
    }
}
