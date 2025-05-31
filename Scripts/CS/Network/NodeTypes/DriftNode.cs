using Godot;

public class DriftNode : NetworkNode {
    public DriftSector Sector { get; init; }
    public DriftNode(string hostName, string displayName, string IP, NetworkNode parentNode, DriftSector sector, int secLvl) : 
        base(hostName, displayName, IP, NodeType.DRIFT, parentNode, false, 0){
        Sector = sector;
        NetworkManager.AssignDNS(this);
        DefLvl = (int)Mathf.Clamp(GD.Randfn(secLvl + .5, Mathf.E), 1, 10);
        SecLvl = (int)Mathf.Clamp(GD.Randfn(DefLvl/2+3.5, .2 * Mathf.E * Mathf.Pi), 1, DefLvl);
    }
}
