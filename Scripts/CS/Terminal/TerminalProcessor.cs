using Godot;
using System.Collections.Generic;

public static partial class TerminalProcessor {
	static RuntimeDirector overseer;
	static RichTextLabel terminalOutputField; static RichTextLabel terminalCommandPrompt; 
	static TextEdit terminalCommandField; 
	static Timer crackDurationTimer;
	static NetworkNode _currNode = null; static NodeDirectory _currDir = null;
	public static NodeDirectory CurrDir { get { return _currDir; } set { _currDir = value; SetCommandPrompt(); } }
	static NetworkNode CurrNode { get { return _currNode; } set { _currNode = value; SetCommandPrompt(); } }
	static readonly List<string> commandHistory = []; static int _commandHistoryIndex = 0;
	static int CommandHistoryIndex {
		get => _commandHistoryIndex;
		set {
			value = Mathf.Clamp(value, 0, Mathf.Max(0, commandHistory.Count - 1));
			_commandHistoryIndex = value;
			if (_commandHistoryIndex < commandHistory.Count) {
				terminalCommandField.Text = commandHistory[_commandHistoryIndex];
				terminalCommandField.SetCaretColumn(commandHistory[_commandHistoryIndex].Length);
			}
		}
	}
	public static void IntializeInterface(
		RuntimeDirector overseer, RichTextLabel terminalOutputField, 
		RichTextLabel terminalCommandPrompt, TextEdit terminalCommandField, 
		Timer crackDurationTimer) {

		// Assign provided parameters to static fields
		TerminalProcessor.overseer = overseer;
		TerminalProcessor.terminalOutputField = terminalOutputField;
		TerminalProcessor.terminalCommandPrompt = terminalCommandPrompt;
		TerminalProcessor.terminalCommandField = terminalCommandField;
		TerminalProcessor.crackDurationTimer = crackDurationTimer;

		// Set the username and command prompt
		terminalCommandField.GrabFocus();

		// Mark as initialized
		terminalCommandField.ScrollFitContentHeight = true;
		terminalCommandField.ScrollFitContentWidth = true;
	}
	public static void IntializeInternal() {
		TerminalProcessor._currNode = NetworkManager.playerNode;
		TerminalProcessor._currDir = PlayerFileManager.FileSystem;
		TerminalProcessor.SetCommandPrompt();
	}
	public static void Process(double delta) {
		HandleInput(delta);
		ShowMoreChars(delta);
		UpdateProcessingGraphic(delta);
		NetworkManager.CollectHackFarmMoney(delta);
	}
	public static void HandleInput(double dela) {
		if (terminalCommandField.HasFocus()) {
			if (Input.IsActionJustPressed("moveDownHistory")) CommandHistoryIndex += 1;
			if (Input.IsActionJustPressed("moveUpHistory")) CommandHistoryIndex -= 1;
			if (Input.IsActionJustPressed("submitCommand")) {
				if (terminalCommandField.Text.Length == 0) { return; }
				terminalCommandField.Text = terminalCommandField.Text.Replace("\r", " ").Replace("\n", " ");
				if(SubmitCommand(terminalCommandField.Text) == 0) terminalCommandField.Text = "";
			}
		}
	}

}
public struct NodeAnalysis {
	public string IP { get; init; }
	public string HostName { get; init; }
	public string DisplayName { get; init; }
	public int DefLvl { get; init; }
	public int SecLvl { get; init; }
	public int RetLvl { get; init; }
	public SecurityType SecType { get; init; }
	public NodeType NodeType { get; init; }
}
