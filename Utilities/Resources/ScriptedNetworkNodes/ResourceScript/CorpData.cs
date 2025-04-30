using Godot;

public partial class CorpData : Resource {
    [Export] string hostName, displayName;
    [Export] int minDepth, maxDepth;
    [Export] string stockSymbol;
    [Export] double stockDrift, stockVolatility;
    [Export] string factionName;
}
