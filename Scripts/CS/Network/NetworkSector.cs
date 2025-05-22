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
        int layer = 3, node = 3;
        (string displayName, string hostName) = GenNodeName();
        DriftNode chainNode = NetworkNode.MakeNode(NodeType.DRIFT, hostName, displayName, 0, 0, null) as DriftNode;
        AddSurfaceNode(chainNode);
        for (int i = 0; i < node-1; ++i) {
            (displayName, hostName) = GenNodeName();
            AddSurfaceNode(NetworkNode.MakeNode(NodeType.DRIFT, hostName, displayName, 0, 0, null));
        }
        for (int i = 0; i < layer; i++) {
            for (int j = 0; j < node; j++) {
                (displayName, hostName) = GenNodeName();
                NetworkNode.MakeNode(NodeType.DRIFT, hostName, displayName, 0, 0, chainNode);
            }
            chainNode = chainNode.ChildNode[0] as DriftNode;
        }
    }
    void GenerateStarNetwork() {
        if (_isIntialized) return;
        int node = 5;
        for (int i = 0; i < node; ++i) {
            (string displayName, string hostName) = GenNodeName();
            AddSurfaceNode(NetworkNode.MakeNode(NodeType.DRIFT, hostName, displayName, 0, 0, null));
        }
    }
    void GenerateVineNetwork() {
        if (_isIntialized) return;
        int vine = 2, node = 3;
        for(int i = 0; i < vine; ++i) {
            (string displayName, string hostName) = GenNodeName();
            DriftNode chainNode = NetworkNode.MakeNode(NodeType.DRIFT, hostName, displayName, 0, 0, null) as DriftNode;
            AddSurfaceNode(chainNode);
            for (int j = 0; j < node; ++j) {
                (displayName, hostName) = GenNodeName();
                chainNode = NetworkNode.MakeNode(NodeType.DRIFT, hostName, displayName, 0, 0, chainNode) as DriftNode;
            }
        }
    }
    
    static string GenSectorName() {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string sb = DRIFT_SECTOR_NAMES[GD.RandRange(0, DRIFT_SECTOR_NAMES.Length - 1)] + "_";
        for (int i = 0; i < 4; i++) {
            sb += chars[GD.RandRange(0, chars.Length - 1)];
        }
        return sb;
    }
    static (string, string) GenNodeName() {
        string baseName = DRIFT_NODE_NAMES[GD.RandRange(0, DRIFT_NODE_NAMES.Length - 1)], suffix = GetSuffix(3);
        return ($"{char.ToUpper(baseName[0])}{baseName[1..]} {suffix.ToUpper()}", $"{baseName}_{suffix}");
    }
    static string GetSuffix(int length) {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        string sb = "";
        for (int i = 0; i < length; i++) {
            sb += chars[GD.RandRange(0, chars.Length - 1)];
        } return sb;
    }
}