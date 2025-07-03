using Godot;

public partial class Terminal : MarginContainer {
	[Export] public RuntimeDirector overseer;
	[Export] public TerminalSidebar sidebar;
    [Export] public RichTextLabel terminalOutputField; [Export] public RichTextLabel terminalCommandPrompt;
	[Export] public TextEdit terminalCommandField;
	[Export] public Timer crackDurationTimer;

	bool isProcessing = false;
	public override void _Ready() {
		ShellCore.IntializeInterface(this);
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
