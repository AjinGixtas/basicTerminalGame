using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
public class PlayerNode : ScriptedNetworkNode {
    public PlayerNode(NodeData playerNodeData)
        : base(playerNodeData, null) {
        LifeCycleDirector.FinishScene += TutorialSpeak0;
    }
    public ChatManager cm = new("root");
    public NodeData SerializeNodeData() {
        return new NodeData {
            NodeType = NodeType,
            HostName = HostName,
            DisplayName = DisplayName,
            ParentNode = NodeData.ParentNode,
            ChildNodes = NodeData.ChildNodes,
            DefLvl = DefLvl,
            SecLvl = SecLvl,
            RetLvl = RetLvl,
            GcDeposit = GCdeposit,
            MineralsDeposit = MineralDeposit,
            Locks = NodeData.Locks.Select(e => (int)e).ToArray()
        };
    }
    async void TutorialSpeak0() {
        if (PlayerDataManager.CompletedTutorial) return;
        (double, string)[] dialougesP1 = [
            (1, "Hey, it's your first time here right?"),
            (2, "Well, there's a lot of things in around here, so let me give you a quick tour."),
            (6, "First, you can see the menu bar at the top of the screen."),
            (5, "It has all the important stuff you need to know about the game."),
            (8, "And I ain't have time to teach you about it."),
            (4, $"Go to sector `SCALE_HOUSE_MetroClean_847137_nw` with `{Util.Format("link <sector_name>", StrType.CMD_FUL)}`"),
            (4, $"You will be better off learning from there."),
            (4, $"Though if they did a bad job at it, look at the bottom of the menu and read the [color={Util.CC(Cc.gB)}]Documentation[/color]"),
        ];
        await DialogueManager.SaySequenceAsync(dialougesP1, "???");
        Action<string, Dictionary<string, string>, string[]> onCommandProcessed = null;

        onCommandProcessed = (command, farg, parg) => {
            if (!(command == "link" && parg.Length > 0 && parg[0] == "SCALE_HOUSE_MetroClean_847137_nw")) return;
            DialogueManager.Interrupt(); _ = TutorialSpeak1();
            ShellCore.OnCommandProcessed -= onCommandProcessed; // Unsubscribe to avoid multiple calls
        };
        ShellCore.OnCommandProcessed += onCommandProcessed; // Subscribe to the command processed event
    }
    async Task TutorialSpeak1() {
        (double, string)[] dialougesP2 = [
            (2, "Ok great, so about sector..."),
            (3, "They are cluster of nodes you can connect to."),
            (3, "Each connected sector give you direct connection to nodes in that sector"),
            (3, $"To see neighbouring nodes, run `{Util.Format("scan", StrType.CMD_FUL)}`"),
            (3, $"Connect to any of them with `{Util.Format("connect <ip_or_hostname>", StrType.CMD_FUL)}`"),
            (3, "You can also connect to nodes in other sectors, but you need to link to them first."),
            (4, "I can tell you how to do it, but you need to learn other stuff first."),
            (3, "Connect to the next node, you'll figure the rest out.")
        ];
        await DialogueManager.SaySequenceAsync(dialougesP2, "???");
    }
}