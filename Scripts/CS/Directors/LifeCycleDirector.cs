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
			PlayerDataManager.Ready();
			PlayerFileManager.Ready();
			NetworkManager.Ready();
			QuickLoad(this);
			ShellCore.Ready();
		} else { GD.PrintErr("Game scene not set in LifeCycleDirector."); }
	}
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("sequentialSave")) { 
			QuickSave(true);
		}
		if (Input.IsActionJustPressed("quickLoadGame")) { 
			QuickLoad(this);
		}
		if (Input.IsActionJustPressed("overwriteSave")) {
			QuickSave(false);
		}
	}
	private const string SaveRoot = "user://Saves";
	static string CurrentSavePath = "";
	public static void QuickSave(bool newSave) {
		// Ensure the save root directory exists
		DirAccess.MakeDirAbsolute(ProjectSettings.GlobalizePath(SaveRoot));

		var existingSaves = DirAccess.GetDirectoriesAt(SaveRoot).Select(dir => {
			var match = Regex.Match(dir, @"Game_(\d+)$");
			return match.Success ? int.Parse(match.Groups[1].Value) : 0;
		}).Where(num => num > 0).ToList();

		// Pick next save number
		int nextSaveNumber = existingSaves.Count == 0 ? 1 : newSave ? existingSaves.Max() + 1 : existingSaves.Max();
		string newSaveFolderName = $"Game_{nextSaveNumber:D5}";
		string newSavePath = StringExtensions.PathJoin(SaveRoot, newSaveFolderName);

		// Create new save directory
		DirAccess.MakeDirAbsolute(ProjectSettings.GlobalizePath(newSavePath));
		// Save player data (convert back to user:// path for Godot API)
		if (newSave) ShellCore.Say($"Saving game to {newSavePath}");
		else ShellCore.Say($"Overwriting save to {newSavePath}");
		ShellCore.Say("-n", PlayerDataManager.GetSaveStatusMsg(
				PlayerDataManager.SavePlayerData(StringExtensions.PathJoin(newSavePath, "PlayerData.tres"))
			) + "... ");
		ShellCore.Say("-n", PlayerFileManager.GetSaveStatusMsg(
				PlayerFileManager.SaveFileSysData(StringExtensions.PathJoin(newSavePath, "FileSys"))
			) + "... ");
		ShellCore.Say("-n", NetworkManager.GetSaveStatusMsg(
				NetworkManager.SaveNetworkData(StringExtensions.PathJoin(newSavePath, "NetworkData"))
			));

		ShellCore.Say($"\nIf there are any error related to save file, feel free to email {Util.Format("ajingixtascontact", StrType.USERNAME)}@{Util.Format("gmail.com", StrType.HOSTNAME)}");
		CurrentSavePath = newSavePath; // Update current save path
	}

	static void QuickLoad(LifeCycleDirector lifeCycleDirector) {
		lifeCycleDirector.RemakeScene();
		if (!DirAccess.DirExistsAbsolute(ProjectSettings.GlobalizePath(SaveRoot))) { ShellCore.Say("-r", "No previous record of user found. Intialize new user."); return; }

		var saveFolders = DirAccess.GetDirectoriesAt(SaveRoot).Where(dir => dir.StartsWith("Game_")).ToList();

		if (saveFolders.Count == 0) { ShellCore.Say("-r", "No previous record of user found in system. Intialize new user."); return; }

		// Find the most recently updated one
		string latestFolder = saveFolders
			.Where(dir => FileAccess.FileExists(StringExtensions.PathJoin($"{SaveRoot}/{dir}", "PlayerData.tres")))
			.OrderByDescending(dir => FileAccess.GetModifiedTime(StringExtensions.PathJoin($"{SaveRoot}/{dir}", "PlayerData.tres")))
			.FirstOrDefault();

		ShellCore.Say($"Loading save from: {latestFolder}");
		// Load global data
		int[][] statusCodes = [
			// Wrap singletons in arrays to match the expected type
			[PlayerDataManager.LoadPlayerData(StringExtensions.PathJoin($"{SaveRoot}/{latestFolder}", "PlayerData.tres"))],
			[PlayerFileManager.LoadFileSysData(StringExtensions.PathJoin($"{SaveRoot}/{latestFolder}", "FileSys"))],
			NetworkManager.LoadNetworkData(StringExtensions.PathJoin($"{SaveRoot}/{latestFolder}", "NetworkData"))
		];
		// Wipe the slate clean
		lifeCycleDirector.RemakeScene();

		ShellCore.Say("Quick loading save...");
		ShellCore.Say("-n", PlayerDataManager.GetLoadStatusMsg(statusCodes[0][0]) + "... ");
		ShellCore.Say("-n", PlayerFileManager.GetLoadStatusMsg(statusCodes[1][0]) + "... ");
        // The two above use int[][] to match the expected type, but this one is a single int
        ShellCore.Say("-n", NetworkManager.GetLoadStatusMsg(statusCodes[2]) + "... ");
		ShellCore.Say($"\nIf there are any error related to save file, feel free to email {Util.Format("ajingixtascontact", StrType.USERNAME)}@{Util.Format("gmail.com", StrType.HOSTNAME)}");
		CurrentSavePath = StringExtensions.PathJoin(SaveRoot, latestFolder); // Update current save path
	}
	void RemakeScene() {
		if (runtimeDirector != null) RemoveChild(runtimeDirector);
		runtimeDirector = gameScene.Instantiate<RuntimeDirector>();
		AddChild(runtimeDirector);
		ShellCore.IntializeInternal();
	}
	public void OnAutosaveTimerTimeout() {
		QuickSave(false);
	}
}
