using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class LifeCycleDirector : Node
{
	[Export] PackedScene gameScene;
	RuntimeDirector runtimeDirector;
	public static event Action FinishScene;
	public override void _Ready() {
		// Load the game scene
		if (gameScene != null) {
			// Intialize default state
			PlayerDataManager.Setup();
            PlayerFileManager.Setup();
            NetworkManager.Setup();
            QuickLoad(this);
            ShellCore.Ready();
            NetworkManager.Ready();
            RemakeScene();
            FinishScene?.Invoke();
			foreach (Delegate d in FinishScene.GetInvocationList()) FinishScene -= (Action)d; // Clear the event to prevent multiple invocations
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
		int nextSaveNumber = existingSaves.Count == 0 ? 1 : existingSaves.Max() + 1;
		string newSaveFolderName = newSave ? $"Game_{nextSaveNumber:D5}" : CurrentSavePath;
		string newSavePath = StringExtensions.PathJoin(SaveRoot, newSaveFolderName);

		// Create new save directory
		DirAccess.MakeDirAbsolute(ProjectSettings.GlobalizePath(newSavePath));
        
		// Save player data (convert back to user:// path for Godot API)
        int[][] saveStatus = [
            [PlayerDataManager.SavePlayerData(StringExtensions.PathJoin(newSavePath, "PlayerData.tres"))],
            [PlayerFileManager.SaveFileSysData(StringExtensions.PathJoin(newSavePath, "FileSys"))],
            NetworkManager.SaveNetworkData(StringExtensions.PathJoin(newSavePath, "NetworkData"))
        ];
		if (saveStatus.All(row => row.All(cell => cell == 0))) {
            if (newSave) ShellCore.Say($"Saved game to {newSavePath}");
            else ShellCore.Say($"Overwrote save to {newSavePath}");
        } else {
			ShellCore.Say("-n", PlayerDataManager.GetSaveStatusMsg(saveStatus[0][0]) + "... ");
			ShellCore.Say("-n", PlayerFileManager.GetSaveStatusMsg(saveStatus[1][0]) + "... ");
			ShellCore.Say("-n", NetworkManager.GetSaveStatusMsg(saveStatus[2]));
			ShellCore.Say($"\nIf error are unexpected. Email {Util.Format("ajingixtascontact", StrSty.USERNAME)}@{Util.Format("gmail.com", StrSty.HOSTNAME)}");
        }
        CurrentSavePath = newSavePath; // Update current save path
	}

	static void QuickLoad(LifeCycleDirector lifeCycleDirector) {
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

		ShellCore.Say("Quick loading save...");
		ShellCore.Say("-n", PlayerDataManager.GetLoadStatusMsg(statusCodes[0][0]) + "... ");
		ShellCore.Say("-n", PlayerFileManager.GetLoadStatusMsg(statusCodes[1][0]) + "... ");
        // The two above use int[][] to match the expected type, but this one is a single int
        ShellCore.Say("-n", NetworkManager.GetLoadStatusMsg(statusCodes[2]) + "... ");
		ShellCore.Say($"\nIf there are any error related to save file, feel free to email {Util.Format("ajingixtascontact", StrSty.USERNAME)}@{Util.Format("gmail.com", StrSty.HOSTNAME)}");

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
