using Godot;

public class ScriptedNetworkNode : NetworkNode {
    public NodeData NodeData { get; }
    public HackFarm _hackFarm;
    public HackFarm HackFarm { get => _hackFarm; }
    public ScriptedNetworkNode(NodeData nodeData, NetworkNode parent)
        : base(
            nodeData != null ? nodeData.DisplayName : "Unknown",
            nodeData != null ? nodeData.HostName    : "Unknown",
            NetworkManager.GetRandomIP(),
            nodeData != null ? nodeData.NodeType : NodeType.VM,
            parent, nodeData.OwnedByPlayer) {
        
        NodeData = nodeData;
        if (NodeData != null) {
            _hackFarm = new HackFarm(NodeData.MAX_GROW_MINING, 1, 1, 1, NodeData.MiningWeights);
            Init(NodeData.DefLvl, NodeData.SecLvl);
        }
    }
    public override (int, int) GenerateDefAndSec(double indexRatio, double depthRatio) {
        return (NodeData.DefLvl, NodeData.SecLvl);
    }
}
