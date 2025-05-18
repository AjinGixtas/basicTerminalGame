using Godot;
using System.Collections.Generic;
using System;

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
    public LockSystem LockSystem { get; private set; }
    public int SecLvl {
        get => _secLvl;
        protected set {
            value = Math.Clamp(value, 0, _defLvl);
            if (_secLvl == value) return;
            _secLvl = value;
            LockSystem.LockIntialization(_secLvl);
            _retLvl = _defLvl - _secLvl;
            SecType = _secLvl switch {
                < 1 => SecurityType.NOSEC,
                < 4 => SecurityType.LOSEC,
                < 8 => SecurityType.MISEC,
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
    public int RetLvl {
        get => _retLvl;
    }
    public NetworkNode(string hostName, string displayName, string IP, NetworkNodeType NodeType, NetworkNode parentNode) {
        HostName = hostName; DisplayName = displayName; this.IP = IP; this.NodeType = NodeType;
        CurrentOwner = this; ParentNode = parentNode; ChildNode = [];
        LockSystem = new();
    }
    public virtual void Init(int SecLvl, int DefLvl, HackFarm HackFarm) {
        this.DefLvl = DefLvl; this.SecLvl = SecLvl; this.HackFarm = HackFarm;
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
    public virtual NodeAnalysis Analyze() {
        return new NodeAnalysis {
            IP = IP,
            HostName = HostName,
            DisplayName = DisplayName,
            DefLvl = DefLvl,
            SecLvl = SecLvl,
            RetLvl = _retLvl,
            SecType = SecType,
            NodeType = NodeType
        };
    }
    public static NetworkNode GenerateProceduralNode(NetworkNodeType nodeType, string hostName, string displayName, double indexRatio, double depthRatio, NetworkNode parentNode) {
        string IP = $"{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}.{GD.RandRange(0, 255)}";
        NetworkNode node = nodeType switch {
            NetworkNodeType.PLAYER => throw new Exception("WHY ARE YOU GENERATING THE PLAYER NODE AGAIN?!!?!"),
            NetworkNodeType.PERSON => new PersonNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.BUSINESS => new BusinessNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.CORP => new CorpNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.FACTION => new FactionNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.HONEYPOT => new HoneypotNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.MINER => new MinerNode(hostName, displayName, IP, parentNode),
            NetworkNodeType.ROUGE => new RougeNode(hostName, displayName, IP, parentNode),
            _ => throw new Exception("Unknown node value")
        };
        (int secLvl, int defLvl) = node.GenerateSecAndDef(indexRatio, depthRatio);
        HackFarm hackFarm = new(indexRatio, depthRatio);
        node.Init(secLvl, defLvl, hackFarm);
        return node;
    }
    NetworkNode _currentOwner = null;
    public NetworkNode CurrentOwner {
        get => _currentOwner;
        private set => _currentOwner = value;
    }
    bool _isSecure = true; bool IsSecure { get => _isSecure; set => _isSecure = value; }
    public int AttempCrackNode(Dictionary<string, string> ans, double timeStamp) {
        int result = LockSystem.CrackAttempt(ans, timeStamp);
        if (result == 0) {
            IsSecure = false;
            SecLvl = 0; DefLvl = 0;
        }
        return result;
    }
    public int TransferOwnership(NetworkNode node) {
        if (IsSecure) return 1; // Node secured, transfer impossible
        CurrentOwner = node; return 0; // Transfer successful
    }
}