using Godot;

public class NetworkModule {
    public CError LinkSector(string sectorName) {
        CError errorCode = NetworkManager.ConnectToSector(sectorName);
        return errorCode;
    }
    public CError UnlinkSector(string sectorName) {
        CError errorCode = NetworkManager.DisconnectFromSector(sectorName);
        return errorCode;
    }
    public string[] Scan(bool verbose = false, string IP = "", int MAX_DEPTH = 1) {
        GD.Print(TerminalProcessor.Scan(verbose, IP, MAX_DEPTH).Join(""));
        return TerminalProcessor.Scan(verbose, IP, MAX_DEPTH);
    }
    public string[] Sector(bool connectedOnly = false) {
        if (connectedOnly) return NetworkManager.GetConnectedSectorNames();
        return NetworkManager.GetSectorNames();
    }
}