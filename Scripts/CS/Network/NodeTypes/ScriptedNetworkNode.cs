public abstract class ScriptedNetworkNode : NetworkNode {
    HackFarm _hackFarm; public HackFarm HackFarm { get => _hackFarm; set { _hackFarm ??= value; } }
    public ScriptedNetworkNode(string hostName, string displayName, string IP, NodeType nodeType, NetworkNode parentNode, HackFarm hackFarm)
        : base(hostName, displayName, IP, nodeType, parentNode) {
        HackFarm = hackFarm;
    }
}