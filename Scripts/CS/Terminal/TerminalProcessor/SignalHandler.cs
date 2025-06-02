using Godot;
using System;

public static partial class TerminalProcessor {
    public static void OnCrackDurationTimerTimeout() {
        Say($"{Util.Format("Flare sequence timeout", StrType.ERROR)}. All [color={Util.CC(Cc.Y)}]locks[/color] closed.");
        EndFlare();
    }
    static void EndFlare() {
        endEpoch = 0;
        crackDurationTimer.Stop();
        foreach (WeakReference<DriftSector> sectorRef in sectorAttacked) {
            if (!sectorRef.TryGetTarget(out DriftSector sector)) continue;
            NetworkManager.DisconnectFromSector(sector);
            NetworkManager.RemoveSector(sector);
        }
        sectorAttacked.Clear();
        GC.Collect();                    // Try to collect unreachable objects
        GC.WaitForPendingFinalizers();   // Wait for destructors (~finalizers)
        GC.Collect();                    // Re-collect objects that were just finalized
    }
    public static void OnCommandFieldTextChanged() {
        int pastCaretPos = terminalCommandField.GetCaretColumn();
        terminalCommandField.Text = terminalCommandField.Text.Replace("\n", "").Replace("\r", "");
        terminalCommandField.SetCaretColumn(pastCaretPos);
    }
}
