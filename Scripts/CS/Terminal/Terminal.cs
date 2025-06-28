using Godot;

public partial class Terminal : MarginContainer {
	[Export] public RuntimeDirector overseer;
	[Export] RichTextLabel terminalOutputField; [Export] RichTextLabel terminalCommandPrompt;
	[Export] TextEdit terminalCommandField;
	[Export] Timer crackDurationTimer;

	bool isProcessing = false;
	public override void _Ready() {
		ShellCore.IntializeInterface(
			overseer,
			terminalOutputField, terminalCommandPrompt, terminalCommandField, 
			crackDurationTimer);
	}
	public override void _Process(double delta) {
		base._Process(delta);
	}
	public void OnCommandFieldTextChanged() { ShellCore.OnCommandFieldTextChanged(); }
	public void OnCrackDurationTimerTimeout() { ShellCore.OnCrackDurationTimerTimeout(); }
	public void OnFocusEntered() {
		// When the terminal gains focus, grab focus for the command field
		terminalCommandField.GrabFocus();
	}
}
