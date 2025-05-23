using Godot;
using Godot.Collections;
[GlobalClass]
public partial class NodeData : Resource
{
	private int _numberCount;
	private Array<MiningWeight> _miningWeights;
	private Array<NodeData> _childNodes;
	[Export] public Array<MiningWeight> MiningWeights {
		get => _miningWeights;
		set {
			_miningWeights = value;
			NotifyPropertyListChanged();
		}
	}
	[Export] public Array<NodeData> ChildNodes {
		get => _childNodes;
		set {
			_childNodes = value;
			NotifyPropertyListChanged();
		}
	}
	public override Array<Dictionary> _GetPropertyList() {
		Array<Dictionary> properties = [];
		for (int i = 0; i < _miningWeights.Count; i++) {
			properties.Add(new Dictionary() {
				{ "name", $"miningweights/{i}" },
				{ "type", (int)Variant.Type.Object },
				{ "hint", (int)PropertyHint.ResourceType },
				{ "hint_string", nameof(MiningWeight) },
			});
		}
		for (int i = 0; i < _childNodes.Count; i++) {
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
			if (index >= 0 && index < _miningWeights.Count) {
				return _miningWeights[index];
			}
		}
		if (propertyName.StartsWith("childnodes")) {
			int index = int.Parse(propertyName.Split('/')[1]);
			if (index >= 0 && index < _childNodes.Count) {
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
			if (index >= 0 && index < _miningWeights.Count) {
				_miningWeights[index] = value.As<MiningWeight>();
				return true;
			}
		}
		if (propertyName.StartsWith("childnodes")) {
			int index = int.Parse(propertyName.Split('/')[1]);
			if (index >= 0 && index < _childNodes.Count) {
				_childNodes[index] = value.As<NodeData>();
				return true;
			}
		}
		return false;
	}
}
