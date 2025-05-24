public class CorpNode : ScriptedNetworkNode {
    Faction _faction; Stock _stock;
    public Stock Stock { get => _stock; set { _stock ??= value; } }
    public Faction Faction { get => _faction; set { _faction ??= value; } }
    public CorpNode(string hostName, string displayName, string IP, NetworkNode parentNode, HackFarm hackFarm)
        : base(hostName, displayName, IP, NodeType.CORP, parentNode, hackFarm) {
    }
    public override (int, int) GenerateDefAndSec(double indexRatio, double depthRatio) {
        return (10, 10);
    }
}