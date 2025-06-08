using Godot;

public class NetworkModule {
    public int LinkSector(string sectorName) {
        return (int)NetworkManager.ConnectToSector(sectorName);
    }
    public int UnlinkSector(string sectorName) {
        return (int)NetworkManager.DisconnectFromSector(sectorName);
    }
    public int ConnectNode(string IP) {
        return (int)TerminalProcessor.ConnectNode(IP);
    }
    public string[] Scan(string IP = "", int MAX_DEPTH = 1) {
        return TerminalProcessor.Scan(IP, MAX_DEPTH);
    }
    public string[] Sector(bool connectedOnly = false, int level=-1) {
        return NetworkManager.GetSectorNames(connectedOnly, level);
    }
}