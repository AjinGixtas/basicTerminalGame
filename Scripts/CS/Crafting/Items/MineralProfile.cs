using Godot;

[GlobalClass]
public partial class MineralProfile : Resource {
	public string Name { get; init; }
	public string Shorthand { get; init; }
	public int ID { get; init; }
	public Cc ColorCode { get; init; }
    public double Value { get; init; }
	public MineralProfile(string name, string shorthand, int id, double value, Cc colorCode) {
		Name = name; Shorthand = shorthand;
		ID = id; ColorCode = colorCode;
		Value = value;
	}
}
