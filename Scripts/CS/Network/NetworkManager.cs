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
        PlayerNode = new PlayerNode("home", "Player Terminal", GetRandomIP(), null) {
            HackFarm = new HackFarm(1.0, 1.0, 255, 255, 255)
        }; AssignDNS(PlayerNode);
        PlayerHackFarm = [PlayerNode.HackFarm];
        for (int i = 0; i < DRIFT_SECTOR_COUNT; ++i) {
            driftSectors.Add(new DriftSector());
        }
    }

    public static string[] GetSectorNames() {
        string[] names = new string[driftSectors.Count];
        for (int i = 0; i < driftSectors.Count; ++i) {
            if (driftSectors[i] == null) { driftSectors.Remove(driftSectors[i]); }
            names[i] = driftSectors[i].Name;
        }
        return names;
    }
    public static void GenerateDriftSector() {
        connectedSectors.RemoveAll(item => typeof(DriftSector) == item.GetType());
        driftSectors = [];
        for (int i = 0; i < DRIFT_SECTOR_COUNT; ++i) {
            driftSectors.Add(new DriftSector());
        }
    }

    static readonly (NodeType, string, string)[][] BASIC_NODE_DATA = ReadBasicNodeName();
    static readonly NetworkNodeData[] SCRIPTED_NODE_DATA = ReadScriptedNodeData();

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
    public static void CollectHackFarmMoney(double delta) {
        foreach (HackFarm h in PlayerHackFarm) { PlayerDataManager.Deposit(h.Process(delta)); }
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
    static (NodeType, string, string)[][] ReadBasicNodeName() {
        (string, NodeType)[] nodeTypeData = [
            ("Person.txt", NodeType.PERSON),
            ("Rouge.txt", NodeType.ROUGE),
            ("Honeypot.txt", NodeType.HONEYPOT),
            ("Miner.txt", NodeType.MINER),
        ];
        (NodeType, string, string)[][] output = new (NodeType, string, string)[nodeTypeData.Length][];
        for (int i = 0; i < nodeTypeData.Length; i++) {
            output[i] = ReadNodeNameOfType(nodeTypeData[i].Item1, nodeTypeData[i].Item2);
        }
        return output;

        static (NodeType, string, string)[] ReadNodeNameOfType(string fileName, NodeType nodeType) {
            FileAccess fileAccess = FileAccess.Open($"res://Utilities/TextFiles/ServerNames/{fileName}", FileAccess.ModeFlags.Read);

            int count = int.Parse(fileAccess.GetLine());
            var output = new (NodeType, string, string)[count];

            for (int i = 0; i < count; ++i) {
                string[] parts = StringExtensions.Split(fileAccess.GetLine(), " ", false);
                output[i] = (nodeType, parts[0], string.Join(" ", parts[1..]));
            }

            return output;
        }
    }
    static NetworkNodeData[] ReadScriptedNodeData() {
        string[] CATEGORY = [ "Corp", "Faction", "Business" ];
        List<NetworkNodeData> output = [];
        for (int i = 0; i < CATEGORY.Length; ++i) {
            DirAccess dirAccess = DirAccess.Open($"res://Utilities/Resources/ScriptedNetworkNodes/{CATEGORY[i]}");

            string[] fileNames = dirAccess.GetFiles();
            foreach (string fileName in fileNames) {
                string filePath = $"res://Utilities/Resources/ScriptedNetworkNodes/{CATEGORY[i]}/{fileName}";
                string IP = NetworkManager.GetRandomIP();
                switch (CATEGORY[i]) {
                    case "Corp": {
                            CorpData corpData = GD.Load<CorpData>(filePath);
                            if (corpData != null) { break; }
                            output.Add(new(
                                corpData.minDepth, corpData.maxDepth,
                                new CorpNode(corpData.hostName, corpData.displayName, IP, null) {
                                    Faction = new Faction(corpData.f_name, corpData.f_desc, 0, 0),
                                    Stock = new(corpData.s_symbol, corpData.s_price, corpData.s_drift, corpData.s_volatility, 0)
                                }
                            ));
                            GD.Print($"Loaded CorpData: {corpData.hostName}, {corpData.displayName}");
                            break;
                        }
                    case "Faction": {
                            FactionData factionData = GD.Load<FactionData>(filePath);
                            if (factionData == null) { break; }
                            output.Add(new(
                                factionData.minDepth, factionData.maxDepth,
                                new FactionNode(factionData.hostName, factionData.displayName, IP, null) {
                                    Faction = new(factionData.f_name, factionData.f_desc, 0, 0)
                                }
                            ));
                            GD.Print($"Loaded FactionData: {factionData.hostName}, {factionData.displayName}");
                            break;
                        }
                    case "Business": {
                            BusinessData businessData = GD.Load<BusinessData>(filePath);
                            if (businessData != null) { break; }
                            output.Add(new(
                                businessData.minDepth, businessData.maxDepth,
                                new BusinessNode(businessData.hostName, businessData.displayName, IP, null) {
                                    Stock = new(businessData.symbol, businessData.price, businessData.drift, businessData.volatility, 0)
                                }
                            ));
                            GD.Print($"Loaded BusinessData: {businessData.hostName}, {businessData.displayName}");
                            break;
                        }
                    default:
                        GD.PrintErr($"Unknown category: {CATEGORY[i]}");
                        break;
                }
            }
        }

        return [.. output];
    }
}
public struct NetworkNodeData (int minDepth, int maxDepth, NetworkNode network) {
    public int minDepth = minDepth, maxDepth = maxDepth;
    public NetworkNode networkNode = network;
}