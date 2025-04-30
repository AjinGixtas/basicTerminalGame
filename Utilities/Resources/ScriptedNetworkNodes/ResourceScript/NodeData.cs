using Godot;

public partial class NodeData : Resource
{
    [Export] string hostName, displayName;
    [Export] int minDepth, maxDepth;
}
