using Godot;
public static partial class TerminalProcessor {
    static bool _isProcessing = false; static bool IsProcessing { get { return _isProcessing; } set { _isProcessing = value; } }
    static readonly string[] ProgressChars = [$"[color={Util.CC(Cc.RGB)}]>[/color]", $"[color={Util.CC(Cc.rgb)}]>[/color]"];
    static int tick = 0;
    static string[] queuedCommands = []; static System.Action<string[]> queuedAction = null;
    static void StartProcess(double time) {
        if (IsProcessing) return;
        IsProcessing = true; tick = 0;
        Say("-n", $"   ");
        processTimer.WaitTime = time;
        processTimer.Start();
        updateProcessGraphicTimer.WaitTime = Mathf.Max(.05, time / 17.0);
        updateProcessGraphicTimer.Start();
    }
    public static void UpdateProcessingGraphic() {
        if (!IsProcessing) return;
        ++tick; Say("-n", $"[color=#ffffff{Mathf.Clamp(tick * tick, 1, 255):X2}]>[/color]");
        updateProcessGraphicTimer.Start();
    }
    public static void ProcessFinished() {
        IsProcessing = false; terminalCommandField.Editable = true; terminalCommandField.GrabFocus();
        for (int i = tick + 1; i < 17; ++i) { Say("-n", $"[color=#ffffff{Mathf.Clamp(tick * tick, 1, 255):X2}]>[/color]"); }
        tick = 0; Say("-n", $"\n");
        queuedAction(queuedCommands);
    }
}