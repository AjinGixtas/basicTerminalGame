using Godot;

public class DriftNode : NetworkNode {
    public DriftNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NodeType.DRIFT, parentNode) {
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        int def = (int)Mathf.Clamp(GD.Randfn(8.4, 2.75), 1, 10);
        int sec = (int)Mathf.Clamp(GD.Randfn(def/2+3.5, .33 * Mathf.E * Mathf.Pi), 1, def);
        return (sec, def);
    }
}
