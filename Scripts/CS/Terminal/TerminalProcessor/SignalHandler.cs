public static partial class TerminalProcessor {
    public static void OnCrackDurationTimerTimeout() {
        Say($"{Util.Format("Flare sequence timeout", StrType.ERROR)}. All [color={Util.CC(Cc.Y)}]locks[/color] closed.");
        EndFlare();
    }

    public static void OnCommandFieldTextChanged() {
        int pastCaretPos = terminalCommandField.GetCaretColumn();
        terminalCommandField.Text = terminalCommandField.Text.Replace("\n", "").Replace("\r", "");
        terminalCommandField.SetCaretColumn(pastCaretPos);
    }
}
