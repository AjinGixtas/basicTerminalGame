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
    public int depth { get; init; } // depth of node in the network
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
    public NetworkNode(string hostName, string displayName, string IP, int depth, NetworkNodeType NodeType, NetworkNode parentNode) {
        HostName = hostName; DisplayName = displayName; this.IP = IP; this.NodeType = NodeType; this.depth = depth;
        ParentNode = parentNode;
        ChildNode = [];
        NodeDirectory = new("~"); HackFarm = new(SecLvl, depth);
        LockSystem = new([new I5(), new P9(), new I13(), new P16(), new C0(), new C1(), new C3(), new M2(), new M3()]);
    }
    public abstract void Init();
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
    public static NetworkNode GenerateProceduralNode(NetworkNodeType nodeType, string hostName, string displayName, int depth, NetworkNode parentNode) {
        string IP = $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}";
        NetworkNode output = nodeType switch {
            NetworkNodeType.PLAYER => throw new Exception("WHY ARE YOU GENERATING THE PLAYER NODE AGAIN?!!?!"),
            NetworkNodeType.PERSON => new PersonNode(hostName, displayName, IP, depth, nodeType, parentNode),
            NetworkNodeType.BUSINESS => new BusinessNode(hostName, displayName, IP, depth, nodeType, parentNode),
            NetworkNodeType.CORP => new CorpNode(hostName, displayName, IP, depth, nodeType, parentNode),
            NetworkNodeType.FACTION => new FactionNode(hostName, displayName, IP, depth, nodeType, parentNode),
            NetworkNodeType.HONEYPOT => new HoneypotNode(hostName, displayName, IP, depth, nodeType, parentNode),
            NetworkNodeType.MINER => new MinerNode(hostName, displayName, IP, depth, nodeType, parentNode),
            NetworkNodeType.ROUGE => new MinerNode(hostName, displayName, IP, depth, nodeType, parentNode),
            _ => throw new Exception("Unknown node value")
        };
        output.Init();
        return output;
    }
}
public class PlayerNode : NetworkNode {
    public PlayerNode(string hostName, string displayName, string IP, int depth, NetworkNodeType NodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, depth, NodeType, parentNode) {
    }
    public override void Init() { }
}
public class PersonNode : NetworkNode {
    public PersonNode(string hostName, string displayName, string IP, int depth, NetworkNodeType nodeType, NetworkNode parentNode)
        : base(hostName, displayName, IP, depth, nodeType, parentNode) {
    }
    public override void Init() {
        DefLvl = GD.RandRange(0, depth*depth); SecLvl = DefLvl - (int)GD.Randfn(0, 3.45575191895); // People aren't too tech savvy in general. Retaliation mostly come from antivirus
    }
}
public class BusinessNode : NetworkNode {
    public Stock Stock { get; set; }
    public BusinessNode(string hostName, string displayName, string IP, int depth, NetworkNodeType nodeType, NetworkNode parentNode)
    : base(hostName, displayName, IP, depth, nodeType, parentNode) {
    }
    public override void Init() {
        DefLvl = (int)GD.Randfn(depth*depth-2, Math.PI * .4321); SecLvl = Math.Max(1, DefLvl-1); // No risk taking, still could be safer, but it's civillian doing shops.
    }
}
public class CorpNode : NetworkNode {
    public Stock Stock { get; set; }
    public Faction Faction { get; set; }
    public CorpNode(string hostName, string displayName, string IP, int depth, NetworkNodeType nodeType, NetworkNode parentNode)
    : base(hostName, displayName, IP, depth, nodeType, parentNode) {
    }
    public override void Init() {
        DefLvl = 10; SecLvl = 10; // No risk taking, they want to ensure maximum security
    }
}
public class FactionNode : NetworkNode {
    public Faction Faction { get; set; }
    public FactionNode(string hostName, string displayName, string IP, int depth, NetworkNodeType nodeType, NetworkNode parentNode)
    : base(hostName, displayName, IP, depth, nodeType, parentNode) {
    }
    public override void Init() {
        DefLvl = (int)Math.Max(1, depth*depth*.4); SecLvl = 0; // This is just their "official" presence by proxy, they don't give a shit
    }
}
public class HoneypotNode : NetworkNode {
    double lastEpoch;
    public static Tuple<NetworkNodeType, string, string>[][] namePool;
    NetworkNodeType displayNetworkNodeType;
    public HoneypotNode(string hostName, string displayName, string IP, int depth, NetworkNodeType nodeType, NetworkNode parentNode)
    : base(hostName, displayName, IP, depth, nodeType, parentNode) {
        lastEpoch = Time.GetUnixTimeFromSystem();
    }
    public override void Init() {
        DefLvl = 10; SecLvl = depth; // Maximum point to make sure you get punish on failed lock.
    }
    public override Dictionary<string, string> Analyze() {
        if(Time.GetUnixTimeFromSystem() - lastEpoch > 10) {
            int chosenSection = GD.RandRange(0, namePool.GetLength(0) - 1);
            int chosenElement = GD.RandRange(0, namePool[chosenSection].GetLength(0) - 1);
            Tuple<NetworkNodeType, string, string> name = namePool[chosenSection][chosenElement];
            HostName = name.Item2; DisplayName = name.Item3; 
            displayNetworkNodeType = name.Item1;
        }
        return new() {
            { "IP", IP },
            { "hostName",  HostName },
            { "displayName", DisplayName },
            { "defLvl", $"{DefLvl}" },
            { "secLvl", $"{SecLvl}" },
            { "retLvl", $"{RetLvl}" },
            { "secType", $"{SecType}" },
            { "nodeType", $"{displayNetworkNodeType}" }
        };
    }

}
public class MinerNode : NetworkNode {
    public MinerNode(string hostName, string displayName, string IP, int depth, NetworkNodeType nodeType, NetworkNode parentNode)
    : base(hostName, displayName, IP, depth, nodeType, parentNode) {
    }
    public override void Init() {
        DefLvl = 1; SecLvl = 1; // Increase later on as competitor steal this node
    }
}
public class RougeNode : NetworkNode {
    public RougeNode(string hostName, string displayName, string IP, int depth, NetworkNodeType nodeType, NetworkNode parentNode)
    : base(hostName, displayName, IP, depth, nodeType, parentNode) {
    }
    public override void Init() {
        DefLvl = 10; SecLvl = GD.RandRange(1, 3); // Just enough to filter the spam and break the target worth their while.
    }
}