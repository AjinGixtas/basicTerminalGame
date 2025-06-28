using Godot;
using System.Linq;
public partial class CraftThreadButton : MarginContainer {
	ItemCraftingUserInterface owner;
	[Export] RichTextLabel threadNameLabel;
    [Export] RichTextLabel itemNameLabel;
	[Export] RichTextLabel ingredientLabel;
	[Export] RichTextLabel craftProgressLabel;
	int _id = -1; public int ID { get => _id; private set { if (_id >= 0) return; _id = value; } }
	public void Initialize(int ID, ItemCraftingUserInterface owner) {
		this.ID = ID; this.owner = owner;
		threadNameLabel.Text = $"Thread #{ID}";
        itemNameLabel.Text = "null";
        ingredientLabel.Text = "N/A";
        craftProgressLabel.Text = GenerateProgressBarText(ItemCrafter.CraftThreads[ID]);
        Visible = ID < ItemCrafter.CurThreads;
        ItemCrafter.ThreadAmountChanged += OnThreadAmountChanged;
    }
	public void OnPress() {
		owner.PullupCraftRecipeWindow(ID);
	}
	string GenerateProgressBarText(ItemCrafter.CraftThread thread) {
        double progress = thread.Recipe == null ? 0 : (thread.Recipe.CraftTime - thread.RemainTime) / thread.Recipe.CraftTime;
        int percentage = (int)(progress * 100); int _bar = percentage / 5;
        string bar = new string('|', _bar) + new string('-', 20 - _bar);
        return $"[{bar}] {percentage,3}%";
    }
    public override void _Process(double delta) {
        itemNameLabel.Text = ItemCrafter.CraftThreads[ID].Recipe != null
    ? $"([color={Util.CC(ItemCrafter.CraftThreads[ID].Recipe?.ColorCode ?? Cc.W)}]{ItemCrafter.CraftThreads[ID].Recipe.Shorthand}[/color])\n[color={Util.CC(ItemCrafter.CraftThreads[ID].Recipe?.ColorCode ?? Cc.W)}]{ItemCrafter.CraftThreads[ID].Recipe.Name}[/color]"
    : $"[color={Util.CC(Cc.W)}]null[/color]";
        ingredientLabel.Text = ItemCrafter.CraftThreads[ID].Recipe?.RequiredIngredients.Length > 0
            ? string.Join(", ", ItemCrafter.CraftThreads[ID].Recipe.RequiredIngredients.Select(ing => $"[color={Util.CC(ItemCrafter.ALL_ITEMS[ing.ID].ColorCode)}]{ItemCrafter.ALL_ITEMS[ing.ID].Shorthand}[/color]  x{ing.Amount}"))
            : "N/A";
        craftProgressLabel.Text = GenerateProgressBarText(ItemCrafter.CraftThreads[ID]);
    }
    void OnThreadAmountChanged(int thread) {
        Visible = ID < thread;
    }
}
