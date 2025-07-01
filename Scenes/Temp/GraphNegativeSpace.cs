using Godot;
using System;
using System.Linq;

public partial class GraphNegativeSpace : MarginContainer
{
    [Export] RichTextLabel Overlay;
    [Export] Control GrnZoneContainer, RedZoneContainer;
    RichTextLabel[] GrnZ, RedZ;
    [Export] int focusedID = 1;
    const int LINE_COUNT = 8, HALF_LINE_COUNT = LINE_COUNT / 2;
    const int CLEAN_UP_TICK_INTERVAL = 128;
    int curTick = 0;
    static (string, double)[] blocks = new (string, double)[] {
        (" ", 1.0 / LINE_COUNT / 8 * 0),
        ("▁", 1.0 / LINE_COUNT / 8 * 1),
        ("▂", 1.0 / LINE_COUNT / 8 * 2),
        ("▃", 1.0 / LINE_COUNT / 8 * 3),
        ("▄", 1.0 / LINE_COUNT / 8 * 4),
        ("▅", 1.0 / LINE_COUNT / 8 * 5),
        ("▆", 1.0 / LINE_COUNT / 8 * 6),
        ("▇", 1.0 / LINE_COUNT / 8 * 7),
        ("█", 1.0 / LINE_COUNT / 8 * 8),
    };
    public override void _Ready() {
        Overlay.Text = string.Concat(Enumerable.Repeat("································································\n", 12));
        GrnZ = Array.ConvertAll(GrnZoneContainer.GetChildren().ToArray(), e => (RichTextLabel)e);
        RedZ = Array.ConvertAll(RedZoneContainer.GetChildren().ToArray(), e => (RichTextLabel)e);
        for (int i = 0; i < RedZ.Length; ++i) RedZ[i].Text = "································································";
        for (int i = 0; i < GrnZ.Length; ++i) GrnZ[i].Text = "································································";
    }
    public void RenderPriceGraph(double curVal, double basVal) {
        double percentageDelta = (curVal / basVal - 1.0);
        double budgetPercent = Mathf.Abs(percentageDelta);
        int curCharI = blocks.Length - 1;
        if (percentageDelta > 0) {
            for (int i = GrnZ.Length - 1; i >= 0; --i) {
                while (budgetPercent < blocks[curCharI].Item2 && curCharI > 0) --curCharI;
                GrnZ[i].AddText(blocks[curCharI].Item1);
                budgetPercent -= blocks[curCharI].Item2;
            }
            for (int i = 0; i < RedZ.Length; ++i) RedZ[i].AddText(" ");
        } else {
            for (int i = RedZ.Length - 1; i >= 0; --i) {
                while (budgetPercent < blocks[curCharI].Item2 && curCharI > 0) --curCharI;
                RedZ[i].AddText(blocks[curCharI].Item1);
                budgetPercent -= blocks[curCharI].Item2;
            }
            for (int i = 0; i < GrnZ.Length; ++i) GrnZ[i].AddText(" ");
        }
        ++curTick;
        if (curTick == CLEAN_UP_TICK_INTERVAL) {
            for (int i = 0; i < GrnZ.Length; ++i) GrnZ[i].Text = GrnZ[i].Text.Substr(Mathf.Max(GrnZ[i].Text.Length - CLEAN_UP_TICK_INTERVAL, 0), CLEAN_UP_TICK_INTERVAL);
            for (int i = 0; i < RedZ.Length; ++i) RedZ[i].Text = RedZ[i].Text.Substr(Mathf.Max(RedZ[i].Text.Length - CLEAN_UP_TICK_INTERVAL, 0), CLEAN_UP_TICK_INTERVAL);
            curTick = 0;
        }
    }
}
