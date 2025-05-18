public class PlayerNode : NetworkNode {
    public NodeDirectory nodeDirectory { get; set; }
    public PlayerNode(string hostName, string displayName, string IP, NetworkNode parentNode)
        : base(hostName, displayName, IP, NetworkNodeType.PLAYER, parentNode) {
        nodeDirectory = new("~");
    }
}