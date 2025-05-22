using Godot;
public static partial class TerminalProcessor {
    static bool _isProcessing = false; static bool IsProcessing { get { return _isProcessing; } set { _isProcessing = value; } }
    static readonly string[] ProgressChars = [$"[color={Util.CC(Cc.RGB)}]>[/color]", $"[color={Util.CC(Cc.rgb)}]>[/color]"];
    static int tick = 0; static double timeRemain, timeTilNextTick, TimePerTick;
    static string[] queuedCommands = []; static System.Action<string[]> queuedAction = null;
    static void StartProcess(double time) {
        if (IsProcessing) return;
        IsProcessing = true; tick = 0;
        timeRemain = time; TimePerTick = time / 17.0;
        timeTilNextTick = TimePerTick;
        Say("-n", $"   ");
    }
    static void UpdateProcessingGraphic(double delta) {
        if (!IsProcessing) return;
        timeTilNextTick -= delta; timeRemain -= delta;
        while(timeTilNextTick < 0 && tick < 17) {
            timeTilNextTick += TimePerTick;
            ++tick; Say("-n", $"[color=#ffffff{Mathf.Clamp(tick * tick, 1, 255):X2}]>[/color]");
        }
        if (timeRemain <= 0) { ProcessFinished(); return; }
    }
    static void ProcessFinished() {
        IsProcessing = false; terminalCommandField.Editable = true; terminalCommandField.GrabFocus();
        tick = 0; Say("-n", $"\n");
        queuedAction(queuedCommands);
    }
}