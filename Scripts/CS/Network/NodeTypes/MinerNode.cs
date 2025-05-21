using Godot;

public class MinerNode : NetworkNode {
    public MinerNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NodeType.MINER, parentNode) {
    }
    // Kind of whatever, they're for production anyway.
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        int seclvl = GD.RandRange(2, 10);
        int deflvl = GD.RandRange(1, seclvl - 1);
        return (seclvl, deflvl);
    }
}