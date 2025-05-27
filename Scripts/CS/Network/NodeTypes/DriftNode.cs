using Godot;

public class DriftNode : NetworkNode {
    public DriftSector Sector { get; init; }
    public DriftNode(string hostName, string displayName, string IP, NetworkNode parentNode, DriftSector sector, int secLvl) : 
        base(hostName, displayName, IP, NodeType.DRIFT, parentNode, false, 0){
        Sector = sector;
        NetworkManager.AssignDNS(this);
        DefLvl = (int)Mathf.Clamp(GD.Randfn(secLvl + .5, 2.75), 1, 10);
        SecLvl = (int)Mathf.Clamp(GD.Randfn(DefLvl/2+3.5, .33 * Mathf.E * Mathf.Pi), 1, DefLvl);
    }
}
