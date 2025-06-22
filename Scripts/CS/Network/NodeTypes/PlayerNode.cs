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
        cm.Enqueue("But I ain't have time to teach you about it.");
        cm.Enqueue($"Go to sector `{Util.Format("SCALE_HOUSE_MetroClean_847137_nw", StrType.SECTOR)}` with `{Util.Format("link <sector_name>", StrType.CMD_FUL)}`");
        cm.Enqueue("You will be better off learning from there.");
        _ = cm.RunDialogueAsync();
        ShellCore.OnCommandProcessed += TutorialSpeak1;
    }
    void TutorialSpeak1(string cmd, Dictionary<string,string> farg, string[] parg) {
        if (!(cmd == "link" && parg.Length > 0 && parg[0].Contains("SCALE_HOUSE_MetroClean_847137_nw"))) {
            //cm.Enqueue(4, $"The command is `{Util.Format("link SCALE_HOUSE_MetroClean_847137_nw", StrType.CMD_FUL)}`. Paste it to the prompt.");
            //_ = cm.RunDialogueAsync();
            // Removed for player who likes to explore by themselves
            return;
        }
        cm.Enqueue("Ok great, so about sector...");
        cm.Enqueue("They are cluster of nodes you can connect to.");
        cm.Enqueue("Each connected sector give you direct connection to nodes in that sector");
        cm.Enqueue($"To see neighbouring nodes, run `{Util.Format("scan", StrType.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.OnCommandProcessed -= TutorialSpeak1; ShellCore.OnCommandProcessed += TutorialSpeak2;
    }
    void TutorialSpeak2(string cmd, Dictionary<string, string> farg, string[] parg) {
        if (cmd != "scan") {
            cm.Enqueue(4, $"It's `{Util.Format("scan", StrType.CMD_FUL)}`.");
            _ = cm.RunDialogueAsync();
            return;
        }
        if (!NetworkManager.GetSectorNames(true).Contains("SCALE_HOUSE_MetroClean_847137_nw")) {
            cm.Enqueue(7, "...");
            cm.Enqueue(4, $"Weren't you just {Util.Format("link", StrType.CMD_FUL)}ed to the sector from earlier?");
            cm.Enqueue(4, $"Did you {Util.Format("unlink", StrType.CMD_FUL)}ed it?");
            cm.Enqueue(7, "...");
            cm.Enqueue($"Run `{Util.Format("link SCALE_HOUSE_MetroClean_847137_nw; scan;", StrType.CMD_FUL)}`");
        } else {
            cm.Enqueue($"You see that one singular node? Run `{Util.Format("connect <ip_or_hostname>", StrType.CMD_FUL)}` on it!");
            cm.Enqueue(10, $"The guy's name is {Util.Format("cl3ver-b0y_fnZ3yX", StrType.HOSTNAME)}. You should be able to see it.");
            ShellCore.OnCommandProcessed -= TutorialSpeak2; ShellCore.OnCommandProcessed += TutorialSpeak3;
        }
        _ = cm.RunDialogueAsync();
    }
    void TutorialSpeak3(string cmd, Dictionary<string, string> farg, string[] parg) {
        if (ShellCore.CurrNode.HostName != "cl3ver-b0y_fnZ3yX") { 
            cm.Enqueue(4, $"You're still on the wrong host. It's {Util.Format("cl3ver-b0y_fnZ3yX", StrType.HOSTNAME)}. `{Util.Format("connect", StrType.CMD_FUL)}` to it.");
            _ = cm.RunDialogueAsync();
            return;
        }
        ShellCore.OnCommandProcessed -= TutorialSpeak3;
        cm.Enqueue("Alright, nice job.");
        cm.Enqueue("You will meet someone soon.");
        cm.Enqueue("Be nice and do as they say.");
        _ = cm.RunDialogueAsync();
    }
}