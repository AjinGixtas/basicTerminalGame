using Godot;

public class BusinessNode : ScriptedNetworkNode {
    public Stock _stock;
    public Stock Stock { get => _stock; set { _stock ??= value; } }
    public BusinessNode(string hostName, string displayName, string IP, NetworkNode parentNode, HackFarm hackFarm)
        : base(hostName, displayName, IP, NodeType.BUSINESS, parentNode, hackFarm) {
    }
    // They're in it for the money, not to keep it
    public override (int, int) GenerateDefAndSec(double indexRatio, double depthRatio) {
        int seclvl = GD.RandRange(5, 9) + (int)(depthRatio * 2) + (int)(indexRatio * 3);
        int deflvl = GD.RandRange((int)Mathf.Floor(seclvl * 0.6), seclvl);
        return (seclvl, deflvl);
    }
}