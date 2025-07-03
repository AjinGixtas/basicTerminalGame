using System;
using System.Linq;

public class ScriptedNetworkNode : NetworkNode {
    public NodeData NodeData { get; }
    public ScriptedNetworkNode(NodeData nodeData, NetworkNode parent)
        : base(
            nodeData != null ? nodeData.HostName    : "Unknown",
            nodeData != null ? nodeData.DisplayName : "Unknown",
            NetworkManager.GetRandomIP(),
            nodeData != null ? nodeData.NodeType : NodeType.VM,
            parent, nodeData.OwnedByPlayer, [.. nodeData.Locks.Cast<LocT>()]) {
        
        NodeData = nodeData;
        if (NodeData != null) {
            if (NodeData.SecLvl > 4) { throw new ArgumentException("Security level cannot be greater than 4 for scripted nodes."); }
            Init(NodeData.DefLvl, (SecLvl)NodeData.SecLvl);
        }
    }
}
