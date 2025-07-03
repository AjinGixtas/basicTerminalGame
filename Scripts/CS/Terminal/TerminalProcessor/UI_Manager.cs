using System.Collections.Generic;

public static partial class ShellCore {
    public static void Sidebar(Dictionary<string, string> farg, string[] parg) {
        bool help = Util.ContainKeys(farg, "--help"); if (help) {
            Say(
@$"Usage: {Util.Format("sidebar --[help] --[lock] --[switch < index >]", StrSty.CMD_FUL)}

Open the sidebar menu to access various features and tools.

Options:
    {Util.Format("--help", StrSty.CMD_FLAG)}          Show this help information.

    {Util.Format("--lock, -l", StrSty.CMD_FLAG)}      Toggle auto-switching of sidebar pages.
                    When locked, the sidebar stays on the current page.

    {Util.Format("--switch, -s", StrSty.CMD_FLAG)}    Switch the sidebar to a specific page index.
                    Must be a number between 0 and the total number of pages.

Examples:
    {Util.Format("sidebar --help", StrSty.CMD_FUL)}            Show help.
    {Util.Format("sidebar --lock", StrSty.CMD_FUL)}            Toggle the sidebar auto-switch.
    {Util.Format("sidebar --switch 2", StrSty.CMD_FUL)}        Switch to page 2.");
            return;
        }
        bool lockPage = Util.ContainKeys(farg, "--lock", "-l"); if (lockPage) {
            T_Sidebar.DoSwitch = !T_Sidebar.DoSwitch;
            if (T_Sidebar.DoSwitch) Say(Util.Format("Sidebar continue auto switch.", StrSty.FULL_SUCCESS));
            else Say(Util.Format("Sidebar stopped auto switch.", StrSty.FULL_SUCCESS));
            return;
        }
        bool switchPage = Util.ContainKeys(farg, "--switch", "-s"); string pageIndex = Util.GetArg(farg, "--switch", "-s"); if (switchPage) {
            if (!int.TryParse(pageIndex, out int index) || index < 0 || index >= T_Sidebar.pageAmount) {
                Say("-r", "Invalid page index. Use --help for usage."); return;
            }
            T_Sidebar.PageIndex = index;
            return;
        }
        Say($"No action taken. Use {Util.Format("cmd --help", StrSty.CMD_FUL)} for usage.");
    }
}