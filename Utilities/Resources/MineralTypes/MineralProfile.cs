using Godot;

[GlobalClass]
public partial class MineralProfile : Resource {
	[Export] public string Name { get; set; }
	[Export] public string Shorthand { get; set; }
	[Export] public int ID { get; set; }
	public Cc ColorCode;
	[Export] public int _colorCode {
		get => (int)ColorCode; 
		set {
			ColorCode = (Cc)value;
		}
	}
	public MineralProfile() : this(string.Empty, string.Empty, 0, 0) { }
	public MineralProfile(string name, string shorthand, int index, int _colorCode) {
		Name = name; Shorthand = shorthand;
		ID = index; this._colorCode = _colorCode;
	}
}
