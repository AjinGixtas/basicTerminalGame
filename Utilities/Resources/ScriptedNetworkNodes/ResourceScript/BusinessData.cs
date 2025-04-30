using Godot;

public partial class BusinessData : Resource {
    [Export] string hostName, displayName;
    [Export] int minDepth, maxDepth;
    [Export] string stockSymbol;
    [Export] double stockDrift, stockVolatility;
}
