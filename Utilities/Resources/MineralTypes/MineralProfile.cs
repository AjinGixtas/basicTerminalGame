using Godot;

[GlobalClass]
public partial class MineralProfile : Resource {
    [Export] public string Name { get; set; }
    public MineralProfile() : this(string.Empty) { }
    public MineralProfile(string name) {
        Name = name;
    }
}