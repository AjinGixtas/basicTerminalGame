using Godot;
using System.Collections.Generic;

public abstract class NetworkNode {
	public string HostName { get; protected set; }
	public string DisplayName { get; protected set; }
	public string IP { get; protected set; }
	// ^ cosmetic info
	int _secLvl, _defLvl, _retLvl; // Defense = Sec+Ret
	public SecurityType SecType { get; protected set; }
	public NodeType NodeType { get; protected set; }

	NetworkNode _parentNode; 
	public NetworkNode ParentNode { 
		get => _parentNode; 
		set { 
			_parentNode?.ChildNode.Remove(this);
			value?.ChildNode.Add(this);
			_parentNode = value;
		}
	}
	public List<NetworkNode> ChildNode { get; init; }
	public LockSystem LockSystem { get; private set; }
	public int SecLvl {
		get => _secLvl;
		protected set {
			value = Mathf.Clamp(value, 0, _defLvl);
			if (_secLvl == value) return;
			_secLvl = value;
			LockSystem?.LockIntialization(_secLvl);
			_retLvl = _defLvl - _secLvl;
			SecType = Util.MapEnum<SecurityType>(_secLvl);
		}
	}
	public int DefLvl {
		get => _defLvl;
		protected set {
			value = Mathf.Clamp(value, 0, 10);
			SecLvl += value - _defLvl;
			_defLvl = value;
		}
	}
	public int RetLvl {
		get => _retLvl;
	}
	
	public NetworkNode(string hostName, string displayName, string IP, NodeType NodeType, NetworkNode parentNode, bool ownedByPlayer, int lockCode = 0) {
		HostName = hostName; DisplayName = displayName; this.IP = IP; this.NodeType = NodeType;
		OwnedByPlayer = ownedByPlayer; ParentNode = parentNode; ChildNode = [];
		LockSystem = new(lockCode);
	}
	
	public virtual void Init(int DefLvl, int SecLvl) {
		this.DefLvl = DefLvl; this.SecLvl = SecLvl;
	}
	
	public int GetDepth() {
		int output = 0;
		NetworkNode curNode = this;
		while (curNode.ParentNode != null) {
			curNode = curNode.ParentNode;
			++output;
		}
		return output;
	}
	public virtual NodeAnalysis Analyze() {
		return new NodeAnalysis {
			IP = IP,
			HostName = HostName,
			DisplayName = DisplayName,
			DefLvl = DefLvl,
			SecLvl = SecLvl,
			RetLvl = _retLvl,
			SecType = SecType,
			NodeType = NodeType
		};
	}
	
	bool _ownedByPlayer = false; public bool OwnedByPlayer {
		get => _ownedByPlayer;
		protected set => _ownedByPlayer = value;
	}
	double _gcDeposit = 0; public double GCdeposit { get => _gcDeposit;
		set {
			if (_gcDeposit != 0) return;
			_gcDeposit = value;
		}
	}
	double[] _mineralDeposit = new double[10]; public double[] MineralDeposit {
		get => _mineralDeposit;
		set {
			if (value.Length != MineralDeposit.Length) { GD.PrintErr("Different length was submitted"); return; }
			_mineralDeposit = value;
		}
	}
	bool _isSecure = true; protected bool IsSecure { get => _isSecure; 
		set {
			if (_isSecure == true && value == false) {
				PlayerDataManager.DepositGC(GCdeposit);
				PlayerDataManager.DepositMineral(MineralDeposit);
            }
            _isSecure = value;
		}
	}
	public virtual CError AttempCrackNode(Dictionary<string, string> ans, double endEpoch) {
		if (LockSystem == null) { return CError.INVALID; } // No lock system, cannot crack
		CError result = LockSystem.CrackAttempt(ans, endEpoch);
		if (result == CError.OK) {
			IsSecure = false; LockSystem = null;
			SecLvl = 0; DefLvl = 0;
		}
		return result;
	}
	public virtual int TransferOwnership() {
		if (IsSecure) return 1; // Node secured, transfer impossible
		OwnedByPlayer = true; return 0; // Transfer successful
	}
}
