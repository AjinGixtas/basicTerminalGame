using Godot;
using System.Collections.Generic;
public static partial class NetworkManager {
    static PlayerNode _playerNode;
    public static PlayerNode PlayerNode { get => _playerNode; private set { _playerNode = value; } }
    static List<DriftSector> connectedSectors;
    static List<DriftSector> driftSectors;
    static Dictionary<string, NetworkNode> DNS;
	static List<HackFarm> PlayerHackFarm = [];
    const int DRIFT_SECTOR_COUNT = 128;
    public static void Ready() {
        DNS = []; driftSectors = []; connectedSectors = [];
        PlayerNode = new PlayerNode("home", "Player Terminal", GetRandomIP(), null, new HackFarm(1.0, 1.0, 255, 255, 255)); 
        PlayerHackFarm = [PlayerNode.HackFarm];

        AssignDNS(PlayerNode);
        RegenerateDriftSector();
    }

    public static string[] GetSectorNames() {
        string[] names = new string[driftSectors.Count];
        for (int i = 0; i < driftSectors.Count; ++i) {
            if (driftSectors[i] == null) { driftSectors.Remove(driftSectors[i]); }
            names[i] = driftSectors[i].Name;
        }
        return names;
    }
    public static void RegenerateDriftSector() {
        connectedSectors.RemoveAll(item => typeof(DriftSector) == item.GetType());
        driftSectors = [];
        for (int i = 0; i < DRIFT_SECTOR_COUNT; ++i) {
            driftSectors.Add(new DriftSector());
        }
    }

    public static int ConnectToSector(string sectorName) {
        foreach (DriftSector sector in driftSectors) {
            if (sector == null) driftSectors.Remove(sector);
            if (sector.Name != sectorName) { continue; }
            return ConnectToSector(sector);
        }
        return 3;
    }
    public static int DisconnectFromSector(string sectorName) {
        foreach (DriftSector sector in driftSectors) {
            if (sector == null) driftSectors.Remove(sector);
            if (sector.Name != sectorName) { continue; }
            return DisconnectFromSector(sector);
        }
        return 3;
    }

    public static int ConnectToSector(DriftSector sector) {
        if (sector == null) return 1;
        if (connectedSectors.Contains(sector)) return 2;

        foreach (NetworkNode node in sector.GetSurfaceNodes()) node.ParentNode = PlayerNode;
        connectedSectors.Add(sector); return 0;
    }
    public static int DisconnectFromSector(DriftSector sector) {
        if (sector == null) return 1;
        if (!connectedSectors.Contains(sector)) return 2;

        // Disconnect player if currently in this sector
        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            if (IsNodeOrDescendant(TerminalProcessor.CurrNode, node)) {
                TerminalProcessor.Home();
                break;
            }
            node.ParentNode = null;
        }

        connectedSectors.Remove(sector);
        return 0;
    }

    static bool IsNodeOrDescendant(NetworkNode target, NetworkNode node) {
        // Helper: recursively checks if target is node or any of its descendants
        if (target == null || node == null) return false;
        if (target == node) return true;
        foreach (NetworkNode child in node.ChildNode) {
            if (IsNodeOrDescendant(target, child)) return true;
        }
        return false;
    }

    public static int RemoveSector(string sectorName) {
        foreach (DriftSector sector in driftSectors) {
            if (sector == null) driftSectors.Remove(sector);
            if (sector.Name != sectorName) { continue; }
            return RemoveSector(sector);
        }
        return 3;
    }
    public static int RemoveSector(DriftSector sector) {
        if (sector == null) return 1;
        if (!driftSectors.Contains(sector)) return 2;
        // Recursively remove all nodes in the sector from DNS
        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            RemoveNodeAndChildrenFromDNS(node); node.ParentNode = null;
        }
        driftSectors.Remove(sector);
        return 0;
    }

    // Helper method to recursively remove a node and its children from DNS
    static void RemoveNodeAndChildrenFromDNS(NetworkNode node) {
        if (node == null) return;
        if (!string.IsNullOrEmpty(node.IP)) {
            DNS.Remove(node.IP);
        }
        foreach (var child in node.ChildNode) {
            RemoveNodeAndChildrenFromDNS(child);
        }
    }

    public static void AddHackFarm(HackFarm hackFarm) { 
        PlayerHackFarm.Add(hackFarm); 
    }
    public static void CollectHackFarmMinerals(double delta) {
        foreach (HackFarm h in PlayerHackFarm) {
            var minerals = h.ProcessMinerals(delta);
            for (int i = 0; i < minerals.Length; ++i) {
                // You need to implement this method to deposit minerals by type.
                PlayerDataManager.DepositMineral(i, minerals[i]);
            }
        }
    }
    public static NetworkNode QueryDNS(string IP) {
        return DNS.TryGetValue(IP, out var node) ? node : null; ;
    }
    public static int AssignDNS(NetworkNode node) {
        if (string.IsNullOrEmpty(node.IP)) return 1;
        DNS[node.IP] = node; return 0;
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
