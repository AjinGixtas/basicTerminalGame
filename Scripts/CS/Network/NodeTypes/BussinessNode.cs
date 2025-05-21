using Godot;

public class BusinessNode : NetworkNode {
    public Stock _stock;
    public Stock Stock { get => _stock; set { _stock ??= value; } }
    public BusinessNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.BUSINESS, parentNode) {
    }
    // They're in it for the money, not to keep it
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        int seclvl = GD.RandRange(5, 9) + (int)(depthRatio * 2) + (int)(indexRatio * 3);
        int deflvl = GD.RandRange((int)Mathf.Floor(seclvl * 0.6), seclvl);
        return (seclvl, deflvl);
    }
}