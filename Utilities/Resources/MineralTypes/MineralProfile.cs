using Godot;

[GlobalClass]
public partial class MineralProfile : Resource {
	[Export] public string Name { get; set; }
	[Export] public int Index { get; set; }
	public MineralProfile() : this(string.Empty, 0) { }
	public MineralProfile(string name, int index) {
		Name = name;
	}
}
