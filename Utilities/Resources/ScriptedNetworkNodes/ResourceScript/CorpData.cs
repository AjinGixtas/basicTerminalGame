using Godot;

public partial class CorpData : NodeData {
    [Export] public string s_symbol;
    [Export] public double s_price, s_drift, s_volatility;
    [Export] public string f_name;
    [Export] public string f_desc;
}
