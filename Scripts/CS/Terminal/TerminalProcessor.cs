using Godot;
using System.Collections.Generic;

public static partial class ShellCore {
	static Terminal terminal; public static Terminal Terminal => terminal;
	
	public static RuntimeDirector Overseer => terminal.overseer;
    static RichTextLabel TCMD_Prompt => terminal.terminalCommandPrompt;
    static RichTextLabel TOUT_Field => terminal.terminalOutputField; 
	static TextEdit      TINP_Field => terminal.terminalCommandField; 
	static Timer crackDurationTimer => terminal.crackDurationTimer;
	static TerminalSidebar T_Sidebar => terminal.sidebar;

    static NetworkNode _currNode = null; static NodeDirectory _currDir = null;
	public static NodeDirectory CurrDir { get { return _currDir; } private set { _currDir = value; SetCommandPrompt(); } }
	public static NetworkNode CurrNode { 
		get { return _currNode; }
		private set { 
			if (_currNode != value && value != null) { 
				value.NotifyConnected();
				_currNode.NotifyDisconnected();
			} 
			_currNode = value; SetCommandPrompt();
		} 
	}
	static readonly List<string> cmdHistory = []; static int _commandHistoryIndex = 0;
	static int CommandHistoryIndex {
		get => _commandHistoryIndex;
		set {
			value = Mathf.Clamp(value, 0, Mathf.Max(0, cmdHistory.Count - 1));
			_commandHistoryIndex = value;
			if (_commandHistoryIndex < cmdHistory.Count) {
				TINP_Field.Text = cmdHistory[_commandHistoryIndex];
				TINP_Field.SetCaretColumn(cmdHistory[_commandHistoryIndex].Length);
			}
		}
	}
	public static void IntializeInterface(
		Terminal terminal) {

		// Assign provided parameters to static fields
		ShellCore.terminal = terminal;

		// Set the username and command prompt
		ShellCore.TINP_Field.GrabFocus();

        // Mark as initialized
        ShellCore.TINP_Field.ScrollFitContentHeight = true;
		ShellCore.TINP_Field.ScrollFitContentWidth = true;
	}
	public static void IntializeInternal() {
		ShellCore._currNode = NetworkManager.PlayerNode;
		ShellCore._currDir = PlayerFileManager.FileSystem;
		ShellCore.SetCommandPrompt();
	}
	public static void Ready() {
    }
	public static void Process(double delta) {
		HandleInput(delta);
		ShowMoreChars(delta);
		UpdateProcessingGraphic(delta);
	}
	public static void HandleInput(double dela) {
		if (TINP_Field.HasFocus()) {
			if (Input.IsActionJustPressed("moveDownHistory")) CommandHistoryIndex += 1;
			if (Input.IsActionJustPressed("moveUpHistory")) CommandHistoryIndex -= 1;
			if (Input.IsActionJustPressed("submitCommand")) {
				if (TINP_Field.Text.Length == 0) { return; }
				TINP_Field.Text = TINP_Field.Text.Replace("\r", " ").Replace("\n", " ");
				if(SubmitCommand(TINP_Field.Text) == 0) TINP_Field.Text = "";
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
