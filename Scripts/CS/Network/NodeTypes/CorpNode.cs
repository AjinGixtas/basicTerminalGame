public class CorpNode : NetworkNode {
    Faction _faction; Stock _stock;
    public Stock Stock { get => _stock; set { _stock ??= value; } }
    public Faction Faction { get => _faction; set { _faction ??= value; } }
    public CorpNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.CORP, parentNode) {
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (10, 10);
    }
}