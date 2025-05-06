using Godot;

public partial class NodeData : Resource
{
    [Export] public string hostName, displayName;
    [Export] public int minDepth, maxDepth;
    [Export(PropertyHint.Enum, "PLAYER,PERSON,BUSINESS,CORP,FACTION,HONEYPOT,MINER,ROUGE")]
    public int enum_NodeType;
}
