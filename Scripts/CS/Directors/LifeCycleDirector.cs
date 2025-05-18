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
            QuickLoad(this);
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
        FileSystemSerializer.ExportToDisk(TerminalProcessor.NetworkManager.playerNode.nodeDirectory, Path.Combine(newSaveFolderPath, "FileSys")); // Export the virtual file system to disk
        GD.Print($"Quick saved to {userSaveFilePath}");
    }
    static void QuickLoad(LifeCycleDirector lifeCycleDirector) {
        string fullPath = ProjectSettings.GlobalizePath(SaveRoot);  // Convert Godot path to absolute system path
        if (!Directory.Exists(fullPath)) { GD.Print("Save directory does not exist."); return; }

        // Find all save folders named like "Game_###"
        var saveFolders = Directory.GetDirectories(fullPath).Where(dir => Regex.IsMatch(dir, @"Game_\d{3}$")).ToList();

        if (saveFolders.Count == 0) { GD.Print("No save files found."); return; }

        // Find the most recently updated one
        string latestFolder = saveFolders.OrderByDescending(dir => Directory.GetLastWriteTime(Path.Combine(dir, "PlayerData.tres"))).First();

        string playerDataPath = Path.Combine(latestFolder, "PlayerData.tres");
        string fileSystemPath = Path.Combine(latestFolder, "FileSys");

        if (!File.Exists(playerDataPath)) {GD.Print("PlayerData.tres not found in latest save."); return; }
        
        GD.Print($"Loading save from: {playerDataPath}");
        // Load global data
        PlayerData.LoadPlayerData(ProjectSettings.LocalizePath(playerDataPath));

        lifeCycleDirector.RemakeScene();
        // Load instance data
        if (Directory.Exists(fileSystemPath)) {
            TerminalProcessor.NetworkManager.playerNode.nodeDirectory = FileSystemSerializer.ImportFromDisk(fileSystemPath);
            TerminalProcessor.CurrDir = TerminalProcessor.NetworkManager.playerNode.nodeDirectory;
            GD.Print(string.Join(" ", TerminalProcessor.CurrDir.Childrens.Select(x => x.Name)));
            GD.Print("Virtual file system restored.");
        } else {
            GD.Print("No virtual file system found in save.");
        }
    }
    void RemakeScene() {
        RemoveChild(runtimeDirector);
        runtimeDirector = gameScene.Instantiate<RuntimeDirector>();
        AddChild(runtimeDirector);
    }
}
