using Godot;
using System.Collections.Generic;
public static class NetworkManager {
    public static PlayerNode playerNode;
    public static NetworkSector[] sectors;
    static Dictionary<string, NetworkNode> DNS;
	static List<HackFarm> PlayerHackFarm = [];
    public static void Ready() {
        DNS = [];
        playerNode = new PlayerNode("home", "Player Terminal", GetRandomIP(), null) {
            HackFarm = new HackFarm(1.0, 1.0, 255, 255, 255)
        }; AssignDNS(playerNode);
        PlayerHackFarm = [playerNode.HackFarm];
        sectors = [GenerateDriftSector("DriftSector")]; // TODO: Generate sectors based on player progress
        ConnectPlayerToSector(sectors[0]);
    }
    static readonly (NetworkNodeType, string, string)[][] BASIC_NODE_DATA = ReadBasicNodeName();
    static readonly NetworkNodeData[] SCRIPTED_NODE_DATA = ReadScriptedNodeData();

    static int ConnectPlayerToSector(NetworkSector sector) {
        if (sector == null) return 1;
        foreach(NetworkNode node in sector.GetSurfaceNodes()) {
            playerNode.ChildNode.Add(node);
            node.ParentNode = playerNode;
            AssignDNS(node);
        }
        return 0;
    }
    static int DisconnectPlayerFromSector(NetworkSector sector) {
        if (sector == null) return 1;
        foreach (NetworkNode node in sector.GetSurfaceNodes()) {
            playerNode.ChildNode.Remove(node);
            node.ParentNode = null;
        }
        return 0;
    }

    static NetworkSector GenerateDriftSector(string baseName) {
        string fullName = baseName + GenerateRandomSuffix(6);
        NetworkSector sector = new(fullName);
        for(int i = 0; i < 100; ++i)
        sector.MarkIntializationCompleted();
        return sector;
    }

    static string GenerateRandomSuffix(int length) {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string sb = "";
        for (int i = 0; i < length; i++) {
            sb += chars[GD.RandRange(0, chars.Length-1)];
        }
        return sb;
    }

    public static void AddHackFarm(HackFarm hackFarm) { PlayerHackFarm.Add(hackFarm); }
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
    static string GetRandomIP() {
        static string Generate() => $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}";
        DNS ??= [];
        string ip;
        do ip = Generate();
        while (DNS.ContainsKey(ip));
        return ip;
    }


    static (NetworkNodeType, string, string)[][] ReadBasicNodeName() {
        (string, NetworkNodeType)[] nodeTypeData = [
            ("Person.txt", NetworkNodeType.PERSON),
            ("Rouge.txt", NetworkNodeType.ROUGE),
            ("Honeypot.txt", NetworkNodeType.HONEYPOT),
            ("Miner.txt", NetworkNodeType.MINER),
        ];
        (NetworkNodeType, string, string)[][] output = new (NetworkNodeType, string, string)[nodeTypeData.Length][];
        for (int i = 0; i < nodeTypeData.Length; i++) {
            output[i] = ReadNodeNameOfType(nodeTypeData[i].Item1, nodeTypeData[i].Item2);
        }
        return output;

        static (NetworkNodeType, string, string)[] ReadNodeNameOfType(string fileName, NetworkNodeType nodeType) {
            FileAccess fileAccess = FileAccess.Open($"res://Utilities/TextFiles/ServerNames/{fileName}", FileAccess.ModeFlags.Read);

            int count = int.Parse(fileAccess.GetLine());
            var output = new (NetworkNodeType, string, string)[count];

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