using Godot;
using System.Collections.Generic;
using System;
using System.Threading;

public abstract class NetworkNode {
    public string HostName { get; protected set; }
    public string DisplayName { get; protected set; }
    public string IP { get; protected set; }
    // ^ cosmetic info
    int _secLvl, _defLvl, _retLvl; // Defense = Sec+Ret
    public SecurityType SecType { get; protected set; }
    public NetworkNodeType NodeType { get; protected set; }

    NetworkNode _parentNode; HackFarm _hackFarm;
    public NetworkNode ParentNode { get => _parentNode; set { _parentNode ??= value; } }
    public HackFarm HackFarm { get => _hackFarm; set { _hackFarm ??= value; } }
    public List<NetworkNode> ChildNode { get; init; }
    public NodeDirectory NodeDirectory { get; init; }
    public LockSystem LockSystem { get; init; }
    public int SecLvl {
        get => _secLvl;
        protected set {
            _secLvl = Math.Clamp(value, 0, _defLvl);
            LockSystem.ActivateLock(_secLvl);
            _retLvl = _defLvl - _secLvl;
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
        LockSystem = new([new I5(), new C0(), new P9(), new I13(), new P16(), new C1(), new C3(), new M2(), new M3()]);
    }
    public virtual void Init() {
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
            { "retLvl", $"{_retLvl}" },
            { "secType", $"{SecType}" },
            { "nodeType", $"{NodeType}" }
        };
    }
    public static NetworkNode GenerateProceduralNode(NetworkNodeType nodeType, string hostName, string displayName, double indexRatio, double depthRatio, NetworkNode parentNode) {
        string IP = $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}";
        NetworkNode output = nodeType switch {
            NetworkNodeType.PLAYER => throw new Exception("WHY ARE YOU GENERATING THE PLAYER NODE AGAIN?!!?!"),
            NetworkNodeType.PERSON =>     new PersonNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.BUSINESS => new BusinessNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.CORP =>         new CorpNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.FACTION =>   new FactionNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.HONEYPOT => new HoneypotNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.MINER =>       new MinerNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
            NetworkNodeType.ROUGE =>       new MinerNode(hostName, displayName, IP, indexRatio, depthRatio, nodeType, parentNode),
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
        // Kind of just stupid in general...
        DefLvl = GD.RandRange(1, 3) + (int)Math.Ceiling(depthRatio * 4) + (int)Math.Ceiling(indexRatio * 30);
        SecLvl = GD.RandRange(1, DefLvl) + (int)Math.Ceiling(depthRatio * 10) + (int)Math.Ceiling(indexRatio * 4);
    }
    public override void Init() {
        base.Init();
    }
}
public class BusinessNode : NetworkNode {
    public Stock _stock;
    public Stock Stock { get => _stock; set { _stock ??= value; } }
    public BusinessNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        // They're in it for the money, not to keep it
        DefLvl = GD.RandRange(5, 9) + (int)(depthRatio * 2) + (int)(indexRatio * 3); // [5-8]+[0-2]+[0-3]
        SecLvl = GD.RandRange((int)Math.Floor(DefLvl * 0.6), DefLvl);
    }
    public override void Init() {
        base.Init();
    }
}
public class CorpNode : NetworkNode {
    Faction _faction; Stock _stock;
    public Stock Stock { get => _stock; set { _stock ??= value; } }
    public Faction Faction { get => _faction; set { _faction ??= value; } }
    public CorpNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        // No shit, no giggle
        DefLvl = 10; SecLvl = 10;
    }
    public override void Init() {
        base.Init();
    }
}
public class FactionNode : NetworkNode {
    Faction _faction;
    public Faction Faction { get => _faction; set { _faction ??= value; } }
    public FactionNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        // Their little blip on the map for recon, sec is just to filter garbage out.
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
        // They're gonna make sure you regret ever touched them.
        DefLvl = 10; SecLvl = 0;
        GenerateFakeData();
    }
    public override void Init() {
        base.Init();
    }
    public string fakeHostName, fakeDisplayName;
    public int fakeDefLvl = 0, fakeSecLvl = 0, fakeRetLvl = 0;
    SecurityType fakeSecType; NetworkNodeType fakeDisplayNetworkNodeType;
    public void GenerateFakeData() {
        if (Time.GetUnixTimeFromSystem() - lastEpoch < 15) { return; }
        int chosenSection = GD.RandRange(0, namePool.GetLength(0) - 1);
        int chosenElement = GD.RandRange(0, namePool[chosenSection].GetLength(0) - 1);
        Tuple<NetworkNodeType, string, string> name = namePool[chosenSection][chosenElement];
        for (int i = 0; i < 10; ++i) {
            if (name.Item1 == NetworkNodeType.HONEYPOT) {
                chosenSection = GD.RandRange(0, namePool.GetLength(0) - 1);
                chosenElement = GD.RandRange(0, namePool[chosenSection].GetLength(0) - 1);
                name = namePool[chosenSection][chosenElement];
            } else { break; }
        }
        lastEpoch = Time.GetUnixTimeFromSystem();
        fakeHostName = name.Item2; fakeDisplayName = name.Item3;
        fakeDisplayNetworkNodeType = name.Item1;

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
            { "hostName",  fakeHostName },
            { "displayName", fakeDisplayName },
            { "defLvl", $"{fakeDefLvl}" },
            { "secLvl", $"{fakeSecLvl}" },
            { "retLvl", $"{fakeRetLvl}" },
            { "secType", $"{fakeSecType}" },
            { "nodeType", $"{fakeDisplayNetworkNodeType}" }
        };
    }

}
public class MinerNode : NetworkNode {
    public MinerNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        // Kind of whatever, they're for production anyway.
        DefLvl = GD.RandRange(2, 10); SecLvl = GD.RandRange(1, DefLvl - 1);
    }
    public override void Init() {
        base.Init();
    }
}
public class RougeNode : NetworkNode {
    public RougeNode(string hostName, string displayName, string IP, double indexRatio, double depthRatio, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, indexRatio, depthRatio, NodeType, parentNode) {
        // Adaptive AI ready to kick your shit in.
        DefLvl = 10; SecLvl = GD.RandRange(3, 8);
    }
    public override void Init() {
        base.Init();
    }
}