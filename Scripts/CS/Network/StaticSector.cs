using Godot;
using System.Collections.Generic;

public class StaticSector : Sector {
    private SectorData _sectorData;

    public StaticSector(SectorData sectorData) {
        _sectorData = sectorData;
        Name = sectorData.SectorName;
        if (sectorData.Nodes != null) {
            foreach (var nodeData in sectorData.Nodes) {
                ScriptedNetworkNode node = CreateNodeRecursive(nodeData, null);
                if (node is ScriptedNetworkNode scriptedNode)
                    SurfaceNodes.Add(scriptedNode);
            }
        }
        MarkIntializationCompleted();
    }

    private ScriptedNetworkNode CreateNodeRecursive(NodeData nodeData, NetworkNode parent) {
        ScriptedNetworkNode node = new ScriptedNetworkNode(nodeData, parent);
        if (nodeData.ChildNodes != null) {
            foreach (NodeData childData in nodeData.ChildNodes) {
                ScriptedNetworkNode childNode = CreateNodeRecursive(childData, node);
                childNode.ParentNode = node;
            }
        }
        return node;
    }

    public int MarkIntializationCompleted() {
        _isIntialized = true;
        return 0;
    }

    public List<NetworkNode> GetSurfaceNodes() {
        return [.. SurfaceNodes];
    }
}
