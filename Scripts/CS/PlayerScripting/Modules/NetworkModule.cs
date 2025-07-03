using Godot;

public class NetworkModule {
    public string GetMyIP() {
        return NetworkManager.PlayerNode.IP;
    }
    public CError LinkSector(string sectorName) {
        return NetworkManager.ConnectToSector(sectorName);
    }
    public CError UnlinkSector(string sectorName) {
        return NetworkManager.DisconnectFromSector(sectorName);
    }
    public CError ConnectNode(string IP) {
        return ShellCore.ConnectNode(IP);
    }
    public string[] Scan(string IP = "") {
        return ShellCore.Scan(IP);
    }
    public string[] Sector(bool connectedOnly = false, int level=-1) {
        return NetworkManager.GetSectorNames(connectedOnly, level);
    }
    public int? GetNodeFirewallRating(string IP) {
        return NetworkManager.GetNodeByIP(IP)?.DefLvl;
    }
    public SecLvl? GetNodeSecurity(string IP) {
        return NetworkManager.GetNodeByIP(IP)?.SecLvl;
    }
}