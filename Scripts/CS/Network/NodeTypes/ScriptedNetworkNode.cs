using Godot;

public class ScriptedNetworkNode : NetworkNode {
    public NodeData NodeData { get; }
    public HackFarm _hackFarm;
    public HackFarm HackFarm { get => _hackFarm; }
    public ScriptedNetworkNode(NodeData nodeData, NetworkNode parent)
        : base(
            nodeData != null ? nodeData.HostName    : "Unknown",
            nodeData != null ? nodeData.DisplayName : "Unknown",
            NetworkManager.GetRandomIP(),
            nodeData != null ? nodeData.NodeType : NodeType.VM,
            parent, nodeData.OwnedByPlayer, (int)nodeData.Locks) {
        
        NodeData = nodeData;
        if (NodeData != null) {
            _hackFarm = new HackFarm(NodeData.MAX_GROW_MINING, nodeData.hackLvl, nodeData.timeLvl, nodeData.growLvl, NodeData.MiningWeights);
            Init(NodeData.DefLvl, NodeData.SecLvl);
        }
    }
}
