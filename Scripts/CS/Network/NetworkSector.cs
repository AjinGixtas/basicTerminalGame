using System.Collections.Generic;

public class NetworkSector {
    public string Name;
    readonly List<NetworkNode> SurfaceNodes = [];
    bool _isComplete = false;
    public int AddSurfaceNode(NetworkNode node) { if (_isComplete) return 1; SurfaceNodes.Add(node); return 0; }
    public int MarkSectorAsComplete() { _isComplete = true; return 0; }
    public List<NetworkNode> GetSurfaceNodes() { return [.. SurfaceNodes]; }
}