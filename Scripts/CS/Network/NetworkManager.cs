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
    const int DRIFT_SECTOR_COUNT = 128;

    public static void Setup() {
        DNS = []; driftSectors = []; connectedSectors = []; staticSectors = []; BotNet = [];
        RegenerateDriftSector();
        LoadStaticSector();
    }
    public static void Ready() {
        _playerNode ??= new PlayerNode(Util.PLAYER_NODE_DATA_DEFAULT);
    }
    const double CYCLE_TIME = 60*15; // 15min in seconds
    static double TimeRemains = CYCLE_TIME;
    public static void Process(double delta) {
        ManageHackFarm(delta);
        ManageDriftSector(delta);
        MineralCollection(delta);
        TimeRemains -= delta;
        if (TimeRemains <= 0) {
            TimeRemains += CYCLE_TIME;
            RegenerateDriftSector((int)(DRIFT_SECTOR_COUNT * .33));
        }
    }

    public static string[] GetSectorNames(bool mustConnected=false, int level=-1) {
        List<string> names = [];
        int i = -1; foreach (var sector in driftSectors) {
            ++i; if (sector == null) { QueueRemoveDriftSector(i); continue; }
            if (level > -1 && sector.SectorLevel != level) continue; // Skip sectors that don't match the level
            if (mustConnected && !connectedSectors.Contains(sector)) continue; // Skip if mustConnected is true and sector is not connected
            names.Add(sector.Name);
        }
        i = -1; foreach (var sector in staticSectors) {
            if (level > -1) continue; // Static sectors don't have levels, so skip all static sectors if level is specified
            ++i; if (sector == null) { QueueRemoveDriftSector(i); continue; } 
            if (mustConnected && !connectedSectors.Contains(sector)) continue; // Skip if mustConnected is true and sector is not connected
            names.Add(sector.Name);
        }
        return [.. names];
    }
    
    public static string[] GetBotsName() {
        List<string> names = [];
        foreach(BotFarm hackFarm in BotNet) {
            if (hackFarm == null) { QueueRemoveHackFarm(hackFarm); continue; }
            names.Add(hackFarm.HostName);
        }
        return [.. names];
    }
    public static string[] GetBotsIP() {
        List<string> ips = [];
        foreach (BotFarm hackFarm in BotNet) {
            if (hackFarm == null) { QueueRemoveHackFarm(hackFarm); continue; }
            ips.Add(hackFarm.IP);
        }
        return [.. ips];
    }
    public static int GetBotCount() {
        return BotNet.Count; // Count non-null bot farms
    }
    public static BotFarm GetBotByName(string name) {
        foreach (BotFarm farm in BotNet) {
            if (farm == null) { QueueRemoveHackFarm(farm); continue; } // Remove null farms
            if (farm.HostName == name) { return farm; }
        } return null;
    }
    public static BotFarm GetBotByIP(string ip) {
        foreach (BotFarm farm in BotNet) {
            if (farm == null) { QueueRemoveHackFarm(farm); continue; } // Remove null farms
            if (farm.IP == ip) { return farm; }
        }
        return null;
    }
    public static BotFarm[] GetBotFarms() {
        return [.. BotNet];
    }
    public static DriftSector GetDriftSectorByName(string name) {
        foreach (DriftSector sector in driftSectors) {
            if (sector == null) { QueueRemoveDriftSector(sector); continue; } // Remove null sectors
            if (sector.Name == name) return sector;
        }
        return null;
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
        staticSectors = [new TutorialSector()];
        return;
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
        int i = -1; foreach (DriftSector sector in driftSectors) {
            ++i; if (sector == null) { QueueRemoveDriftSector(i); continue; } // Remove null sectors
            if (sector.Name == sectorName) return ConnectToSector(sector);
        }
        i = -1;  foreach (StaticSector sector in staticSectors) {
            ++i; if (sector == null) { QueueRemoveDriftSector(i); continue; } // Remove null sectors
            if (sector.Name == sectorName) return ConnectToSector(sector);
        }
        return CError.NOT_FOUND;
    }
    public static CError DisconnectFromSector(string sectorName) {
        int i = -1; foreach (DriftSector sector in driftSectors) {
            ++i; if (sector == null) { QueueRemoveDriftSector(i); continue; } // Remove null sectors
            if (sector.Name == sectorName) return DisconnectFromSector(sector);
        }
        i = -1; foreach (StaticSector sector in staticSectors) {
            ++i; if (sector == null) { QueueRemoveDriftSector(i); continue; } // Remove null sectors
            if (sector.Name == sectorName) return DisconnectFromSector(sector);
        }
        return CError.NOT_FOUND;
    }

    public static CError ConnectToSector(Sector sector) {
        if (sector == null) return CError.INVALID;
        if (connectedSectors.Contains(sector)) return CError.REDUNDANT;

        foreach (NetworkNode node in sector.GetSurfaceNodes()) node.ParentNode = PlayerNode;
        connectedSectors.Add(sector); return CError.OK;
    }
    public static CError DisconnectFromSector(Sector sector) {
        if (sector == null) return CError.INVALID;
        if (!connectedSectors.Contains(sector)) return CError.REDUNDANT;

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

    public static CError RemoveSector(int index) {
        if (index < 0 || index >= driftSectors.Count) return CError.INVALID;
        DriftSector sector = driftSectors[index];
        driftSectors.RemoveAt(index);
        return CError.OK;
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
        if (sector == null) return CError.INVALID; // Use index overload instead for null sectors
        if (!driftSectors.Contains(sector)) return CError.NOT_FOUND;
        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            RemoveNodeAndChildrenFromDNS(node); node.ParentNode = null;
        }
        driftSectors.Remove(sector);
        driftSectors.Add(new());
        return CError.OK;
    }
    public static CError RemoveSector(StaticSector sector) {
        if (sector == null) return CError.INVALID; // Use index overload instead for null sectors
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

    readonly static Queue<BotFarm> AddBotFarmQueue = [];
    public static void QueueAddHackFarm(BotFarm hackFarm) {
        AddBotFarmQueue.Enqueue(hackFarm);
    }
    readonly static Queue<BotFarm> RemoveBotFarmQueue = [];
    readonly static Queue<int> RemoveBotFarmIndexQ = new Queue<int>();
    public static void QueueRemoveHackFarm(BotFarm hackFarm) {
        RemoveBotFarmQueue.Enqueue(hackFarm);
    }
    public static void QueueRemoveHackFarm(int index) {
        RemoveBotFarmIndexQ.Enqueue(index);
    }

    readonly static Queue<DriftSector> RemoveDriftSectorQueue = [];
    readonly static Queue<int> RemoveDriftSectorIndexQ = new Queue<int>();
    public static void QueueRemoveDriftSector(DriftSector sector) {
        RemoveDriftSectorQueue.Enqueue(sector);
    }
    public static void QueueRemoveDriftSector(int index) {
        RemoveDriftSectorIndexQ.Enqueue(index); // We can check valid index later
    }

    static void ManageHackFarm(double delta) {
        while (AddBotFarmQueue.Count > 0) {
            BotFarm farm = AddBotFarmQueue.Dequeue();
            if (farm == null || BotNet.Contains(farm)) continue;
            BotNet.Add(farm);
        }
        while (RemoveBotFarmQueue.Count > 0) {
            BotFarm farm = RemoveBotFarmQueue.Dequeue();
            if (farm == null || !BotNet.Contains(farm)) continue;
            BotNet.Remove(farm);
        }
        while (RemoveBotFarmIndexQ.Count > 0) {
            int index = RemoveBotFarmIndexQ.Dequeue();
            if (index < 0 || index >= BotNet.Count) continue; // Invalid index
            BotFarm farm = BotNet[index];
            BotNet.RemoveAt(index);
        }
    }
    static void ManageDriftSector(double delta) {
        while (RemoveDriftSectorQueue.Count > 0) {
            DriftSector sector = RemoveDriftSectorQueue.Dequeue();
            if (sector == null || !driftSectors.Contains(sector)) continue;
            RemoveSector(sector);
        }
        while (RemoveDriftSectorIndexQ.Count > 0) {
            int index = RemoveDriftSectorIndexQ.Dequeue();
            RemoveSector(index); // This checks for valid index already
        }
    }
    static void MineralCollection(double delta) {
        foreach (BotFarm h in BotNet) {
            if (h.LifeTime <= 0) { QueueRemoveHackFarm(h); continue; }
            (int, long)[] minerals = h.ProcessMinerals(delta);
            for (int i = 0; i < minerals.Length; ++i) {
                PlayerDataManager.DepositMineral(minerals[i].Item1, minerals[i].Item2);
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

    public static NetworkNode GetNodeByIP(string IP) {
        DNS.TryGetValue(IP, out var nodeRef);
        if (nodeRef != null && nodeRef.TryGetTarget(out NetworkNode node)) {
            return node;
        }
        DNS.Remove(IP); // Remove stale entry
        return null;
    }
    public static int AssignNodeToIP(NetworkNode node) {
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
    
    const string BotnetFileName = "Botnet.tres", PlayerNodeFileName = "PlayerNode.tres";
    public static string GetSaveStatusMsg(int[] statusCodes) {
        statusCodes = statusCodes.Select(x => Mathf.Abs(x)).ToArray();
        string[] botnetMsgs = {
            Util.Format("Saved botnet data", StrSty.FULL_SUCCESS),
            Util.Format("No botnet to save", StrSty.WARNING),
        };
        string[] nodeMsgs = {
            Util.Format("Saved player node data", StrSty.FULL_SUCCESS),
            Util.Format("No player node to save, this behavior should never happen and should be reported.", StrSty.ERROR),
        };

        string botnetMsg = statusCodes[0] < botnetMsgs.Length
            ? botnetMsgs[statusCodes[0]]
            : Util.Format($"{statusCodes[0]}", StrSty.UNKNOWN_ERROR, "saving botnet data");

        string nodeMsg = statusCodes[1] < nodeMsgs.Length
            ? nodeMsgs[statusCodes[1]]
            : Util.Format($"{statusCodes[1]}", StrSty.UNKNOWN_ERROR, "saving player node data");

        return $"{botnetMsg}... {nodeMsg}... ";
    }

    public static int[] SaveNetworkData(string filePath) {
        filePath = ProjectSettings.GlobalizePath(filePath);
        if (!DirAccess.DirExistsAbsolute(filePath)) DirAccess.MakeDirAbsolute(filePath);
        return [
            SaveBotNet(filePath), // Save botnet data first
            SavePlayerNode(filePath), // Save player node data
        ];
    }
    static int SavePlayerNode(string filePath) {
        if (PlayerNode == null) return 1; // No player node to serialize
        NodeData playerNodeData = PlayerNode.SerializeNodeData();
        Error error = ResourceSaver.Save(playerNodeData, StringExtensions.PathJoin(filePath, PlayerNodeFileName));
        return (int)error; // Return error code
    }
    static int SaveBotNet(string filePath) {
        if (BotNet.Count == 0) return -1; // No bots to serialize
        HackFarmsDataSaveResource botnetData = new();
        botnetData.BotNet = new HackFarmDataSaveResource[BotNet.Count];
        for (int i = 0; i < BotNet.Count; ++i) {
            if (BotNet[i] == null) continue;
            botnetData.BotNet[i] = BotFarm.SerializeBotnet(BotNet[i]); ;
        }
        Error error = ResourceSaver.Save(botnetData, StringExtensions.PathJoin(filePath, BotnetFileName));
        return (int)error; // Return error code
    }

    public static string GetLoadStatusMsg(int[] statusCode) {
        string[] LOAD_BOTNET_STATUS_MSG = [
            Util.Format("Loaded botnet data", StrSty.FULL_SUCCESS),
            Util.Format("No botnet data to load", StrSty.WARNING),
            Util.Format("Failed to load botnet data file", StrSty.ERROR),
            Util.Format("Failed to load all botnet data", StrSty.ERROR),
            Util.Format("Null botnet data found", StrSty.ERROR), // This should never happen, but just in case
        ];
        string[] LOAD_PLAYER_NODE_STATUS_MSG = [
            Util.Format("Loaded player node data", StrSty.FULL_SUCCESS),
            Util.Format("No player node data to load", StrSty.WARNING),
            Util.Format("Failed to load player node data file", StrSty.ERROR),
            Util.Format("Null player node data file found. Fall back to default player node state", StrSty.ERROR),
            Util.Format("Failed to load player node data. This should never happen and should be reported.", StrSty.ERROR), // This should never happen, but just in case
        ];
        string botnetMsg = statusCode[0] < LOAD_BOTNET_STATUS_MSG.Length
            ? LOAD_BOTNET_STATUS_MSG[statusCode[0]]
            : Util.Format($"{statusCode[0]}", StrSty.UNKNOWN_ERROR, "loading botnet data");
        string playerNodeMsg = statusCode[1] < LOAD_PLAYER_NODE_STATUS_MSG.Length
            ? LOAD_PLAYER_NODE_STATUS_MSG[statusCode[1]]
            : Util.Format($"{statusCode[1]}", StrSty.UNKNOWN_ERROR, "loading player node data");
        return $"{botnetMsg}...{playerNodeMsg}... ";
    }
    public static int[] LoadNetworkData(string filePath) {
        filePath = ProjectSettings.GlobalizePath(filePath);
        if (!DirAccess.DirExistsAbsolute(filePath)) return [1, 0, 0]; // No botnet data to load
        DirAccess dir = DirAccess.Open(filePath);
        if (dir == null) return [2,0,0]; // Failed to open directory
        return [0, LoadBotNet(dir, filePath), LoadPlayerNode(dir, filePath)];
    }
    static int LoadBotNet(DirAccess dir, string filePath) {
        if (!dir.FileExists(StringExtensions.PathJoin(filePath, BotnetFileName))) { return 1; }
        HackFarmsDataSaveResource botnetData;
        try { botnetData = GD.Load<HackFarmsDataSaveResource>(StringExtensions.PathJoin(filePath, BotnetFileName)); } 
        catch { return 2; } // Failed to load botnet data
        int errCode = 0;
        for (int i = 0; i < botnetData.BotNet.Length; ++i) {
            if (botnetData.BotNet[i] == null) continue; // Skip null entries
            BotFarm farm;
            try { farm = new(botnetData.BotNet[i]); } catch { errCode = 3; continue; } // Failed to deserialize
            if (farm == null) { errCode = 4; continue; }
            QueueAddHackFarm(farm);
        }
        return errCode;
    }
    static int LoadPlayerNode(DirAccess dir, string filePath) {
        if (!dir.FileExists(StringExtensions.PathJoin(filePath, PlayerNodeFileName))) { return 1; }
        NodeData playerNodeData;
        try { playerNodeData = GD.Load<NodeData>(StringExtensions.PathJoin(filePath, PlayerNodeFileName)); } catch { return 2; } // Failed to load player node data
        if (playerNodeData == null) { PlayerNode = new(Util.PLAYER_NODE_DATA_DEFAULT); return 3; }// Null player node data
        try { PlayerNode = new PlayerNode(playerNodeData); } catch { return 4; } // Failed to deserialize player node
        AssignNodeToIP(PlayerNode);
        return 0; // Success
    }

    static void CleanNullValue() {
        driftSectors = driftSectors.Where(s => s != null).ToList();
        staticSectors = staticSectors.Where(s => s != null).ToList();
        connectedSectors = connectedSectors.Where(s => s != null).ToList();
        BotNet = BotNet.Where(b => b != null).ToList();
        DNS = DNS.Where(kv => kv.Value.TryGetTarget(out _)).ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
