using Godot;
using System.Collections.Generic;

public class DriftNode : NetworkNode {
	public DriftSector Sector { get; init; }
	public DriftNode(string hostName, string displayName, string IP, NetworkNode parentNode, DriftSector sector, SecLvl secLvl) :
		base(hostName, displayName, IP, NodeType.DRIFT, parentNode, false, []) {
		Sector = sector;
		int deflvl = GD.Randf() < .02 ? 0 : GD.RandRange(1, 5);
		Init(deflvl, secLvl);
		int hackFarmTier = SecLvl switch {
			SecLvl.NOSEC or SecLvl.LOSEC => GD.RandRange(0, 2),
			SecLvl.MISEC => GD.RandRange(3, 5),
			SecLvl.HISEC => GD.RandRange(6, 8),
			SecLvl.MASEC => 9,
			_ => throw new System.Exception("Invalid security level for hack farm tier generation.")
		};
		_hackFarm = new BotFarm(hackFarmTier, this);
		GCdeposit = GD.RandRange(
			Mathf.CeilToInt(Mathf.Pow(2, DefLvl) * Mathf.Pow(10, DefLvl) * 1.25),
			Mathf.CeilToInt(Mathf.Pow(2, DefLvl) * Mathf.Pow(10, DefLvl) * 0.75)
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