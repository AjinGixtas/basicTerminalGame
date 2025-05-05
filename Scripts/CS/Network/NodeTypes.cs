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
    public NetworkNode(string hostName, string displayName, string IP, NetworkNodeType NodeType, NetworkNode parentNode) {
        HostName = hostName; DisplayName = displayName; this.IP = IP; this.NodeType = NodeType;
        ParentNode = parentNode; ChildNode = [];
        NodeDirectory = new("~");
    }
    public virtual void Init(int SecLvl, int DefLvl, HackFarm HackFarm) {
        this.SecLvl = SecLvl; this.DefLvl = DefLvl; this.HackFarm = HackFarm;
    }
    public virtual (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (0, 0);
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
        NetworkNode node = nodeType switch {
            NetworkNodeType.PLAYER => throw new Exception("WHY ARE YOU GENERATING THE PLAYER NODE AGAIN?!!?!"),
            NetworkNodeType.PERSON =>     new PersonNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.BUSINESS => new BusinessNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.CORP =>         new CorpNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.FACTION =>   new FactionNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.HONEYPOT => new HoneypotNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.MINER =>       new MinerNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.ROUGE =>       new RougeNode(hostName, displayName, IP, parentNode),
            _ => throw new Exception("Unknown node value")
        };
        (int secLvl, int defLvl) = node.GenerateSecAndDef(indexRatio, depthRatio);
        HackFarm hackFarm = new(secLvl, indexRatio, depthRatio);
        node.Init(secLvl, defLvl, hackFarm);
        return node;
    }
}
public class PlayerNode : NetworkNode {
    public PlayerNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.PLAYER, parentNode) {
    }
}
public class PersonNode : NetworkNode {
    public PersonNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.PERSON, parentNode) {
        // Kind of just stupid in general...
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (GD.RandRange(1, 3) + (int)Math.Ceiling(depthRatio * 4) + (int)Math.Ceiling(indexRatio * 30), 
            GD.RandRange(1, DefLvl) + (int)Math.Ceiling(depthRatio * 10) + (int)Math.Ceiling(indexRatio * 4));
    }
}
public class BusinessNode : NetworkNode {
    public Stock _stock;
    public Stock Stock { get => _stock; set { _stock ??= value; } }
    public BusinessNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.BUSINESS, parentNode) {
    }
    // They're in it for the money, not to keep it
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (GD.RandRange(5, 9) + (int)(depthRatio * 2) + (int)(indexRatio * 3), 
            GD.RandRange((int)Math.Floor(DefLvl * 0.6), DefLvl)); 
    }
}
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
public class FactionNode : NetworkNode {
    Faction _faction;
    public Faction Faction { get => _faction; set { _faction ??= value; } }
    public FactionNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.FACTION, parentNode) {
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (10, GD.RandRange(3, 6) + (int)(depthRatio * 2) + (int)(indexRatio * 2));
    }
}
public class HoneypotNode : NetworkNode {
    double lastEpoch = 0;
    public static Tuple<NetworkNodeType, string, string>[][] namePool;
    public HoneypotNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.HONEYPOT, parentNode) {
        GenerateFakeData();
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (10, 0);
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
    public MinerNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.MINER, parentNode) {
    }
    // Kind of whatever, they're for production anyway.
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (GD.RandRange(2, 10), GD.RandRange(1, DefLvl - 1));
    }
}
public class RougeNode : NetworkNode {
    public RougeNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.ROUGE, parentNode) {
    }
    public override (int, int) GenerateSecAndDef(double indexRatio, double depthRatio) {
        return (10, GD.RandRange(3, 8));
    }
}