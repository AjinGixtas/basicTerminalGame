public class PlayerNode : ScriptedNetworkNode {
    public PlayerNode(string hostName, string displayName, string IP, NetworkNode parentNode, HackFarm hackFarm)
        : base(hostName, displayName, IP, NodeType.PLAYER, parentNode, hackFarm) {
    }
}