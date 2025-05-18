using Godot;

public class RougeNode : NetworkNode {
    public RougeNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.ROUGE, parentNode) {
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (10, GD.RandRange(3, 8));
    }
}