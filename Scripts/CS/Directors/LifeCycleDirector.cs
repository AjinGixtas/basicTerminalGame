using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class LifeCycleDirector : Node
{
	[Export] PackedScene gameScene;
	public static RuntimeDirector runtimeDirector;
	public static event Action FinishScene;
	public override void _Ready() {
		// Load the game scene
		if (gameScene != null) {
			// Intialize default state
            RemakeScene();
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
		string newSavePath = newSave ? StringExtensions.PathJoin(SaveRoot, $"Game_{nextSaveNumber:D5}") : CurrentSavePath;

		// Create new save directory
		DirAccess.MakeDirAbsolute(ProjectSettings.GlobalizePath(newSavePath));
        
		// Save player data (convert back to user:// path for Godot API)
        int[][] saveStatus = [
            [PlayerDataManager.SavePlayerData(StringExtensions.PathJoin(newSavePath, "PlayerData.tres"))],
            [PlayerFileManager.SaveFileSysData(StringExtensions.PathJoin(newSavePath, "FileSys"))],
            NetworkManager.SaveNetworkData(StringExtensions.PathJoin(newSavePath, "NetworkData")),
			[ItemCrafter.SaveItemCrafterData(StringExtensions.PathJoin(newSavePath, "ItemCrafter.tres"))],
        ];
        if (saveStatus.All(row => row.All(cell => cell == 0))) {
            if (newSave) runtimeDirector.MakeNotification($"Saved to {newSavePath}");
            else runtimeDirector.MakeNotification($"Overwrite to {newSavePath}");
        } else {
			runtimeDirector.MakeNotification("Save problem found. Check terminal output.");
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
		int[][] loadStatus = [
			// Wrap singletons in arrays to match the expected type
			[PlayerDataManager.LoadPlayerData(StringExtensions.PathJoin($"{SaveRoot}/{latestFolder}", "PlayerData.tres"))],
			[PlayerFileManager.LoadFileSysData(StringExtensions.PathJoin($"{SaveRoot}/{latestFolder}", "FileSys"))],
			NetworkManager.LoadNetworkData(StringExtensions.PathJoin($"{SaveRoot}/{latestFolder}", "NetworkData")),
			[ItemCrafter.LoadItemCrafterData(StringExtensions.PathJoin($"{SaveRoot}/{latestFolder}", "ItemCrafter.tres"))],
		];
        ShellCore.Say("-n", PlayerDataManager.GetLoadStatusMsg(loadStatus[0][0]) + "... ");
		ShellCore.Say("-n", PlayerFileManager.GetLoadStatusMsg(loadStatus[1][0]) + "... ");
		ShellCore.Say("-n", NetworkManager.GetLoadStatusMsg(loadStatus[2]));
        ShellCore.Say($"\nIf error are unexpected. Email {Util.Format("ajingixtascontact", StrSty.USERNAME)}@{Util.Format("gmail.com", StrSty.HOSTNAME)}");
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
