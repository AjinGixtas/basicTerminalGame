using Godot;
using System.Collections.Generic;
using System;
using System.Threading;

public abstract class NetworkNode {
    public string HostName { get; protected set; }
    public string DisplayName { get; protected set; }
    public string IP { get; protected set; }
    // ^ cosmetic info
    int _secLvl, _defLvl; // Defense = Sec+Ret
    public int RetLvl { get; protected set; } // retaliation level
    public SecurityType SecType { get; protected set; }
    public NetworkNodeType NodeType { get; protected set; }

    public NetworkNode ParentNode { get; init; }
    public List<NetworkNode> ChildNode { get; protected set; }
    public NodeDirectory NodeDirectory { get; init; }
    public LockSystem LockSystem { get; init; }
    public HackFarm HackFarm { get; init; }
    public int SecLvl {
        get => _secLvl;
        protected set {
            _secLvl = Math.Clamp(value, 0, DefLvl);
            LockSystem.ActivateLock(_secLvl);
            RetLvl = DefLvl - _secLvl;
            SecType = _secLvl switch {
                < 1 => SecurityType.NOSEC,
                < 4 => SecurityType.LOSEC,
                < 7 => SecurityType.MISEC,
                < 10 => SecurityType.HISEC,
                _ => SecurityType.MASEC
            };
        }
    }
    public int DefLvl {
        get => _defLvl;
        protected set {
            value = Math.Clamp(value, 0, 10);
            SecLvl += value - _defLvl;
            _defLvl = value;
        }
    }
    public NetworkNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode) {
        HostName = hostName; DisplayName = displayName; this.IP = IP; this.NodeType = NodeType;
        ParentNode = parentNode;
        ChildNode = [];
        NodeDirectory = new("~"); HackFarm = new(SecLvl, indexRatio, depthRatio);
        LockSystem = new([new I5(), new P9(), new I13(), new P16(), new C0(), new C1(), new C3(), new M2(), new M3()]);
    }
    public virtual void Init() {
        GD.Print(DefLvl, ' ', SecLvl, ' ', SecType, ' ', RetLvl, ' ', NodeType);
    }
    public int GetDepth() {
        int output = 0;
        NetworkNode curNode = this;
        while (curNode.ParentNode != null) {
            curNode = curNode.ParentNode;
            ++output;
        }
        return output;
    }
    public virtual Dictionary<string, string> Analyze() {
        return new Dictionary<string, string>() {
            { "IP", IP },
            { "hostName",  HostName }, 
            { "displayName", DisplayName },
            { "defLvl", $"{DefLvl}" }, 
            { "secLvl", $"{SecLvl}" }, 
            { "retLvl", $"{RetLvl}" },
            { "secType", $"{SecType}" },
            { "nodeType", $"{NodeType}" }
        };
    }
    public static NetworkNode GenerateProceduralNode(NetworkNodeType nodeType, string hostName, string displayName, double indexRatio, double depthRatio, NetworkNode parentNode) {
        string IP = $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}";
        NetworkNode output = nodeType switch {
            NetworkNodeType.PLAYER => throw new Exception("WHY ARE YOU GENERATING THE PLAYER NODE AGAIN?!!?!"),
            NetworkNodeType.PERSON => new PersonNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.BUSINESS => new BusinessNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.CORP => new CorpNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.FACTION => new FactionNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.HONEYPOT => new HoneypotNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.MINER => new MinerNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.ROUGE => new MinerNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            _ => throw new Exception("Unknown node value")
        };
        output.Init();
        return output;
    }
}
public class PlayerNode : NetworkNode {
    public PlayerNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
    }
    public override void Init() { 
        base.Init();
    }
}
public class PersonNode : NetworkNode {
    public PersonNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        DefLvl = GD.RandRange(2, 6) + (int)(depthRatio * 4) + (int)(indexRatio * 3); // [2-6]+[0-3]+[0-3] = ~2 to 12 clamped later
        DefLvl = Math.Clamp(DefLvl, 1, 10);
        SecLvl = GD.RandRange(0, DefLvl);
    }
    public override void Init() {
        base.Init();
    }
}
public class BusinessNode : NetworkNode {
    public Stock Stock { get; set; }
    public BusinessNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        DefLvl = GD.RandRange(5, 8) + (int)(depthRatio * 2) + (int)(indexRatio * 3); // [5-8]+[0-2]+[0-3]
        SecLvl = GD.RandRange((int)Math.Ceiling(DefLvl * 0.6), DefLvl);
    }
    public override void Init() {
        base.Init();
    }
}
public class CorpNode : NetworkNode {
    public Stock Stock { get; set; }
    public Faction Faction { get; set; }
    public CorpNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        DefLvl = 10; SecLvl = 10;
    }
    public override void Init() {
        base.Init();
    }
}
public class FactionNode : NetworkNode {
    public Faction Faction { get; set; }
    public FactionNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        DefLvl = 10;
        SecLvl = GD.RandRange(3, 6) + (int)(depthRatio * 2) + (int)(indexRatio * 2); // [5-8]+[0-2]+[0-2]
    }
    public override void Init() {
        base.Init();
    }
}
public class HoneypotNode : NetworkNode {
    double lastEpoch = 0;
    public static Tuple<NetworkNodeType, string, string>[][] namePool;
    NetworkNodeType displayNetworkNodeType;
    public HoneypotNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        DefLvl = GD.RandRange(8, 10);
        SecLvl = GD.RandRange(2, DefLvl);
        GenerateFakeData();
    }
    public override void Init() {
        base.Init();
    }
    int fakeDefLvl = 0, fakeSecLvl = 0, fakeRetLvl = 0;
    SecurityType fakeSecType;
    public void GenerateFakeData() {
        if (Time.GetUnixTimeFromSystem() - lastEpoch < 15) { return; }
        int chosenSection = GD.RandRange(0, namePool.GetLength(0) - 1);
        int chosenElement = GD.RandRange(0, namePool[chosenSection].GetLength(0) - 1);
        Tuple<NetworkNodeType, string, string> name = namePool[chosenSection][chosenElement];
        lastEpoch = Time.GetUnixTimeFromSystem();
        HostName = name.Item2; DisplayName = name.Item3;
        displayNetworkNodeType = name.Item1;

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
    public override Dictionary<string, string> Analyze() {
        GenerateFakeData();
        return new() {
            { "IP", IP },
            { "hostName",  HostName },
            { "displayName", DisplayName },
            { "defLvl", $"{fakeDefLvl}" },
            { "secLvl", $"{fakeSecLvl}" },
            { "retLvl", $"{fakeRetLvl}" },
            { "secType", $"{fakeSecType}" },
            { "nodeType", $"{displayNetworkNodeType}" }
        };
    }

}
public class MinerNode : NetworkNode {
    public MinerNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
    }
    public override void Init() {
        DefLvl = GD.RandRange(2, 10); // Wide range
        SecLvl = GD.RandRange(1, DefLvl - 1); // Always some defense but not full
        base.Init();
    }
}
public class RougeNode : NetworkNode {
    public RougeNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
    }
    public override void Init() {
        DefLvl = 10; // Rogue = max def
        SecLvl = GD.RandRange(3, 8); // but not necessarily high sec, because they're chaotic
        base.Init();
    }
}