using Godot;

public partial class Terminal : MarginContainer {
	[Export] public Overseer overseer;
	[Export] RichTextLabel terminalOutputField; [Export] RichTextLabel terminalCommandPrompt;
	[Export] TextEdit terminalCommandField;
	[Export] NetworkManager networkManager;
	[Export] Timer processTimer, updateProcessGraphicTimer, crackDurationTimer;
    string userName;

	bool isProcessing = false;
	public override void _Ready() {
		TerminalProcessor.Intialize(
			overseer,
			terminalOutputField, terminalCommandPrompt, terminalCommandField, 
			processTimer, updateProcessGraphicTimer, crackDurationTimer, 
			networkManager);
	}
	public override void _Process(double delta) {
		base._Process(delta);
		TerminalProcessor.Process(delta);
	}
    public void OnCommandFieldTextChanged() { TerminalProcessor.OnCommandFieldTextChanged(); }
	public void ProcessFinished() { TerminalProcessor.ProcessFinished(); }
	public void UpdateProcessingGraphic() { TerminalProcessor.UpdateProcessingGraphic(); }
	public void OnCrackDurationTimerTimeout() { TerminalProcessor.OnCrackDurationTimerTimeout(); }
}
