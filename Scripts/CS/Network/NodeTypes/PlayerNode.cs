using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        await Task.Delay(5_000);
        cm.Enqueue("Hey, it's your first time here right?");
        cm.Enqueue("Well, there's a lot of things in around here, so let me give you a quick tour.");
        cm.Enqueue("But I ain't have time to teach you about it.");
        cm.Enqueue($"Go to sector `{Util.Format("SCALE_HOUSE_MetroClean_847137_nw", StrSty.SECTOR)}` with `{Util.Format("link <sector_name>", StrSty.CMD_FUL)}`");
        cm.Enqueue("You will be better off learning from there.");
        _ = cm.RunDialogueAsync();
        ShellCore.OnCommandProcessed += TutorialSpeak1;
    }
    async void TutorialSpeak1(string cmd, Dictionary<string,string> farg, string[] parg) {
        if (!(cmd == "link" && parg.Length > 0 && parg[0].Contains("SCALE_HOUSE_MetroClean_847137_nw"))) {
            //cm.Enqueue(4, $"The command is `{Util.Format("link SCALE_HOUSE_MetroClean_847137_nw", StrType.CMD_FUL)}`. Paste it to the prompt.");
            //_ = cm.RunDialogueAsync();
            // Removed for player who likes to explore by themselves
            return;
        }
        await Task.Delay(500);
        cm.Enqueue("Ok great, so about sector...");
        cm.Enqueue("They are cluster of nodes you can connect to.");
        cm.Enqueue("Each connected sector give you direct connection to nodes in that sector");
        cm.Enqueue($"To see neighbouring nodes, run `{Util.Format("scan", StrSty.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.OnCommandProcessed -= TutorialSpeak1; ShellCore.OnCommandProcessed += TutorialSpeak2;
    }
    async void TutorialSpeak2(string cmd, Dictionary<string, string> farg, string[] parg) {
        if (cmd != "scan") {
            cm.Enqueue(4, $"It's `{Util.Format("scan", StrSty.CMD_FUL)}`.");
            _ = cm.RunDialogueAsync();
            return;
        }
        if (!NetworkManager.GetSectorNames(true).Contains("SCALE_HOUSE_MetroClean_847137_nw")) {
            cm.Enqueue(7, "...");
            cm.Enqueue(4, $"Weren't you just {Util.Format("link", StrSty.CMD_FUL)}ed to the sector from earlier?");
            cm.Enqueue(4, $"Did you {Util.Format("unlink", StrSty.CMD_FUL)}ed it?");
            cm.Enqueue(7, "...");
            cm.Enqueue($"Run `{Util.Format("link SCALE_HOUSE_MetroClean_847137_nw; scan;", StrSty.CMD_FUL)}`");
        } else {
            await Task.Delay(500);
            cm.Enqueue($"You see that one singular node? Run `{Util.Format("connect <ip_or_hostname>", StrSty.CMD_FUL)}` on it!");
            cm.Enqueue(10, $"The guy's name is {Util.Format("cl3ver-b0y_fnZ3yX", StrSty.HOSTNAME)}. You should be able to see it.");
            ShellCore.OnCommandProcessed -= TutorialSpeak2; ShellCore.OnCommandProcessed += TutorialSpeak3;
        }
        _ = cm.RunDialogueAsync();
    }
    async void TutorialSpeak3(string cmd, Dictionary<string, string> farg, string[] parg) {
        if (ShellCore.CurrNode.HostName != "cl3ver-b0y_fnZ3yX") { 
            await Task.Delay(200);
            cm.Enqueue($"You're still on the wrong host. It's {Util.Format("cl3ver-b0y_fnZ3yX", StrSty.HOSTNAME)}. `{Util.Format("connect", StrSty.CMD_FUL)}` to it.");
            _ = cm.RunDialogueAsync();
            return;
        }
        await Task.Delay(500);
        ShellCore.OnCommandProcessed -= TutorialSpeak3;
        cm.Enqueue("Alright, nice job.");
        cm.Enqueue("You will meet someone soon.");
        cm.Enqueue("Be nice and do as they say.");
        _ = cm.RunDialogueAsync();
    }
}