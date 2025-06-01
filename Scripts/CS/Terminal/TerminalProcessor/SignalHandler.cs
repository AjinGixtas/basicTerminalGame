public static partial class TerminalProcessor {
    public static void OnCrackDurationTimerTimeout() {
        Say($"{Util.Format("Flare sequence timeout", StrType.ERROR)}. All [color={Util.CC(Cc.Y)}]lok[/color] closed.");
        endEpoch = 0;
        crackDurationTimer.Stop();
        if (sectorAttacked.Count > 0) {
            foreach (DriftSector sector in sectorAttacked) {
                NetworkManager.DisconnectFromSector(sector);
                NetworkManager.RemoveSector(sector);
            }
            sectorAttacked.Clear();
        }
    }
    public static void OnCommandFieldTextChanged() {
        int pastCaretPos = terminalCommandField.GetCaretColumn();
        terminalCommandField.Text = terminalCommandField.Text.Replace("\n", "").Replace("\r", "");
        terminalCommandField.SetCaretColumn(pastCaretPos);
    }
}
