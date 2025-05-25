using Godot;

public class TutorialNetworkNode : ScriptedNetworkNode {
    public TutorialNetworkNode(NodeData nodeData, NetworkNode parent) : base(nodeData, parent) {
        if (nodeData == null) {
            GD.PushError("NodeData is null in TutorialNetworkNode constructor.");
            return;
        }
        Init(nodeData.DefLvl, nodeData.SecLvl);
    }
    static readonly string[] tutorialProgressMsg = [
$@"Great! You gain access to your home node.
Run {Util.Format("scan", StrType.CMD_FUL)} and {Util.Format("connect", StrType.CMD_FUL)} to neighbouring node.
You can see the node information using {Util.Format("analyze", StrType.CMD_FUL)} before {Util.Format("karaxe --attack", StrType.CMD_FUL)} them.
This one is free though. Next one won't though...",

$@"The next node contain a single lock defending them. It's called {Util.Format("I4X", StrType.CMD_FLAG)}.
This lock contains 2 component. {Util.Format("--i4", StrType.CMD_FLAG)} and {Util.Format("--2xtract", StrType.CMD_FLAG)}
{Util.Format("--i4", StrType.CMD_FLAG)} can be any of the first four natural number.
{Util.Format("--2xtract", StrType.CMD_FLAG)} is either {Util.Format("fl1p", StrType.CMD_FLAG)}, {Util.Format("2bin", StrType.CMD_FLAG)}, {Util.Format("der3f", StrType.CMD_FLAG)} or {Util.Format("bl4nk", StrType.CMD_FLAG)}
So how do you unlock them? Just bruteforce it hehe :p",

$@"Oh yeah! Did I tell you that flare sequence last for 120 second?
...
Oh, you already can tell from the timer. Oh well. Bet you didn't know you get to attack any amount of node you like.
...
... well, if it's too long for you, just run {Util.Format("karaxe --axe", StrType.CMD_FUL)} to exit it early.
Anyway... next node still only have a single lock even thought it has Firewall rating at 2, but this time it's {Util.Format("C0", StrType.CMD_FLAG)}.
The lock answer is the opposite color of whatever it gave you.
So why does is there only one lock? Because there is a little suprise waiting for you there. Hehe.",

$@"Did it took your money? 
1 - If yes, pretend I made fun of you. 
2 - If no, pretened I praised you. 
3 - If you saw the tip from earlier, pretend I praised you extra.

[*]The praise from the third case cancel out the first case. The net praise recieved is an exercise for the reader.

Use your imagination, you're playing a text game after all.
Next node have both locks, but it won't take your money, so you won't feel bad XD
"
    ];
    public override int TransferOwnership() {
        if (IsSecure) return 1;
        OwnedByPlayer = true;
        PlayerDataManager.tutorialProgress++;
        TerminalProcessor.Say(tutorialProgressMsg[PlayerDataManager.tutorialProgress]);
        return 0;
    }
}
