using Godot;
using System.Collections.Generic;

public static partial class ShellCore {
	static RuntimeDirector overseer;
	static RichTextLabel terminalOutputField; static RichTextLabel terminalCommandPrompt; 
	static TextEdit terminalCommandField; 
	static Timer crackDurationTimer;
	static NetworkNode _currNode = null; static NodeDirectory _currDir = null;
	public static NodeDirectory CurrDir { get { return _currDir; } private set { _currDir = value; SetCommandPrompt(); } }
	public static NetworkNode CurrNode { get { return _currNode; } private set { _currNode = value; SetCommandPrompt(); } }
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
		ShellCore.overseer = overseer;
		ShellCore.terminalOutputField = terminalOutputField;
		ShellCore.terminalCommandPrompt = terminalCommandPrompt;
		ShellCore.terminalCommandField = terminalCommandField;
		ShellCore.crackDurationTimer = crackDurationTimer;

		// Set the username and command prompt
		ShellCore.terminalCommandField.GrabFocus();

        // Mark as initialized
        ShellCore.terminalCommandField.ScrollFitContentHeight = true;
		ShellCore.terminalCommandField.ScrollFitContentWidth = true;
	}
	public static void IntializeInternal() {
		ShellCore._currNode = NetworkManager.PlayerNode;
		ShellCore._currDir = PlayerFileManager.FileSystem;
		ShellCore.SetCommandPrompt();
	}
	public static void Ready() {
        if (!PlayerDataManager.CompletedTutorial) {
            ShellCore.Say(@$"Welcome to the terminal! Type {Util.Format("help", StrType.CMD_FUL)} to see available commands.
You can also type {Util.Format("scan", StrType.CMD_FUL)} to see the network nodes around you.
Type {Util.Format("connect <hostname>", StrType.CMD_FUL)} to connect to a node.
Type {Util.Format("home", StrType.CMD_FUL)} to return to your home node.");
        }
    }
	public static void Process(double delta) {
		HandleInput(delta);
		ShowMoreChars(delta);
		UpdateProcessingGraphic(delta);
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
