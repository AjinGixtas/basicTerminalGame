using Godot;

public class ScriptedNetworkNode : NetworkNode {
    public NodeData NodeData { get; }
    public ScriptedNetworkNode(NodeData nodeData, NetworkNode parent)
        : base(
            nodeData != null ? nodeData.HostName    : "Unknown",
            nodeData != null ? nodeData.DisplayName : "Unknown",
            NetworkManager.GetRandomIP(),
            nodeData != null ? nodeData.NodeType : NodeType.VM,
            parent, nodeData.OwnedByPlayer, (int)nodeData.Locks) {
        
        NodeData = nodeData;
        if (NodeData != null) {
            Init(NodeData.DefLvl, NodeData.SecLvl);
        }
    }
}
