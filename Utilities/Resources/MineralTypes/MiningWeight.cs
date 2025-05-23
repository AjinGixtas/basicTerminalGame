using Godot;
using Godot.Collections;
[GlobalClass]
public partial class MiningWeight : Resource {
	[Export] MineralProfile mineralProfile;
    [Export] public float weight;
}
