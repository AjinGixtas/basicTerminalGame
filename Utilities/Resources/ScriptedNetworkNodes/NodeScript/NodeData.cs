using Godot;
using Godot.Collections;
[GlobalClass]
public partial class NodeData : Resource
{
	private MiningWeight[] _miningWeights;
	private NodeData[] _childNodes;
	private LockType _locks;
	private int _defLvl, _secLvl, _retLvl;
	private float _gcDeposit;
	[Export] public NodeType NodeType;
	[Export] public string HostName;
    [Export] public string DisplayName;
    [Export] public int DefLvl {
		get => _defLvl;
		set {
			_defLvl = value;
		}
	}
	[Export] public int SecLvl {
		get => _secLvl;
		set {
			_secLvl = value;
		}
	}
	[Export] public int RetLvl {
		get => _retLvl;
        set {
            _retLvl = value;
        }
    }
	[Export] public float GcDeposit {
        get => _gcDeposit;
        set {
            _gcDeposit = value;
        }
    }
    [Export] public LockType Locks {
		get => _locks;
		set {
			_locks = value;
			NotifyPropertyListChanged();
		}
    }
    [Export] public MiningWeight[] MiningWeights {
		get => _miningWeights;
		set {
			_miningWeights = value;
			NotifyPropertyListChanged();
		}
	}
	[Export] public double MAX_GROW_MINING;
	[Export] public NodeData[] ChildNodes {
		get => _childNodes;
		set {
			_childNodes = value;
			NotifyPropertyListChanged();
		}
	}
	[Export] public bool OwnedByPlayer { get; set; }

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
}
