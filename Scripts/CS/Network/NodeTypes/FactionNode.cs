using Godot;

public class FactionNode : NetworkNode {
    Faction _faction;
    public Faction Faction { get => _faction; set { _faction ??= value; } }
    public FactionNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NodeType.FACTION, parentNode) {
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (10, GD.RandRange(3, 6) + (int)(depthRatio * 2) + (int)(indexRatio * 2));
    }
}
