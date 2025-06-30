using Godot;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class ShellCore {
    public static void BiTrader(Dictionary<string, string> farg, string[] parg) {
        bool didSomething = false;

        string itemCode = Util.GetArg(farg, "--item", "-i"); bool sellOne = Util.ContainKeys(farg, "--item", "-i");
        string t_amount = Util.GetArg(farg, "--amount", "-a");
        bool sellAll = Util.ContainKeys(farg, "--sea1");
        bool buy = Util.ContainKeys(farg, "--buy", "-b");
        bool options = Util.ContainKeys(farg, "--option", "-o");
        if (options) {
            didSomething = true;
            ShellCore.Say($"Valid value for {Util.Format("--item", StrSty.CMD_FLAG)}:");
            StringBuilder sb = new(""); for (int i = 0; i < ItemCrafter.ALL_ITEMS.Length; ++i) sb.Append(Util.Format(ItemCrafter.ALL_ITEMS[i].Shorthand, StrSty.COLORED_ITEM_NAME, $"{i}").PadRight(30));
            ShellCore.Say(sb.ToString());
        }

        double percent; int itemID;
        if (!sellAll) {
            didSomething = true;
            if (itemCode == null) { ShellCore.Say($"Missing {Util.Format("--item", StrSty.CMD_FLAG)}"); return; }
            itemID = ItemCrafter.ALL_ITEMS.FirstOrDefault(x => x.Shorthand == itemCode)?.ID ?? (int.TryParse(itemCode, out int parsedId) ? parsedId : -1);
            if (itemID == -1) { ShellCore.Say("-r", "No item with such ID or symbol found."); return; }

            if (t_amount == null) { ShellCore.Say($"Missing {Util.Format("--amount", StrSty.CMD_FLAG)}"); return; }
            if (!long.TryParse(t_amount, out long amount)) { ShellCore.Say("-r", $"Invalid value for {Util.Format("--amount", StrSty.CMD_FLAG)}, must be a number"); return; }

            percent = (ItemSeller.PriceModels[itemID].value / ItemCrafter.ALL_ITEMS[itemID].Value - 1.0) * 100;
            if (buy) {
                long cost = (long)(ItemSeller.PriceModels[itemID].value * amount);
                if (PlayerDataManager.WithdrawGC(cost) != CError.OK) { ShellCore.Say("-r", $"Not enough {Util.Format("GC", StrSty.AUTO_KWORD)}"); return; }

                PlayerDataManager.DepositMineral(itemID, amount);
                ShellCore.Say(
                    $"Buy {Util.Format($"{amount}", StrSty.NUMBER, "0")} unit " +
                    $"of {Util.Format(ItemCrafter.ALL_ITEMS[itemID].Shorthand, StrSty.COLORED_ITEM_NAME, $"{itemID}")} " +
                    $"at {Util.Format($"{ItemSeller.PriceModels[itemID].value}", StrSty.MONEY, "", "2")}  " +
                    $"[color={Util.CC(percent > 0 ? Cc.R : percent < 0 ? Cc.G : Cc.Y)}]({percent:+0.00;-0.00;0.00}%)[/color]");
            } else if (sellOne) {
                if (PlayerDataManager.WithdrawMineral(itemID, amount) != CError.OK) { ShellCore.Say("-r", $"Not enough {Util.Format(ItemCrafter.ALL_ITEMS[itemID].Shorthand, StrSty.COLORED_ITEM_NAME, $"{itemID}")}"); return; }

                PlayerDataManager.DepositGC((long)(amount * ItemSeller.PriceModels[itemID].value));
                ShellCore.Say(
                    $"Sell {Util.Format($"{amount}", StrSty.NUMBER, "0")} unit " +
                    $"of {Util.Format(ItemCrafter.ALL_ITEMS[itemID].Shorthand, StrSty.COLORED_ITEM_NAME, $"{itemID}")} " +
                    $"at {Util.Format($"{ItemSeller.PriceModels[itemID].value}", StrSty.MONEY, "", "2")}  " +
                    $"[color={Util.CC(percent > .01 ? Cc.G : percent < -.01 ? Cc.R : Cc.Y)}]({percent:+0.00;-0.00;0.00}%)[/color]");
            }
        } else if (sellAll) {
            didSomething = true;
            double sellMoney = 0;
            for (int i = 0; i < PlayerDataManager.MineInv.Length; ++i) {
                if (PlayerDataManager.MineInv[i] <= 0) continue;
                long c_amount = PlayerDataManager.MineInv[i];
                CError cer = PlayerDataManager.WithdrawMineral(i, c_amount); if (cer != CError.OK) {
                    ShellCore.Say("-r",
                        $"[SELL_INTERRUPTED]\n" +
                        $"Unexpected internal error detected. Please report this bug!\n" +
                        $"Include this in your report: ShellCore.Sell[{itemCode},{t_amount},{sellAll},{options},{i},{PlayerDataManager.MineInv[i]},{t_amount},{cer}]");
                    PlayerDataManager.DepositGC((long)sellMoney);
                    return;
                }

                sellMoney += ItemSeller.PriceModels[i].value * c_amount;
                percent = (ItemSeller.PriceModels[i].value / ItemCrafter.ALL_ITEMS[i].Value - 1.0) * 100;
                ShellCore.Say(
                    $"Sold {Util.Format($"{c_amount}", StrSty.NUMBER, "0")} unit " +
                    $"of {Util.Format(ItemCrafter.ALL_ITEMS[i].Shorthand, StrSty.COLORED_ITEM_NAME, $"{i}")} " +
                    $"at {Util.Format($"{ItemSeller.PriceModels[i].value}", StrSty.MONEY, "", "2")}  " +
                    $"[color={Util.CC(percent > .01 ? Cc.G : percent < -.01 ? Cc.R : Cc.Y)}]({percent:+0.00;-0.00;0.00}%)[/color]");
            }
            PlayerDataManager.DepositGC((long)sellMoney);
            return;
        }
        if (!didSomething) ShellCore.Say($"Run {Util.Format("bitrader --options", StrSty.CMD_FUL)}");
    }
    public static void BitCrafter(Dictionary<string, string> farg, string[] parg) {
        string t_threadID = Util.GetArg(farg, "--threadid", "-t"); int threadID;

        bool doSetThread = Util.ContainKeys(farg, "--item", "-i");
        string itemCode = Util.GetArg(farg, "--item", "-i"); int itemID;

        bool doList = Util.ContainKeys(farg, "--list", "-l");
        bool doOption = Util.ContainKeys(farg, "--option", "-o");

        bool doUpgrade = Util.ContainKeys(farg, "--upgrade", "-u");

        if (doList) {
            ShellCore.Say("All threads:");
            StringBuilder sb = new(""); for (int i = 0; i < ItemCrafter.CurThreads; ++i) 
                sb.Append($"Thread #{i}:" + Util.Format(ItemCrafter.ALL_ITEMS[i].Shorthand, StrSty.COLORED_ITEM_NAME, $"{i}").PadRight(35));
            ShellCore.Say(sb.ToString());
        }
        if (doOption) {
            ShellCore.Say($"Valid value for {Util.Format("--item", StrSty.CMD_FLAG)}:");
            StringBuilder sb = new(""); for (int i = 0; i < ItemCrafter.ALL_ITEMS.Length; ++i) sb.Append(Util.Format(ItemCrafter.ALL_ITEMS[i].Shorthand, StrSty.COLORED_ITEM_NAME, $"{i}").PadRight(30));
            ShellCore.Say(sb.ToString());
        }


        if (doSetThread) {
            if (!int.TryParse(t_threadID, out threadID)) { ShellCore.Say("-r", $"Invalid value for {Util.Format("--threadid", StrSty.CMD_FLAG)}, must be a number"); return; }
            if (threadID >= ItemCrafter.CurThreads) { ShellCore.Say("-r", $"{Util.Format("--threadid", StrSty.CMD_FLAG)} " +
                $"value must be smaller than " +
                $"{Util.Format("CraftThreadCount", StrSty.VARIABLE)}={Util.Format($"{ItemCrafter.CurThreads}", StrSty.NUMBER)}"); return; }
            itemID = ItemCrafter.ALL_ITEMS.FirstOrDefault(x => x.Shorthand == itemCode)?.ID ?? (int.TryParse(itemCode, out int parsedId) ? parsedId : -1);
            if (itemID == -1) { ShellCore.Say("-r", "No item with such ID or symbol found."); return; }
            ItemCrafter.AddItemCraft(ItemCrafter.GetRecipe(itemID), threadID);
            ShellCore.Say($"Assigned Thread #{Util.Format($"{threadID}", StrSty.NUMBER)} with {Util.Format(ItemCrafter.ALL_ITEMS[itemID].Shorthand, StrSty.COLORED_ITEM_NAME, $"{itemID}")}");
        }
        if (doUpgrade) {
            CError cer = ItemCrafter.UpgradeCraftThreadCount();
            switch (cer) {
                case CError.REDUNDANT: ShellCore.Say("-y", $"Max craft thread reached. No more upgrade needed"); break;
                case CError.INSUFFICIENT: ShellCore.Say("-r", $"Not enough {Util.Format("GC", StrSty.AUTO_KWORD)}"); break;
                case CError.OK: ShellCore.Say(Util.Format("Increased craft thread count by 1", StrSty.FULL_SUCCESS)); break;
            }
        }
    }
}