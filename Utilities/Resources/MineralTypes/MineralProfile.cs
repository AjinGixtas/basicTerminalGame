using Godot;

[GlobalClass]
public partial class MineralProfile : Resource {
	[Export] public string Name { get; set; }
	[Export] public int Index { get; set; }
	public Cc ColorCode;
	[Export] public int _colorCode {
		get => (int)ColorCode; 
		set {
            ColorCode = (Cc)value;
        }
	}
	public MineralProfile() : this(string.Empty, 0, 0) { }
	public MineralProfile(string name, int index, int _colorCode) {
		Name = name;
		Index = index; this._colorCode = _colorCode;
	}
}
