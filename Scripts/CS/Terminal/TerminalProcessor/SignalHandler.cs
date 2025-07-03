public static partial class ShellCore {
    public static void OnCrackDurationTimerTimeout() {
        Say($"{Util.Format("Flare sequence timeout", StrSty.ERROR)}. All [color={Util.CC(Cc.Y)}]locks[/color] closed.");
        EndFlare();
    }

    public static void OnCommandFieldTextChanged() {
        int pastCaretPos = TINP_Field.GetCaretColumn();
        TINP_Field.Text = TINP_Field.Text.Replace("\n", "").Replace("\r", "");
        TINP_Field.SetCaretColumn(pastCaretPos);
    }
}
