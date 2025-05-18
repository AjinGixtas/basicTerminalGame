using Godot;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public partial class LifeCycleDirector : Node
{
	[Export] PackedScene gameScene;
	RuntimeDirector runtimeDirector;
	public override void _Ready() {
		// Load the game scene
		if (gameScene != null) {
			runtimeDirector = gameScene.Instantiate<RuntimeDirector>();
			AddChild(runtimeDirector);
		} else {
			GD.PrintErr("Game scene not set in LifeCycleDirector.");
		}
	}
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("quickSaveGame")) { QuickSave(); }
        if (Input.IsActionJustPressed("quickLoadGame")) { 
            QuickLoad();
            RemakeScene();
        }
	}
    private const string SaveRoot = "user://Saves";
    static void QuickSave() {
        // Convert virtual path to real path
        string realSaveRoot = ProjectSettings.GlobalizePath(SaveRoot);

        // Ensure the save root directory exists
        Directory.CreateDirectory(realSaveRoot);

        // Find existing save folders named like "Game_###"
        var existingSaves = Directory
            .GetDirectories(realSaveRoot)
            .Select(dir => {
                var match = Regex.Match(dir, @"Game_(\d+)$");
                return match.Success ? int.Parse(match.Groups[1].Value) : 0;
            })
            .Where(num => num > 0)
            .ToList();

        // Pick next save number
        int nextSaveNumber = existingSaves.Count == 0 ? 1 : existingSaves.Max() + 1;
        string newSaveFolderName = $"Game_{nextSaveNumber:D3}";
        string newSaveFolderPath = Path.Combine(realSaveRoot, newSaveFolderName);

        // Create new save directory
        Directory.CreateDirectory(newSaveFolderPath);

        // Path to save file
        string saveFilePath = Path.Combine(newSaveFolderPath, "PlayerData.tres");

        // Save player data (convert back to user:// path for Godot API)
        string userSaveFilePath = ProjectSettings.LocalizePath(saveFilePath);

        PlayerData.SavePlayerData(userSaveFilePath);

        GD.Print($"Quick saved to {userSaveFilePath}");
    }
	static void QuickLoad() {
        string fullPath = ProjectSettings.GlobalizePath(SaveRoot);  // Convert Godot path to system path
        if (!Directory.Exists(fullPath)) { Directory.CreateDirectory(fullPath); }
        // Find all PlayerData.tres files in subdirectories
        var saveFiles = Directory.GetDirectories(fullPath).Select(dir => Path.Combine(dir, "PlayerData.tres")).Where(File.Exists).ToList();

        if (saveFiles.Count == 0) {
            GD.Print("No save files found.");
            return;
        }

        // Pick the one with the latest write time
        string latestSave = saveFiles.OrderByDescending(File.GetLastWriteTime).First();

        GD.Print("Loading save from: " + latestSave);
        PlayerData.LoadPlayerData(ProjectSettings.LocalizePath(latestSave));
    }
    void RemakeScene() {
        RemoveChild(runtimeDirector);
        runtimeDirector = gameScene.Instantiate<RuntimeDirector>();
        AddChild(runtimeDirector);
    }
}
