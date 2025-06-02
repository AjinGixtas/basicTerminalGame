using Godot;
using System;
using System.Collections.Generic;

public static partial class NetworkManager {
    static PlayerNode _playerNode;
    public static PlayerNode PlayerNode { get => _playerNode; private set { _playerNode = value; } }
    static List<Sector> connectedSectors;
    static List<DriftSector> driftSectors;
    static List<StaticSector> staticSectors;
    static Dictionary<string, WeakReference<NetworkNode>> DNS;
    static List<HackFarm> BotNet = [];
    const int DRIFT_SECTOR_COUNT = 2;

    public static void Ready() {
        DNS = []; driftSectors = []; connectedSectors = []; staticSectors = [];
        PlayerNode = new PlayerNode(GD.Load<NodeData>("res://Utilities/Resources/ScriptedNetworkNodes/PlayerNode.tres"));

        AssignDNS(PlayerNode);
        RegenerateDriftSector();
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
    public static void RegenerateDriftSector() {
        while(connectedSectors.Count > 0) {
            DisconnectFromSector(connectedSectors[0]);
        }
        while(driftSectors.Count > 0) {
            RemoveSector(driftSectors[0]);
        }
        for (int i = 0; i < DRIFT_SECTOR_COUNT; ++i) {
            driftSectors.Add(new DriftSector());
        }
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
        AddQueue.Enqueue(hackFarm);
    }
    readonly static Queue<HackFarm> RemovalQueue = [];
    public static void QueueRemoveHackFarm(HackFarm hackFarm) {
        RemovalQueue.Enqueue(hackFarm);
    }
    public static void CollectHackFarmMinerals(double delta) {
        while (AddQueue.Count > 0) {
            HackFarm farm = AddQueue.Dequeue();
            if (farm == null || BotNet.Contains(farm)) continue;
            BotNet.Add(farm);
        }
        while (RemovalQueue.Count > 0) {
            HackFarm farm = RemovalQueue.Dequeue();
            if (farm == null || BotNet.Contains(farm)) continue;
            BotNet.Remove(farm);
        }
        foreach (HackFarm h in BotNet) {
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
}
