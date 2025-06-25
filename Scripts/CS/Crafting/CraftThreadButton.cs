using Godot;
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
        craftProgressLabel.Text = GenerateProgressBarText(ItemCrafter.CraftThreads[ID].TotalTime, ItemCrafter.CraftThreads[ID].RemainTime);
    }
	public void OnPress() {
		owner.PullupCraftRecipeWindow(ID);
	}
	string GenerateProgressBarText(double totalTime, double remainingTime) {
        double progress = (totalTime - remainingTime) / totalTime;
        int percentage = (int)(progress * 100);
        string bar = new string('|', percentage / 20) + new string('-', 20 - percentage / 20);
        return $"[{bar}] {percentage}%";
    }
}
