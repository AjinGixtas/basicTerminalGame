using Godot;
using System;

public class PersonNode : NetworkNode {
    public PersonNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.PERSON, parentNode) {
        // Kind of just stupid in general...
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (GD.RandRange(1, 3) + (int)Math.Ceiling(depthRatio * 4) + (int)Math.Ceiling(indexRatio * 30),
            GD.RandRange(0, 1) + (int)Math.Ceiling(depthRatio * 6) + (int)Math.Ceiling(indexRatio * 4));
    }
}