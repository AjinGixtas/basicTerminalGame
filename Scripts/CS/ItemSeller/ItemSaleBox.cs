using Godot;
using System.Linq;
using System.Text;

public partial class ItemSaleBox : Control {
	public void Init(int id) { itemID = id; }
	[Export] RichTextLabel priceGraph;
	const int LINE_COUNT = 18, LINE_LENGTH = 40;
	static readonly (char, double)[] graphChar = new (char, double)[] {
		(' ', 1.0 / LINE_COUNT / 8.0 * 0.0),
		('▁', 1.0 / LINE_COUNT / 8.0 * 1.0),
		('▂', 1.0 / LINE_COUNT / 8.0 * 2.0),
		('▃', 1.0 / LINE_COUNT / 8.0 * 3.0),
		('▄', 1.0 / LINE_COUNT / 8.0 * 4.0),
		('▅', 1.0 / LINE_COUNT / 8.0 * 5.0),
		('▆', 1.0 / LINE_COUNT / 8.0 * 6.0),
		('▇', 1.0 / LINE_COUNT / 8.0 * 7.0),
		('█', 1.0 / LINE_COUNT / 8.0 * 8.0),
	};
	[Export] int itemID = -1;
	StringBuilder[] sbs = new StringBuilder[LINE_COUNT];
    const string pattern = "[color=#123456]-[/color]";

	[Export] RichTextLabel BasePriceOverlay;
    public override void _Ready() {
        string repeated = string.Concat(Enumerable.Repeat(pattern, LINE_LENGTH));
        for (int i = 0; i < LINE_COUNT; i++) sbs[i] = new(repeated);
		BasePriceOverlay.Text = $"[color={Util.CC(Cc.w)}]" + 
			string.Concat(Enumerable.Repeat(new string('-', LINE_LENGTH) + '\n', LINE_COUNT/3)) +
            string.Concat(Enumerable.Repeat(new string('-', LINE_LENGTH) + '\n', LINE_COUNT/3-1)) +
            string.Concat(Enumerable.Repeat(new string('─', LINE_LENGTH) + '\n', 1)) +
            string.Concat(Enumerable.Repeat(new string('-', LINE_LENGTH) + '\n', LINE_COUNT/3)) + "[/color]";
    }
	public void RenderGraph() {
		string[] queue = GenerateGraphComponent(itemID);
		StringBuilder sb = new();
		for (int i = 0; i < sbs.Length; i++) {
			sbs[i].Remove(0, pattern.Length); sbs[i].Append(queue[i]);
			sb.Append(sbs[i]);
			sb.Append('\n');
		}
		priceGraph.Text = sb.ToString();
	}
	static string[] GenerateGraphComponent(int itemID) {
		double percentage = (ItemSeller.ItemPricesModel[itemID].value - ItemSeller.ItemPricesModel[itemID].dMiVal) / ItemSeller.ItemPricesModel[itemID].raVal;
        string[] result = new string[LINE_COUNT];
		int bI = graphChar.Length-1; /* Index iterate backward */
		for (int i = LINE_COUNT-1; i > -1; --i) {
			while (percentage < graphChar[bI].Item2 && bI > 0) --bI;
			result[i] = $"[color={Util.CC((ItemSeller.ItemPricesModel[itemID].value > ItemSeller.ItemPricesModel[itemID].pastValue) ? Cc.g : (ItemSeller.ItemPricesModel[itemID].value < ItemSeller.ItemPricesModel[itemID].pastValue) ? Cc.r : Cc.y)}]{graphChar[bI].Item1}[/color]";
			percentage -= graphChar[bI].Item2;
		}
		return result;
	}
}
