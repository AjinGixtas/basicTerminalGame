using Godot;

public partial class Terminal : MarginContainer {
	[Export] public RuntimeDirector overseer;
	[Export] RichTextLabel terminalOutputField; [Export] RichTextLabel terminalCommandPrompt;
	[Export] TextEdit terminalCommandField;
	[Export] Timer crackDurationTimer;

	bool isProcessing = false;
	public override void _Ready() {
		TerminalProcessor.IntializeInterface(
			overseer,
			terminalOutputField, terminalCommandPrompt, terminalCommandField, 
			crackDurationTimer);
	}
	public override void _Process(double delta) {
		base._Process(delta);
		TerminalProcessor.Process(delta);
		NetworkManager.CollectHackFarmMinerals(delta);
	}
	public void OnCommandFieldTextChanged() { TerminalProcessor.OnCommandFieldTextChanged(); }
	public void OnCrackDurationTimerTimeout() { TerminalProcessor.OnCrackDurationTimerTimeout(); }
}
