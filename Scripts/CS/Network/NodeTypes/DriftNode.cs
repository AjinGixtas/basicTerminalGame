using Godot;
using System.Collections.Generic;

public class DriftNode : NetworkNode {
	public DriftSector Sector { get; init; }
	public DriftNode(string hostName, string displayName, string IP, NetworkNode parentNode, DriftSector sector, SecLvl secLvl) : 
		base(hostName, displayName, IP, NodeType.DRIFT, parentNode, false, []){
		Sector = sector;
		Init(GD.Randf() < .02 ? 0 : GD.RandRange(1, 5), secLvl);
		_hackFarm = new BotFarm(DefLvl, this);
		GCdeposit = GD.RandRange(
			Mathf.CeilToInt(Mathf.Pow(2, DefLvl) * Mathf.Pow(10, (DefLvl-3.0)/4.0)),
            Mathf.CeilToInt(Mathf.Pow(2, DefLvl) * Mathf.Pow(10, (DefLvl+3.0)/4.0))
		);

    }
	BotFarm _hackFarm = null;
	public BotFarm HackFarm {
		get {
			if (!OwnedByPlayer) {
				GD.PrintErr("HackFarm is only available for player-owned nodes.");
				return null;
			}
			return _hackFarm;
		}
		init {
			_hackFarm = value;
		}
	}
	~DriftNode() {
		if (Util.HaveFinalWord) GD.Print($"DriftNode {HostName} is being destroyed");
	}
	public override (CError, string, string, string)[] AttemptCrackNode(Dictionary<string, string> ans, double endEpoch) {
		if (Sector.LockedDown) { 
			ShellCore.Say(Util.Format("Sector is locked down, no attack possible.", StrSty.ERROR));
			return [(CError.NO_PERMISSION, "", "", "")];
		}
		return base.AttemptCrackNode(ans, endEpoch);
	}
}
