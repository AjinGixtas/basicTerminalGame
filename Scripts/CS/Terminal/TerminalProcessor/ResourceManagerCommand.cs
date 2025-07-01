using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class ShellCore {
    public static void BiTrader(Dictionary<string, string> farg, string[] parg) {
        bool help = Util.ContainKeys(farg, "--help"); if (help) {
            Say(
$@"Welcome to {Util.Format("BitTrader", StrSty.CMD_ACT)}
This program allows you to trade items with the market.
Usage: {Util.Format("bitrader --[option] --<(item&amount)^all> --<sell|detail>", StrSty.CMD_FUL)}
    {Util.Format("-o --option", StrSty.CMD_FLAG)}: List out valid item codes.
    
    {Util.Format("-d --desc", StrSty.CMD_FLAG)}: Set mode to list out details of item.
    {Util.Format("-s --sell", StrSty.CMD_FLAG)}: Set mode to sell item.

    {Util.Format("-i --item <item_code>", StrSty.CMD_FLAG)}: Specify the item code to sell or buy.
    {Util.Format("-a --amount <amount_int>", StrSty.CMD_FLAG)}: Specify the amount of items to sell or buy.
    
    {Util.Format("-A --all", StrSty.CMD_FLAG)}: Set action to all item.
"); return;
        }
        bool doOption = Util.ContainKeys(farg, "-o", "--option");

        bool doDesc = Util.ContainKeys(farg, "-d", "--desc");
        bool doSell = Util.ContainKeys(farg, "-s", "--sell");

        bool doAll = Util.ContainKeys(farg, "-A", "--all");

        bool doOne = Util.ContainKeys(farg, "-i", "--item", "-a", "--amount");
        string itemCode = Util.GetArg(farg, "-i", "--item"); int itemID = -1;
        string amountCode = Util.GetArg(farg, "-a", "--amount"); long amount = 0;


        if (doOption) {
            ShellCore.Say($"Valid value for {Util.Format("--item", StrSty.CMD_FLAG)}:");
            StringBuilder sb = new(""); for (int i = 0; i < ItemCrafter.ALL_ITEMS.Length; ++i) sb.Append(Util.Format(ItemCrafter.ALL_ITEMS[i].Shorthand, StrSty.COLORED_ITEM_NAME, $"{i}").PadRight(30));
            ShellCore.Say(sb.ToString());
            return;
        }
        
        if (!(doSell || doDesc || doAll || doOne)) { ShellCore.Say("-r", $"Try {Util.Format("bitrader --help", StrSty.CMD_FUL)}"); return; }
        if (doAll && doOne) { ShellCore.Say("-r", "Cannot configure for all items and specific item at the same time."); return; }
        if (!doAll && !doOne) { ShellCore.Say("-r", "Must configure for either all items or specific item."); return; }
        if (!(doSell || doDesc)) { ShellCore.Say("-r", $"No action specified. Use {Util.Format("--buy", StrSty.CMD_ACT)} or {Util.Format("--desc", StrSty.CMD_ACT)} to specify an action."); return; }
        
        if (doOne) {
            // Parse data
            if (string.IsNullOrEmpty(amountCode)) { ShellCore.Say("-r", $"Missing value for {Util.Format("--amount", StrSty.CMD_FLAG)}"); return; }
            if (!long.TryParse(amountCode, out amount)) { ShellCore.Say("-r", $"Invalid value for {Util.Format("--amount", StrSty.CMD_FLAG)}, must be a number"); return; }
            
            if (string.IsNullOrEmpty(itemCode)) { ShellCore.Say("-r", $"Missing value for {Util.Format("--item", StrSty.CMD_FLAG)}"); return; }
            itemID = ItemCrafter.ALL_ITEMS.FirstOrDefault(x => x.Shorthand == itemCode)?.ID ?? (int.TryParse(itemCode, out int parsedId) ? parsedId : -1);
            if (itemID == -1) { ShellCore.Say("-r", $"No item with {Util.Format("item_code", StrSty.CMD_ARG)}={itemCode} found."); return; }
            if (itemID >= ItemCrafter.ALL_ITEMS.Length) { 
                ShellCore.Say("-r", $"Invalid item ID: {itemID}. Must be in range [{Util.Format("0", StrSty.NUMBER)}, {Util.Format($"{ItemCrafter.ALL_ITEMS.Length}", StrSty.NUMBER)})"); return; 
            }
        }

        string gcStr;
        long sellValue = 0;
        CError withdrawCer;
        if (doSell) {
            if (doOne) {
                gcStr = Util.Format($"{ItemCrafter.ALL_ITEMS[itemID].Value}", StrSty.MONEY, "", "2");
                
                
                sellValue = (long)(ItemCrafter.ALL_ITEMS[itemID].Value * amount);
                withdrawCer = PlayerDataManager.WithdrawItem(itemID, amount);
                switch (withdrawCer) {
                    case CError.OK:
                        PlayerDataManager.DepositGC(sellValue);
                        ShellCore.Say(
                            $"Sold {Util.Format($"{amount}", StrSty.NUMBER, "0")} unit of " +
                            $"{Util.Format(ItemCrafter.ALL_ITEMS[itemID].Shorthand, StrSty.COLORED_ITEM_NAME, $"{itemID}"),-27} " +
                            $"at {gcStr}");
                        break;
                    case CError.INSUFFICIENT:
                        ShellCore.Say("-r", $"Not enough {Util.Format(ItemCrafter.ALL_ITEMS[itemID].Shorthand, StrSty.COLORED_ITEM_NAME, $"{itemID}")} to sell {Util.Format($"{amount}", StrSty.NUMBER)} unit.");
                        break;
                    default:
                        ShellCore.Say("-r", $"Unexpected error occurred while selling item. Error code: {withdrawCer}");
                        break;
                }
                return;
            }
            for (itemID = 0; itemID < PlayerDataManager.MineInv.Length; ++itemID) {
                gcStr = Util.Format($"{ItemCrafter.ALL_ITEMS[itemID].Value}", StrSty.MONEY, "", "2");

                amount = PlayerDataManager.MineInv[itemID];
                withdrawCer = PlayerDataManager.WithdrawItem(itemID, amount);
                if (withdrawCer != CError.OK) {
                    PlayerDataManager.DepositGC(sellValue); // Deposit money back if error occurs
                    ShellCore.Say("-r", $"Unexpected error occurred while selling item of ID = {Util.Format($"{itemID}", StrSty.NUMBER)}. Deposited money. Error code: {withdrawCer}");
                    return;
                }
                if (amount <= 0) continue; // Skip if no mineral to sell
                ShellCore.Say(
                    $"Sold {Util.Format($"{amount}", StrSty.NUMBER, "0")} unit of " +
                    $"{Util.Format(ItemCrafter.ALL_ITEMS[itemID].Shorthand, StrSty.COLORED_ITEM_NAME, $"{itemID}"),-27} ");
                sellValue += (long)(ItemCrafter.ALL_ITEMS[itemID].Value * amount);
            }
            return;
        }
        if (doDesc) {
            if (doOne) {
                gcStr = Util.Format($"{ItemCrafter.ALL_ITEMS[itemID].Value}", StrSty.MONEY);

                ShellCore.Say($"{Util.Format(ItemCrafter.ALL_ITEMS[itemID].Shorthand, StrSty.COLORED_ITEM_NAME, $"{itemID}"),-27} " +
                    $"Price: {gcStr.PadLeft(gcStr.Length + (17 - Util.RemoveBBCode(gcStr).Length))}   ");
                return;
            }
            for (itemID = 0; itemID < ItemCrafter.ALL_ITEMS.Length; ++itemID) {
                gcStr = Util.Format($"{ItemCrafter.ALL_ITEMS[itemID].Value}", StrSty.MONEY);
                ShellCore.Say($"{Util.Format(ItemCrafter.ALL_ITEMS[itemID].Shorthand, StrSty.COLORED_ITEM_NAME, $"{itemID}"),-27} " +
                    $"Price: {gcStr.PadLeft(gcStr.Length + (17 - Util.RemoveBBCode(gcStr).Length))}   ");
            }
            return;
        }
    }
    public static void BitCraft(Dictionary<string, string> farg, string[] parg) {
        bool help = Util.ContainKeys(farg, "--help"); if (help) {
            Say(
$@"Welcome to {Util.Format("BitCraft", StrSty.CMD_ACT)}
This program allows you to craft items using the resources you have.
Usage: {Util.Format("bitcraft --[list] --[option] --[item&threadid] --[upgrade]", StrSty.CMD_FUL)}
    {Util.Format("--list", StrSty.CMD_FLAG)}: List out details of each craft thread.
    {Util.Format("--option", StrSty.CMD_FLAG)}: List out valid item codes.
    {Util.Format("--item", StrSty.CMD_FLAG)}: Specify the item code to craft.
    {Util.Format("--threadid", StrSty.CMD_FLAG)}: Specify the thread ID to assign the item to.
    {Util.Format("--upgrade", StrSty.CMD_FLAG)}: Upgrade the number of craft threads available.
");
            return;
        }
        bool didSomething = false;

        bool doList = Util.ContainKeys(farg, "--list", "-l");
        bool doOption = Util.ContainKeys(farg, "--option", "-o");

        bool doSetThread = Util.ContainKeys(farg, "--threadid", "-t");
        string t_threadID = Util.GetArg(farg, "--threadid", "-t"); int threadID;
        string itemCode = Util.GetArg(farg, "--item", "-i"); int itemID;


        bool doUpgrade = Util.ContainKeys(farg, "--upgrade", "-u");

        if (doList) {
            didSomething = true;
            ShellCore.Say("All threads:");
            StringBuilder sb = new(""); for (int i = 0; i < ItemCrafter.CurThreads; ++i) {
                if (ItemCrafter.CraftThreads[i].Recipe == null) {
                    sb.Append($"Thread #{Util.Format($"{i}", StrSty.NUMBER, "0")}: {Util.Format("null", StrSty.DECOR)}     {Util.Format("0", StrSty.NUMBER)}%         RemainingTime: {Util.Format("0",StrSty.NUMBER)}{Util.Format("s", StrSty.DECOR)}\n");
                    continue;
                }
                sb.Append($"Thread #{Util.Format($"{i}", StrSty.NUMBER, "0")}: "
                + Util.Format(ItemCrafter.CraftThreads[i].Recipe.Shorthand, StrSty.COLORED_ITEM_NAME, $"{ItemCrafter.CraftThreads[i].Recipe.ID}").PadRight(30)
                + $"{Util.Format($"{ItemCrafter.CraftThreads[i].RemainTime / ItemCrafter.CraftThreads[i].Recipe.CraftTime * 100.0}", StrSty.NUMBER)}%".PadLeft(30)
                + "         "
                + $"RemainingTime: {Util.Format($"{ItemCrafter.CraftThreads[i].RemainTime}", StrSty.NUMBER)}{Util.Format("s", StrSty.DECOR)}\n");
            }
            ShellCore.Say(sb.ToString());
            GD.Print(ItemCrafter.CurThreads);
        }
        if (doOption) {
            didSomething = true;
            ShellCore.Say($"Valid value for {Util.Format("--item", StrSty.CMD_FLAG)}:");
            StringBuilder sb = new(""); for (int i = 0; i < ItemCrafter.ALL_ITEMS.Length; ++i) sb.Append(Util.Format(ItemCrafter.ALL_ITEMS[i].Shorthand, StrSty.COLORED_ITEM_NAME, $"{i}").PadRight(30));
            ShellCore.Say(sb.ToString());
        }
        if (doSetThread) {
            didSomething = true;
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
            didSomething = true;
            CError cer = ItemCrafter.UpgradeCraftThreadCount();
            switch (cer) {
                case CError.REDUNDANT: ShellCore.Say("-y", $"Max craft thread reached. No more upgrade needed"); break;
                case CError.INSUFFICIENT: ShellCore.Say("-r", $"Not enough {Util.Format("GC", StrSty.AUTO_KWORD)}"); break;
                case CError.OK: ShellCore.Say(Util.Format("Increased craft thread count by 1", StrSty.FULL_SUCCESS)); break;
            }
        }
        
        if (!didSomething) ShellCore.Say($"Run {Util.Format("bitcraft --help", StrSty.CMD_FUL)}");
    }
}