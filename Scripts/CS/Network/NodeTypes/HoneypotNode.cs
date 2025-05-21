using Godot;

public class HoneypotNode : NetworkNode {
    double lastEpoch = 0;
    public static (NetworkNodeType, string, string)[][] namePool;
    public HoneypotNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.HONEYPOT, parentNode) {
        GenerateFakeData();
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (0, 10);
    }
    public string fakeHostName, fakeDisplayName;
    public int fakeDefLvl = 0, fakeSecLvl = 0, fakeRetLvl = 0;
    SecurityType fakeSecType; NetworkNodeType fakeNodeType;
    public void GenerateFakeData() {
        if (Time.GetUnixTimeFromSystem() - lastEpoch < 15) { return; }
        int chosenSection = GD.RandRange(0, namePool.GetLength(0) - 1);
        int chosenElement = GD.RandRange(0, namePool[chosenSection].GetLength(0) - 1);
        (NetworkNodeType, string, string) name = namePool[chosenSection][chosenElement];
        for (int i = 0; i < 10; ++i) {
            if (name.Item1 == NetworkNodeType.HONEYPOT) {
                chosenSection = GD.RandRange(0, namePool.GetLength(0) - 1);
                chosenElement = GD.RandRange(0, namePool[chosenSection].GetLength(0) - 1);
                name = namePool[chosenSection][chosenElement];
            } else { break; }
        }
        lastEpoch = Time.GetUnixTimeFromSystem();
        fakeHostName = name.Item2; fakeDisplayName = name.Item3;
        fakeNodeType = name.Item1;

        fakeDefLvl = GD.RandRange(2, 10);
        fakeSecLvl = GD.RandRange(1, fakeDefLvl - 1);
        fakeRetLvl = fakeDefLvl - fakeSecLvl;
        fakeSecType = fakeSecLvl switch {
            < 1 => SecurityType.NOSEC,
            < 4 => SecurityType.LOSEC,
            < 7 => SecurityType.MISEC,
            < 10 => SecurityType.HISEC,
            _ => SecurityType.MASEC
        };
    }
    public override NodeAnalysis Analyze() {
        GenerateFakeData();
        return new NodeAnalysis {
            IP = IP,
            HostName = fakeHostName,
            DisplayName = fakeDisplayName,
            DefLvl = fakeDefLvl,
            SecLvl = fakeSecLvl,
            RetLvl = fakeRetLvl,
            SecType = fakeSecType,
            NodeType = fakeNodeType
        };
    }

}