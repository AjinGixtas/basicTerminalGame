using Godot;
using System.Linq;
using System.Text.RegularExpressions;

public partial class LifeCycleDirector : Node
{
	[Export] PackedScene gameScene;
	RuntimeDirector runtimeDirector;
	public override void _Ready() {
		// Load the game scene
		if (gameScene != null) {
			// Intialize default state
			NetworkManager.Ready();
			PlayerFileManager.Ready();
			PlayerDataManager.Ready();
			QuickLoad(this);
		} else { GD.PrintErr("Game scene not set in LifeCycleDirector."); }
	}
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("quickSaveGame")) { 
			QuickSave();
		}
		if (Input.IsActionJustPressed("quickLoadGame")) { 
			QuickLoad(this);
		}
	}
	private const string SaveRoot = "user://Saves";
	
	
	static void QuickSave() {
		// Ensure the save root directory exists
		DirAccess.MakeDirAbsolute(ProjectSettings.GlobalizePath(SaveRoot));

		// Find existing save folders named like "Game_###"
		var existingSaves = DirAccess.GetDirectoriesAt(SaveRoot).Select(dir => {
			var match = Regex.Match(dir, @"Game_(\d+)$");
			return match.Success ? int.Parse(match.Groups[1].Value) : 0;
		}).Where(num => num > 0).ToList();

		// Pick next save number
		int nextSaveNumber = existingSaves.Count == 0 ? 1 : existingSaves.Max() + 1;
		string newSaveFolderName = $"Game_{nextSaveNumber:D3}";
		string newSavePath = StringExtensions.PathJoin(SaveRoot, newSaveFolderName);

		// Create new save directory
		DirAccess.MakeDirAbsolute(ProjectSettings.GlobalizePath(newSavePath));
		// Save player data (convert back to user:// path for Godot API)
		TerminalProcessor.Say($"Quick saving to {newSavePath}");
		TerminalProcessor.Say(PlayerDataManager.GetSaveStatusMsg(
			PlayerDataManager.SavePlayerData(StringExtensions.PathJoin(newSavePath, "PlayerData.tres"))));
		TerminalProcessor.Say(PlayerDataManager.GetSaveStatusMsg(
			PlayerFileManager.SaveFileSysData(StringExtensions.PathJoin(newSavePath, "FileSys"))));
		TerminalProcessor.Say($"If there are any error related to save file, feel free to email {Util.Format("ajingixtascontact", StrType.USERNAME)}@{Util.Format("gmail.com", StrType.HOSTNAME)}");
	}

	static void QuickLoad(LifeCycleDirector lifeCycleDirector) {
		lifeCycleDirector.RemakeScene();
		if (!DirAccess.DirExistsAbsolute(ProjectSettings.GlobalizePath(SaveRoot))) { TerminalProcessor.Say("-r", "No previous record of user found. Intialize new user."); return; }

		// Find all save folders named like "Game_###"
		var saveFolders = DirAccess.GetDirectoriesAt(SaveRoot).Where(dir => Regex.IsMatch(dir, @"Game_\d{3}$")).ToList();

		if (saveFolders.Count == 0) { TerminalProcessor.Say("-r", "No previous record of user found. Intialize new user."); return; }

		// Find the most recently updated one
		
		string latestFolder = saveFolders
			.Where(dir => FileAccess.FileExists(StringExtensions.PathJoin($"{SaveRoot}/{dir}", "PlayerData.tres")))
			.OrderByDescending(dir => FileAccess.GetModifiedTime(StringExtensions.PathJoin($"{SaveRoot}/{dir}", "PlayerData.tres")))
			.FirstOrDefault();
		GD.Print(saveFolders[0]);
		TerminalProcessor.Say($"Loading save from: {latestFolder}");
		// Load global data
		int[] statusCodes = [
			PlayerDataManager.LoadPlayerData(StringExtensions.PathJoin($"{SaveRoot}/{latestFolder}", "PlayerData.tres")),
			PlayerFileManager.LoadFileSysData(StringExtensions.PathJoin($"{SaveRoot}/{latestFolder}", "FileSys")),
		];
		// Wipe the slate clean
		lifeCycleDirector.RemakeScene();

		TerminalProcessor.Say("Quick loading save...");
		TerminalProcessor.Say(PlayerDataManager.GetLoadStatusMsg(statusCodes[0]));
		TerminalProcessor.Say(PlayerFileManager.GetLoadStatusMsg(statusCodes[1]));
		TerminalProcessor.Say($"If there are any error related to save file, feel free to email {Util.Format("ajingixtascontact", StrType.USERNAME)}@{Util.Format("gmail.com", StrType.HOSTNAME)}");
	}
	void RemakeScene() {
		if (runtimeDirector != null) RemoveChild(runtimeDirector);
		runtimeDirector = gameScene.Instantiate<RuntimeDirector>();
		AddChild(runtimeDirector);
		TerminalProcessor.IntializeInternal();
	}
}
