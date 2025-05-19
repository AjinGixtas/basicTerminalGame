using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
public static class NetworkManager {
    public static PlayerNode playerNode;
    public static NetworkNode currentNetwork;
    public static NetworkNode[] networks;
    static Dictionary<string, NetworkNode> DNS;
	static List<HackFarm> PlayerHackFarm = new();
    public static void Ready() {
        DNS = [];
        playerNode = new PlayerNode("home", "Player Terminal", GetRandomIP(), null) {
            HackFarm = new HackFarm(1.0, 1.0, 255, 255, 255)
        }; AssignDNS(playerNode);
        PlayerHackFarm = [playerNode.HackFarm];

        currentNetwork = playerNode;
    }
    public static void AddHackFarm(HackFarm hackFarm) { PlayerHackFarm.Add(hackFarm); }
    public static void CollectHackFarmMoney(double delta) {
        foreach (HackFarm h in PlayerHackFarm) {
            PlayerDataManager.Deposit(h.Process(delta));
        }
    }

    public static NetworkNode QueryDNS(string IP) {
        if (!DNS.TryGetValue(IP, out NetworkNode value)) return null;
        return value;
    }
    public static int AssignDNS(NetworkNode node) {
        if (string.IsNullOrEmpty(node.IP)) return 1;
        DNS[node.IP] = node;
        return 0;
    }
    static string GetRandomIP() {
        string IP = $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}";
        while (DNS.ContainsKey(IP)) {
            IP = $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}";
        }
        return IP;
    }

    static readonly Tuple<string, NetworkNodeType>[] nodeTypeData = [
        new Tuple<string, NetworkNodeType>("Person.txt", NetworkNodeType.PERSON),
        new Tuple<string, NetworkNodeType>("Rouge.txt", NetworkNodeType.ROUGE),
        new Tuple<string, NetworkNodeType>("Honeypot.txt", NetworkNodeType.HONEYPOT),
        new Tuple<string, NetworkNodeType>("Miner.txt", NetworkNodeType.MINER),
    ];
    static Tuple<NetworkNodeType, string, string>[][] ReadProvidedNodeName() {
        Tuple<NetworkNodeType, string, string>[][] output = new Tuple<NetworkNodeType, string, string>[nodeTypeData.Length][];
        for (int i = 0; i < nodeTypeData.Length; i++) { output[i] = ReadNodeNameOfType(nodeTypeData[i]); }
        return output;

        Tuple<NetworkNodeType, string, string>[] ReadNodeNameOfType(Tuple<string, NetworkNodeType> nodeDataContainerType) {
            Tuple<NetworkNodeType, string, string>[] output;
            using (var fileStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Utilities", "TextFiles", "ServerNames", nodeDataContainerType.Item1)))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128)) {
                output = new Tuple<NetworkNodeType, string, string>[int.Parse(streamReader.ReadLine())];
                string[] splitted;
                for (int i = 0; i < output.Length; ++i) {
                    splitted = streamReader.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    output[i] = new Tuple<NetworkNodeType, string, string>(nodeDataContainerType.Item2, splitted[0], splitted[1..].Join(" "));
                }
            }
            return output;
        }
    }
    static readonly string[] CATEGORY = ["Corp", "Faction", "Business"];
    const string INTIAL_SCRIPTED_NODE_FILEPATH = "res://Utilities/Resources/ScriptedNetworkNodes/";
    static NetworkNodeData[] ReadScriptedNodeData() {
        List<NetworkNodeData> output = new();

        for (int i = 0; i < CATEGORY.Length; ++i) {
            string folderPath = Path.Combine(INTIAL_SCRIPTED_NODE_FILEPATH, CATEGORY[i]);
            DirAccess folder = DirAccess.Open(folderPath);
            if (folder == null) {
                GD.PrintErr($"Failed to open directory: {folderPath}");
                continue;
            }

            string[] fileNames = folder.GetFiles();
            foreach (string fileName in fileNames) {
                string filePath = Path.Combine(folderPath, fileName);
                string IP = $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}";
                switch (CATEGORY[i]) {
                    case "Corp": {
                            CorpData corpData = GD.Load<CorpData>(filePath);
                            if (corpData != null) {
                                output.Add(new(
                                    corpData.minDepth,
                                    corpData.maxDepth,
                                    new CorpNode(corpData.hostName, corpData.displayName, IP, null) {
                                        Faction = new Faction(corpData.f_name, corpData.f_desc, 0, 0),
                                        Stock = new(corpData.s_symbol, corpData.s_price, corpData.s_drift, corpData.s_volatility, 0)
                                    }
                                ));
                                GD.Print($"Loaded CorpData: {corpData.hostName}, {corpData.displayName}");
                            }
                            break;
                        }
                    case "Faction": {
                            FactionData factionData = GD.Load<FactionData>(filePath);
                            if (factionData != null) {
                                output.Add(new(
                                    factionData.minDepth,
                                    factionData.maxDepth,
                                    new FactionNode(factionData.hostName, factionData.displayName, IP, null) {
                                        Faction = new(factionData.f_name, factionData.f_desc, 0, 0)
                                    }
                                ));
                                GD.Print($"Loaded FactionData: {factionData.hostName}, {factionData.displayName}");
                            }
                            break;
                        }
                    case "Business": {
                            BusinessData businessData = GD.Load<BusinessData>(filePath);
                            if (businessData != null) {
                                output.Add(new(
                                    businessData.minDepth,
                                    businessData.maxDepth,
                                    new BusinessNode(businessData.hostName, businessData.displayName, IP, null) {
                                        Stock = new(businessData.symbol, businessData.price, businessData.drift, businessData.volatility, 0)
                                    }
                                ));
                                GD.Print($"Loaded BusinessData: {businessData.hostName}, {businessData.displayName}");
                            }
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