using Godot;
using System.Linq;

public class PlayerNode : ScriptedNetworkNode {
    public PlayerNode(NodeData playerNodeData)
        : base(playerNodeData, null) {
    }
    public NodeData SerializeNodeData() {
        return new NodeData {
            NodeType = NodeData.NodeType,
            HostName = NodeData.HostName,
            DisplayName = NodeData.DisplayName,
            ParentNode = NodeData.ParentNode,
            ChildNodes = NodeData.ChildNodes,
            DefLvl = NodeData.DefLvl,
            SecLvl = NodeData.SecLvl,
            RetLvl = NodeData.RetLvl,
            GcDeposit = NodeData.GcDeposit,
            MineralsDeposit = NodeData.MineralsDeposit,
            Locks = NodeData.Locks.Select(e => (int)e).ToArray()
        };
    }
}