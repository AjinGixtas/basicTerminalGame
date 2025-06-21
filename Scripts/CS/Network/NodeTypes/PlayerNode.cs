using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Godot;
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
    void TutorialSpeak0() {
        if (PlayerDataManager.CompletedTutorial) return;
        cm.Enqueue("Hey, it's your first time here right?");
        cm.Enqueue("Well, there's a lot of things in around here, so let me give you a quick tour.");
        cm.Enqueue("First, you can see the menu bar at the top of the screen.");
        cm.Enqueue("It has all the important stuff you need to know about the game.");
        cm.Enqueue("And I ain't have time to teach you about it.");
        cm.Enqueue($"Go to sector `{Util.Format("SCALE_HOUSE_MetroClean_847137_nw", StrType.SECTOR)}` with `{Util.Format("link <sector_name>", StrType.CMD_FUL)}`");
        cm.Enqueue("You will be better off learning from there.");
        cm.Enqueue(2, "...");
        cm.Enqueue(10, $"Though if they did a bad job at it, look at the bottom of the menu and read the [color={Util.CC(Cc.gB)}]Documentation[/color]");
        _ = cm.RunDialogueAsync();
        ShellCore.OnCommandProcessed += TutorialSpeak1;
    }
    int linkFailCount = 0;
    async void TutorialSpeak1(string cmd, Dictionary<string,string> farg, string[] parg) {
        if (!(cmd == "link" && parg.Length > 0 && parg[0].Contains("SCALE_HOUSE_MetroClean_847137_nw"))) {
            ++linkFailCount;
            if (linkFailCount == 1) cm.Enqueue("Come on now, it's not that hard...");
            if (linkFailCount == 2) cm.Enqueue("You can do it, I believe in you!");
            if (linkFailCount == 3) cm.Enqueue("Yea no I lied, I don't believe in you, never did.");
            if (linkFailCount == 4) cm.Enqueue($"Just run `{Util.Format("link SCALE_HOUSE_MetroClean_847137_nw", StrType.CMD_FUL)}` and be on your way.");
            if (linkFailCount >= 4) cm.Enqueue($"There's chat log stored for you to check back. But for now, it's `{Util.Format("link SCALE_HOUSE_MetroClean_847137_nw", StrType.CMD_FUL)}`");
            if (linkFailCount != 6) _ = cm.RunDialogueAsync();
            if (linkFailCount == 6) {
                cm.Enqueue("Yes, I did state the command you needed to write twice on the fourth attempt.");
                _ = cm.RunDialogueAsync();
                await Task.Delay(15_000);
                ChatManager CM = new("Ajin");
                CM.Enqueue("(btw, the game IS prone to bug, so please report them when you can :)");
                CM.Enqueue("(like this one!)");
                CM.Enqueue("(you should report this one. I won't fix this one. Because this one is not a bug. But other bug?)");
                CM.Enqueue("(...maybe? coding is hard, im lazy, tsc tsc tsc...)");
                _ = CM.RunDialogueAsync();
            }
            return;
        }
        cm.Enqueue("Ok great, so about sector...");
        cm.Enqueue("They are cluster of nodes you can connect to.");
        cm.Enqueue("Each connected sector give you direct connection to nodes in that sector");
        cm.Enqueue($"To see neighbouring nodes, run `{Util.Format("scan", StrType.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.OnCommandProcessed -= TutorialSpeak1; ShellCore.OnCommandProcessed += TutorialSpeak2;
    }
    int scanFailCount = 0;
    void TutorialSpeak2(string cmd, Dictionary<string, string> farg, string[] parg) {
        if (cmd != "scan") {
            ++scanFailCount;
            if (scanFailCount == 1) cm.Enqueue("It's 4 letters, how hard can it be to type?");
            if (scanFailCount == 2) cm.Enqueue($"About as hard as copy `{Util.Format("scan", StrType.CMD_FUL)}` to your command prompt");
            if (scanFailCount == 3) cm.Enqueue($"WHICH IS [color={Util.CC(Cc.R)}]HIGHLIGHTED[/color]. BY THE WAY.");
            if (scanFailCount > 3) cm.Enqueue($"It's `{Util.Format("scan", StrType.CMD_FUL)}`. Chatlog is stored in {Util.Format("./irc-log", StrType.DIR)}.");
            return;
        }
        cm.Enqueue($"You see that one singular node? Run `{Util.Format("connect <ip_or_hostname>", StrType.CMD_FUL)}` on it!");
        cm.Enqueue(10, $"The guy's name is {Util.Format("cl3ver-b0y_fnZ3yX", StrType.HOSTNAME)}. If you don't see it, check if you `{Util.Format("link", StrType.CMD_FUL)}`-ed to the sector.");
        if (!NetworkManager.GetSectorNames(true).Contains("SCALE_HOUSE_MetroClean_847137_nw")) {
            cm.Enqueue("...");
            cm.Enqueue(4, "You just unlinked from the sector, didn't you?");
        }
        cm.Enqueue($"Log is stored in {Util.Format("./irc-log", StrType.DIR)} if you forgot what to do next.");
        _ = cm.RunDialogueAsync();
        ShellCore.OnCommandProcessed -= TutorialSpeak2; ShellCore.OnCommandProcessed += TutorialSpeak3;
    }
    void TutorialSpeak3(string cmd, Dictionary<string, string> farg, string[] parg) {
        GD.Print(ShellCore.CurrNode.HostName);
        if (ShellCore.CurrNode.HostName != "cl3ver-b0y_fnZ3yX") return;
        ShellCore.OnCommandProcessed -= TutorialSpeak3;
        cm.Enqueue("Alright, nice job.");
        cm.Enqueue("Someone is probably talking to you right now.");
        cm.Enqueue($"Do as they say, and come back ([color={Util.CC(Cc.R)}]ONLY WHEN[/color]) you've learnt everything.");
    }
}