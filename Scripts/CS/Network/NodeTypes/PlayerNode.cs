public class PlayerNode : NetworkNode {
    public PlayerNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.PLAYER, parentNode) {
    }
}