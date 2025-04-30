using Godot;

public partial class FactionData : Resource {
    [Export] string hostName, displayName;
    [Export] int minDepth, maxDepth;
    [Export] string factionName;
}
