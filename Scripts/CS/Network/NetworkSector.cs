using System.Collections.Generic;

public class NetworkSector {
    public NetworkSector(string name) {
        Name = name;
    }
    public string Name;
    readonly List<NetworkNode> SurfaceNodes = [];
    bool _isIntialized = false;
    public int AddSurfaceNode(NetworkNode node) { if (_isIntialized) return 1; SurfaceNodes.Add(node); return 0; }
    public int MarkIntializationCompleted() { _isIntialized = true; return 0; }
    public List<NetworkNode> GetSurfaceNodes() { return [.. SurfaceNodes]; }
}