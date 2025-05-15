using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
public partial class NetworkManager : Node {
    [Export] string userName = "AjinGixtas";
    public string UserName { get => userName; private set => userName = value; }
    public NetworkNode network;
    public Dictionary<string, NetworkNode> DNS;
    public override void _Ready() {
        while (DNS == null) {
            network = GenerateNetwork() as PlayerNode;
            DNS = GenerateDNS(network);
        }
        PlayerData.AddHackFarm(network.HackFarm);
    }
    public NetworkNode GenerateNetwork() {
        int[] baseNodePerDepths = [5, 6, 10, 7, 6, 4, 5, 3, 3, 1]; int totalNode = baseNodePerDepths.Sum(x => x);
        NetworkNode network = new PlayerNode("home", "Player Terminal", $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}", null) {
            HackFarm = new HackFarm(1.0, 1.0, 1, 1, 1)
        }; 
        List<Tuple<NetworkNodeType, string, string>> namePool = []; int poolIndex = 0;

        List<List<NetworkNode>> layers = [[network]];

        // Build procedural network
        Tuple<NetworkNodeType, string, string>[][] nodeNames = ReadProvidedNodeName();
        HoneypotNode.namePool = nodeNames;
        for (int i = 0; i < baseNodePerDepths.Length; ++i) {
            layers.Add([]);
            if (i < nodeNames.Length) for (int k = 0; k < nodeNames[i].Length; ++k) { namePool.Add(nodeNames[i][k]); }
            for (int k = 0; k < namePool.Count; ++k) { int _k = GD.RandRange(0, namePool.Count - 1); (namePool[k], namePool[_k]) = (namePool[_k], namePool[k]); }
            for (int j = 0; j < baseNodePerDepths[i]; ++j) {
                if (namePool.Count == 0) {
                    for (int k = 0; k < nodeNames[i].Length; ++k) { namePool.Add(nodeNames[i][k]); } ++i;
                    for (int k = 0; k < namePool.Count; ++k) { int _k = GD.RandRange(0, namePool.Count - 1); (namePool[k], namePool[_k]) = (namePool[_k], namePool[k]); }
                }
                // Read the one at the top
                Tuple<NetworkNodeType, string, string> nodeData = namePool[0];
                namePool.RemoveAt(0);
                NetworkNode parentNode = layers[i][GD.RandRange(0, layers[i].Count - 1)];
                NetworkNode node = NetworkNode.GenerateProceduralNode(nodeData.Item1, nodeData.Item2, nodeData.Item3, (double)(i + 1) / baseNodePerDepths.Length, (double)poolIndex / totalNode, parentNode);
                layers[i + 1].Add(node);
                parentNode.ChildNode.Add(node);
                ++poolIndex;
            }
        }

        // Add scripted node data
        NetworkNodeData[] scriptedNodeData = ReadScriptedNodeData();
        for (int i = 0; i < scriptedNodeData.Length; ++i) {
            NetworkNodeData nodeData = scriptedNodeData[i];
            int minDepth = Math.Clamp(nodeData.minDepth, 0, layers.Count - 1);
            int maxDepth = (nodeData.maxDepth < 0 ? (layers.Count - 1) : Math.Clamp(nodeData.maxDepth, 0, layers.Count - 1));
            int depth = GD.RandRange(minDepth, maxDepth);
            for (int j = 0; j < 20 && layers[depth].Count == 0; ++j) { depth = GD.RandRange(minDepth, maxDepth); }
            NetworkNode parentNode = layers[depth][GD.RandRange(0, layers[depth].Count - 1)];
            NetworkNode node = nodeData.networkNode;
            poolIndex = layers.Take(depth).Sum(list => list.Count) + GD.RandRange(0, layers[depth].Count);
            double indexRatio = (double)poolIndex / totalNode;
            double depthRatio = (double)(depth + 1) / layers.Count;
            (int secLvl, int defLvl) = node.GenerateSecAndDef(indexRatio, depthRatio);
            node.Init(secLvl, defLvl, new(indexRatio, depthRatio));
            node.ParentNode = parentNode;
            if (depth + 1 >= layers.Count) { layers.Add([]); }
            layers[depth + 1].Add(node);
            parentNode.ChildNode.Add(node);
            totalNode++;
        }
        return network;
    }
    public Dictionary<string, NetworkNode> GenerateDNS(NetworkNode root) {
        Dictionary<string, NetworkNode> output = new();
        Queue<NetworkNode> queue = new([root]);
        while (queue.Count > 0) {
            NetworkNode node = queue.Dequeue();
            if (output.ContainsKey(node.IP)) { return null; }
            output.Add(node.IP, node);
            foreach (NetworkNode child in node.ChildNode) { queue.Enqueue(child); }
        }
        return output;
    }
    readonly Tuple<string, NetworkNodeType>[] nodeTypeData = [
        new Tuple<string, NetworkNodeType>("Person.txt", NetworkNodeType.PERSON),
        new Tuple<string, NetworkNodeType>("Rouge.txt", NetworkNodeType.ROUGE),
        new Tuple<string, NetworkNodeType>("Honeypot.txt", NetworkNodeType.HONEYPOT),
        new Tuple<string, NetworkNodeType>("Miner.txt", NetworkNodeType.MINER),
    ];

    Tuple<NetworkNodeType, string, string>[][] ReadProvidedNodeName() {
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