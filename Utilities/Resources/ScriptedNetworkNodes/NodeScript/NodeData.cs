using Godot;
using Godot.Collections;
using System;
using System.Linq;
// Despite the general-feel of this file, it is only used for Player's NodeData (as of now).
[GlobalClass]
public partial class NodeData : Resource
{
	private MiningWeight[] _miningWeights;
	private NodeData[] _childNodes;
	private LocT[] _locks;
	private int _defLvl;
	private SecLvl _secLvl;
	private long _gcDeposit;
	[ExportGroup("Node Info")]
    [Export] public NodeType NodeType;
	[Export] public string HostName;
    [Export] public string DisplayName;
	[Export] public NodeData ParentNode;
	[Export] public NodeData[] ChildNodes {
		get => _childNodes;
		set {
			_childNodes = value;
			NotifyPropertyListChanged();
		}
	}
	[ExportGroup("Node Stats")]
    [Export] public int DefLvl {
		get => _defLvl;
		set {
			_defLvl = value;
		}
	}
	[Export] public int SecLvl {
		get => (int)_secLvl;
		set {
			_secLvl = (SecLvl)value;
		}
	}
	[ExportGroup("Node security sys")]
    // GC reward for breaking the node
    [Export] public long GcDeposit {
        get => _gcDeposit;
        set {
            _gcDeposit = value;
        }
    }
	[Export] public long[] MineralsDeposit;
    // Used for security systems against breaking the node
    [Export] public int[] Locks {
		get => _locks.Select(e => (int)e).ToArray();
		set {
			_locks = new LocT[value.Length];
            for (int i = 0; i < value.Length; i++) {
                _locks[i] = (LocT)value[i];
            }
		}
    }
	[ExportGroup("Mining stats")]
    [Export] public MiningWeight[] MiningWeights {
		get => _miningWeights;
		set {
			_miningWeights = value;
			NotifyPropertyListChanged();
		}
	}
	[Export] public double MAX_GROW_MINING;
	[Export] public bool OwnedByPlayer { get; set; }
	[Export] public int hackLvl, growLvl, timeLvl;

    public override Array<Dictionary> _GetPropertyList() {
		Array<Dictionary> properties = [];
        for (int i = 0; i < _miningWeights.Length; i++) {
			properties.Add(new Dictionary() {
				{ "name", $"miningweights/{i}" },
				{ "type", (int)Variant.Type.Object },
				{ "hint", (int)PropertyHint.ResourceType },
				{ "hint_string", nameof(MiningWeight) },
			});
		}
		for (int i = 0; i < _childNodes.Length; i++) {
			properties.Add(new Dictionary() {
				{ "name", $"childnodes/{i}" },
				{ "type", (int)Variant.Type.Object },
				{ "hint", (int)PropertyHint.ResourceType },
				{ "hint_string", nameof(NodeData) },
			});
		}
		return properties;
	}
	public override Variant _Get(StringName property)
	{
		string propertyName = property.ToString();
		if (propertyName.StartsWith("miningweights")) {
			int index = int.Parse(propertyName.Split('/')[1]);
			if (index >= 0 && index < _miningWeights.Length) {
				return _miningWeights[index];
			}
		}
		if (propertyName.StartsWith("childnodes")) {
			int index = int.Parse(propertyName.Split('/')[1]);
			if (index >= 0 && index < _childNodes.Length) {
				return _childNodes[index];
			}
		}
		return default;
	}
	public override bool _Set(StringName property, Variant value)
	{
		string propertyName = property.ToString();
		if (propertyName.StartsWith("miningweights")) {
			int index = int.Parse(propertyName.Split('/')[1]);
			if (index >= 0 && index < _miningWeights.Length) {
				_miningWeights[index] = value.As<MiningWeight>();
				return true;
			}
		}
		if (propertyName.StartsWith("childnodes")) {
			int index = int.Parse(propertyName.Split('/')[1]);
			if (index >= 0 && index < _childNodes.Length) {
				_childNodes[index] = value.As<NodeData>();
				return true;
			}
		}
		return false;
	}
	public NodeData() : this([], [], new long[10], []) { }
    public NodeData(NodeData[] childNodes, MiningWeight[] miningWeights, long[] mineralDeposit,
    int[] locks, NodeType type = NodeType.VM,
    string hostName = "HOST_NOT_FOUND", string displayName = "HOST_NOT_FOUND",
    long gcDeposit = 0, int hacklvl = 1, int growlvl = 1, int timelvl = 1, int defLvl = 0, SecLvl secLvl = global::SecLvl.NOSEC) {
        ChildNodes = childNodes; MiningWeights = miningWeights; MineralsDeposit = mineralDeposit;
        Locks = locks; NodeType = type;
        HostName = hostName; DisplayName = displayName;
        GcDeposit = gcDeposit; hackLvl = hacklvl; timeLvl = timelvl; growLvl = growlvl;
        _defLvl = defLvl; _secLvl = secLvl;
    }
}
