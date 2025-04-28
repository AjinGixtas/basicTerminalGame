using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
public partial class NetworkManager : Node
{
    [Export] string userName = "AjinGixtas";
    public string UserName { get => userName; private set => userName = value; }
    public NetworkNode network;
    public Dictionary<string, NetworkNode> DNS;
    public override void _Ready() {
        network = GenerateNetwork() as PlayerNode;
        DNS = GenerateDNS(network);
    }
    public NetworkNode GenerateNetwork() {
        // 6+7+15+5+3+2+4+3+1
        int[] nodePerDepths = [6, 8, 15, 6, 4, 2, 5, 3, 1]; int totalDepth = nodePerDepths.Sum(x => x);
        NetworkNodeData[] scriptedNodeData = ReadScriptedNodeData();
        NetworkNode network = new PlayerNode("home", "Player Terminal", "192.168.0.1", 0, 0, NetworkNodeType.PLAYER, null);
        Tuple<NetworkNodeType, string, string>[][] nodeNames = ReadProvidedNodeName();
        HoneypotNode.namePool = nodeNames;
        List<Tuple<NetworkNodeType, string, string>> namePool = []; int poolIndex = 0;
        List<NetworkNode> lastLayer = [network], curLayer = [];
        for(int i = 0; i < nodePerDepths.Length; ++i) {
            if(i < nodeNames.Length) for (int k = 0; k < nodeNames[i].Length; ++k) { namePool.Add(nodeNames[i][k]); }
            for (int k = 0; k < namePool.Count; ++k) { int _k = GD.RandRange(0, namePool.Count - 1); (namePool[k], namePool[_k]) = (namePool[_k], namePool[k]); }
            for(int j = 0; j < nodePerDepths[i]; ++j) {
                if (namePool.Count <= poolIndex) {
                    ++i;
                    for (int k = 0; k < nodeNames[i].Length; ++k) { namePool.Add(nodeNames[i][k]); }
                    for (int k = 0; k < namePool.Count; ++k) { int _k = GD.RandRange(0, namePool.Count - 1); (namePool[k], namePool[_k]) = (namePool[_k], namePool[k]); }
                }
                // Read the one at the top
                Tuple<NetworkNodeType, string, string> nodeData = namePool[poolIndex];
                NetworkNode parentNode = lastLayer[GD.RandRange(0, lastLayer.Count - 1)];
                NetworkNode node = NetworkNode.GenerateProceduralNode(nodeData.Item1, nodeData.Item2, nodeData.Item3, (double)(i+1)/totalDepth, (double)poolIndex/totalDepth, parentNode);
                curLayer.Add(node);
                parentNode.ChildNode.Add(node);
                ++poolIndex;
            }
            lastLayer = curLayer; curLayer = [];
        }
        return network;
    }
    public Dictionary<string, NetworkNode> GenerateDNS(NetworkNode root) {
        Dictionary<string, NetworkNode> output = new();
        Queue<NetworkNode> queue = new([root]);
        while(queue.Count > 0) {
            NetworkNode node = queue.Dequeue();
            output.Add(node.IP, node);
            foreach(NetworkNode child in node.ChildNode) { queue.Enqueue(child); }
        }
        return output;
    }
    readonly Tuple<string, NetworkNodeType>[] nodeTypeData = [
        new Tuple<string, NetworkNodeType>("Person.txt", NetworkNodeType.PERSON),
        new Tuple<string, NetworkNodeType>("Business.txt", NetworkNodeType.BUSINESS),
        new Tuple<string, NetworkNodeType>("Miner.txt", NetworkNodeType.MINER),
        new Tuple<string, NetworkNodeType>("Honeypot.txt", NetworkNodeType.HONEYPOT), 
        new Tuple<string, NetworkNodeType>("Rouge.txt", NetworkNodeType.ROUGE), 
        new Tuple<string, NetworkNodeType>("Faction.txt", NetworkNodeType.FACTION),
        new Tuple<string, NetworkNodeType>("Corp.txt", NetworkNodeType.CORP)
    ];
    readonly string[] prefix = ["p_", "b_", "c_", "f_", "h_", "m_", "r_"];

    Tuple<NetworkNodeType, string, string>[][] ReadProvidedNodeName() {
        Tuple<NetworkNodeType, string, string>[][] output = new Tuple<NetworkNodeType, string, string>[nodeTypeData.Length][];
        for (int i = 0; i < nodeTypeData.Length; i++) { output[i] = ReadNodeNameOfType(nodeTypeData[i], prefix[i]); }
        return output;

        Tuple<NetworkNodeType, string, string>[] ReadNodeNameOfType(Tuple<string, NetworkNodeType> nodeDataContainerType, string prefix) {
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
    static NetworkNodeData[] ReadScriptedNodeData() {
        NetworkNodeData[] output;
        using (var fileStream = File.OpenRead(Directory.GetCurrentDirectory() + "\\Utilities\\TextFiles\\ServerNames\\ScriptedNodeData.txt")) {
            string line; string[] splitted;
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128);
            output = new NetworkNodeData[int.Parse(streamReader.ReadLine())];
            for (int i = 0; i < output.Length; ++i) {
                line = streamReader.ReadLine(); splitted = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string hostName = splitted[0], displayName = splitted[1..].Join(" ");
                line = streamReader.ReadLine(); splitted = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int defLvl = int.Parse(splitted[0]), secLvl = int.Parse(splitted[1]);
                line = streamReader.ReadLine(); splitted = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int minDepth = int.Parse(splitted[0]), maxDepth = int.Parse(splitted[1]);
                line = streamReader.ReadLine(); splitted = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                NetworkNodeType nodeType = (NetworkNodeType)Enum.Parse(typeof(NetworkNodeType), splitted[0].ToUpper());
                Type type = Type.GetType(splitted[0] + "Node");
                //output[i] = new NetworkNodeData(minDepth, maxDepth, new type(hostName, displayName, $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}", defLvl, secLvl, nodeType, null));
            }
        }
        return output;
    }
}
public struct NetworkNodeData (int minDepth, int maxDepth, NetworkNode network) {
    public int minDepth = minDepth, maxDepth = maxDepth; // [min, max]
    public NetworkNode networkNode = network;
}