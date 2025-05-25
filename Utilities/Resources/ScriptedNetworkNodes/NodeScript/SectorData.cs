using Godot;
using Godot.Collections;
[GlobalClass]
public partial class SectorData : Resource {
    [Export] public string SectorName { get; set; }
    [Export] public NodeData[] Nodes { get; set; }  

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> properties = [];
        for (int i = 0; i < Nodes.Length; i++) {
            properties.Add(new Dictionary {
                {"name", $"nodes/{i}"},
                {"type", (int)Variant.Type.Object},
                {"hint", (int)PropertyHint.ResourceType},
                {"hint_string", nameof(NodeData)}
            });
        }
        return properties;
    }
}
