using Godot;

public partial class Terminal : MarginContainer {
	public Overseer overseer;
	RichTextLabel terminalOutputField; RichTextLabel terminalCommandPrompt; TextEdit terminalCommandField; Timer processTimer, updateProcessGraphicTimer;
	NetworkManager networkManager;
	NetworkNode currentNode = null; NodeDirectory currentDirectory = null;
	string userName;

	bool isProcessing = false;
	public override void _Ready() {
		IntializeOnReadyVar();
		currentNode = networkManager.network;
		TerminalProcessor.Intialize(overseer, terminalOutputField, terminalCommandPrompt, terminalCommandField, processTimer, updateProcessGraphicTimer, networkManager);
	}
	void IntializeOnReadyVar() {
		terminalOutputField = GetNode<RichTextLabel>("Splitter/TerminalOutputArea");
		terminalCommandField = GetNode<CodeEdit>("Splitter/TerminalCommandLine/CommandField");
		terminalCommandPrompt = GetNode<RichTextLabel>("Splitter/TerminalCommandLine/CommandPrompt");
		processTimer = GetNode<Timer>("Splitter/TimersContainer/ProcessTimer");
		updateProcessGraphicTimer = GetNode<Timer>("Splitter/TimersContainer/UpdateProcessGraphicTimer");
		networkManager = GetNode<NetworkManager>("Splitter/NetworkManager");
	}
	public override void _Process(double delta) {
		base._Process(delta);
		TerminalProcessor.Process(delta);
	}
	public void OnCommandFieldTextChanged() { TerminalProcessor.OnCommandFieldTextChanged(); }
	public void ProcessFinished() { TerminalProcessor.ProcessFinished(); }
	public void UpdateProcessingGraphic() { TerminalProcessor.UpdateProcessingGraphic(); }
}
