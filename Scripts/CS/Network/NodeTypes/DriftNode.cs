using Godot;

public class DriftNode : NetworkNode {
    public DriftNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.DRIFT, parentNode) {
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        int sec = (int)Mathf.Clamp(GD.Randfn(7, 2.5), 0, 10);
        return (sec, GD.RandRange(0, sec));
    }
}
