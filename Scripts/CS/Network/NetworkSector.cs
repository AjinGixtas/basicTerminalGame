using Godot;
using System.Collections.Generic;

public class NetworkSector {
    static readonly string[] DRIFT_NODE_NAMES = StringExtensions.Split(FileAccess.Open("res://Utilities/TextFiles/ServerNames/DriftNode.txt", FileAccess.ModeFlags.Read).GetAsText(), "\n", false);
    static readonly string[] DRIFT_SECTOR_NAMES = StringExtensions.Split(FileAccess.Open("res://Utilities/TextFiles/ServerNames/DriftSector.txt", FileAccess.ModeFlags.Read).GetAsText(), "\n", false);
    public NetworkSector() {
        Name = GenSectorName();
        int type = GD.RandRange(0, 2);
        switch (type) {
            case 0: GenerateBusNetwork(); break;
            case 1: GenerateStarNetwork(); break;
            case 2: GenerateVineNetwork(); break;
            default: GD.PrintErr("Invalid network type"); break;
        }
        MarkIntializationCompleted();
    }
    public string Name;
    readonly List<NetworkNode> SurfaceNodes = [];
    bool _isIntialized = false;
    int AddSurfaceNode(NetworkNode node) { if (_isIntialized) return 1; SurfaceNodes.Add(node); return 0; }
    public int MarkIntializationCompleted() { _isIntialized = true; return 0; }
    public List<NetworkNode> GetSurfaceNodes() { return [.. SurfaceNodes]; }
    void GenerateBusNetwork() {
        if (_isIntialized) return;
        int layer = GD.RandRange(2, 3), node = 3;
        string nodeName = GenNodeName();
        DriftNode chainNode = NetworkNode.MakeNode(NodeType.DRIFT, nodeName, nodeName, 0, 0, null) as DriftNode;
        AddSurfaceNode(chainNode);
        for (int i = 0; i < node-1; ++i) {
            nodeName = GenNodeName();
            AddSurfaceNode(NetworkNode.MakeNode(NodeType.DRIFT, nodeName, nodeName, 0, 0, null));
        }
        for (int i = 0; i < layer; i++) {
            for (int j = 0; j < node; j++) {
                nodeName = GenNodeName();
                NetworkNode.MakeNode(NodeType.DRIFT, nodeName, nodeName, 0, 0, chainNode);
            }
            chainNode = chainNode.ChildNode[0] as DriftNode;
        }
    }
    void GenerateStarNetwork() {
        if (_isIntialized) return;
        int node = GD.RandRange(5, 10);
        for (int i = 0; i < node; ++i) {
            string nodeName = GenNodeName();
            AddSurfaceNode(NetworkNode.MakeNode(NodeType.DRIFT, nodeName, nodeName, 0, 0, null));
        }
    }
    void GenerateVineNetwork() {
        if (_isIntialized) return;
        int vine = GD.RandRange(3, 5);
        for(int i = 0; i < vine; ++i) {
            int node = GD.RandRange(3, 5); string nodeName = GenNodeName();
            DriftNode chainNode = NetworkNode.MakeNode(NodeType.DRIFT, nodeName, nodeName, 0, 0, null) as DriftNode;
            AddSurfaceNode(chainNode);
            for (int j = 0; j < node; ++j) {
                nodeName = GenNodeName();
                chainNode = NetworkNode.MakeNode(NodeType.DRIFT, nodeName, nodeName, 0, 0, chainNode) as DriftNode;
            }
        }
    }
    
    static string GenSectorName() {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string sb = DRIFT_SECTOR_NAMES[GD.RandRange(0, DRIFT_SECTOR_NAMES.Length - 1)] + "_";
        for (int i = 0; i < 6; i++) {
            sb += chars[GD.RandRange(0, chars.Length - 1)];
        }
        return sb;
    }
    static string GenNodeName() {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        string sb = DRIFT_NODE_NAMES[GD.RandRange(0, DRIFT_NODE_NAMES.Length - 1)] + "_";
        for (int i = 0; i < 6; i++) {
            sb += chars[GD.RandRange(0, chars.Length - 1)];
        }
        return sb;
    }
}