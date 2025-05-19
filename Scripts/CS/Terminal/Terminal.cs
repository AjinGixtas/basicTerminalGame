using Godot;

public partial class Terminal : MarginContainer {
	[Export] public RuntimeDirector overseer;
	[Export] RichTextLabel terminalOutputField; [Export] RichTextLabel terminalCommandPrompt;
	[Export] TextEdit terminalCommandField;
	[Export] Timer processTimer, updateProcessGraphicTimer, crackDurationTimer;

	bool isProcessing = false;
	public override void _Ready() {
		TerminalProcessor.IntializeInterface(
			overseer,
			terminalOutputField, terminalCommandPrompt, terminalCommandField, 
			processTimer, updateProcessGraphicTimer, crackDurationTimer);
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
