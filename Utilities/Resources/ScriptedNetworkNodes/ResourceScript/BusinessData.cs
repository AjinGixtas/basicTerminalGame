using Godot;

public partial class BusinessData : NodeData {
    [Export] public string symbol;
    [Export] public double price, drift, volatility;
}
