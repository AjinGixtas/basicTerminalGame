using Godot;

public class TutorialNetworkNode : ScriptedNetworkNode {
    public TutorialNetworkNode(NodeData nodeData, NetworkNode parent) : base(nodeData, parent) {
        if (nodeData == null) {
            GD.PushError("NodeData is null in TutorialNetworkNode constructor.");
            return;
        }
        Init(nodeData.DefLvl, nodeData.SecLvl);
    }
}
