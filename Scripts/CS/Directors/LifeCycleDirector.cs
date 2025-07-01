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
			LoadGame(true);
		} else { GD.PrintErr("Game scene not set in LifeCycleDirector."); }
    }
    public override void _Process(double delta) {
		if (Input.IsActionJustPressed("sequentialSave")) { 
			QuickSave(true);
		}
		if (Input.IsActionJustPressed("quickLoadGame")) {
			LoadGame(true);
        }
		if (Input.IsActionJustPressed("overwriteSave")) {
			QuickSave(false);
		}
    }
	void LoadGame(bool quickLoad) {
		RemakeScene();
		PlayerDataManager.Setup();
		PlayerFileManager.Setup();
		NetworkManager.Setup();
		if (quickLoad) QuickLoad(this);
		ShellCore.Ready();
		NetworkManager.Ready();
		RemakeScene();
		FinishScene?.Invoke();
		foreach (Delegate d in FinishScene.GetInvocationList()) FinishScene -= (Action)d;
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
		// 0 - Success, Pos - Error, Neg - Warning
        int[][] saveStatus = [
            [PlayerDataManager.SavePlayerData(StringExtensions.PathJoin(newSavePath, "PlayerData.tres"))],
            [PlayerFileManager.SaveFileSysData(StringExtensions.PathJoin(newSavePath, "FileSys"))],
            NetworkManager.SaveNetworkData(StringExtensions.PathJoin(newSavePath, "NetworkData")),
			[ItemCrafter.SaveItemCrafterData(StringExtensions.PathJoin(newSavePath, "ItemCrafter.tres"))],
        ];
        if (saveStatus.All(row => row.All(cell => cell <= 0))) {
            if (newSave) RuntimeDirector.MakeNotification($"Saved to {newSavePath}");
            else RuntimeDirector.MakeNotification($"Overwrite to {newSavePath}");
        } else {
            RuntimeDirector.MakeNotification("Save problem found. Check terminal output.");
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
	readonly string DOC = @$"
{Util.Format("ls <dir>", StrSty.CMD_FUL)} - List item in directories.
	{Util.Format("<dir>", StrSty.CMD_FARG)} - Target directory.

{Util.Format("cd <dir>", StrSty.CMD_FUL)} - Change directory.
	{Util.Format("<dir>", StrSty.CMD_FARG)} - Target directory.

{Util.Format("pwd", StrSty.CMD_FUL)} - Output working directory.

{Util.Format("run <file>", StrSty.CMD_FUL)} - Run file as script.
	{Util.Format("<file>", StrSty.CMD_FARG)} - File name.

{Util.Format("read <file>", StrSty.CMD_FUL)} - Output file content.
	{Util.Format("<file>", StrSty.CMD_FARG)} - File name.

{Util.Format("say -[n|e|b] <msg>", StrSty.CMD_FUL)} - Output message to the screen.
	{Util.Format("-n", StrSty.CMD_FARG)} - Suppress trailing newline.
	{Util.Format("-e", StrSty.CMD_FARG)} - Enable escape sequences.
	{Util.Format("-b", StrSty.CMD_FARG)} - Escape BBCode.
	{Util.Format("<msg>", StrSty.CMD_FARG)} - Message to output.

{Util.Format("help", StrSty.CMD_FUL)} - Display help.

{Util.Format("home", StrSty.CMD_FUL)} - Go back to your node.

{Util.Format("scan -[v]", StrSty.CMD_FARG)} - Scan adjacent node.
	{Util.Format("-v", StrSty.CMD_FARG)} - Verbose output.

{Util.Format("connect <hostname|ip>", StrSty.CMD_FUL)} - Connect to node.
	{Util.Format("<hostname|ip>", StrSty.CMD_FARG)} - Node to connect.

{Util.Format("analyze [hostname|ip]", StrSty.CMD_FUL)} - Analyze node.
	{Util.Format("[hostname|ip]", StrSty.CMD_FARG)} - Node to analyze.

{Util.Format("sector -[c] -[l <sector_level>]", StrSty.CMD_FUL)} - List sectors.
	{Util.Format("-c --connected", StrSty.CMD_FARG)} - Connected sectors only.
	{Util.Format("-l, --level <sector_level>", StrSty.CMD_FARG)} - Sectors with this level only.

{Util.Format("link <sector_names>", StrSty.CMD_FUL)} - Link sectors.
	{Util.Format("<sector_names>", StrSty.CMD_FARG)} - Sectors to link.

{Util.Format("unlink <sector_names>", StrSty.CMD_FUL)} - Unlink sectors.
	{Util.Format("<sector_names>", StrSty.CMD_FARG)} - Sectors to unlink.

{Util.Format("stats", StrSty.CMD_FUL)} - Display player stats.

{Util.Format("xyzzy", StrSty.CMD_FUL)} - Nothing happens

{Util.Format("clear", StrSty.CMD_FUL)} - Clear terminal.

{Util.Format("mkf <files>", StrSty.CMD_FUL)} - Make files.
	{Util.Format("<files>", StrSty.CMD_FARG)} - File names.

{Util.Format("rmf <files>", StrSty.CMD_FUL)} - Remove files.
	{Util.Format("<files>", StrSty.CMD_FARG)} - File names.

{Util.Format("mkdir <dirs>", StrSty.CMD_FUL)} - Make directories.
	{Util.Format("<dir>", StrSty.CMD_FARG)} - Directory names.

{Util.Format("rmdir <dirs>", StrSty.CMD_FUL)} - Remove directores.
	{Util.Format("<dir>", StrSty.CMD_FARG)} - Directory names.

{Util.Format("karaxe --[help]", StrSty.CMD_FUL)} - Run karaxe.exe
	{Util.Format("--help", StrSty.CMD_FARG)} - Provide details.

{Util.Format("myneswarm --[help]", StrSty.CMD_FUL)} - Run myneswarm.exe
	{Util.Format("--help", StrSty.CMD_FARG)} - Provide details.

{Util.Format("bitrader --[help]", StrSty.CMD_FUL)} - Run bitrader.exe
	{Util.Format("--help", StrSty.CMD_FARG)} - Provide details.

{Util.Format("bitcrafter --[help]", StrSty.CMD_FUL)} - Run bitcrafter.exe
	{Util.Format("--help", StrSty.CMD_FARG)} - Provide details.

{Util.Format("setname [-u <new_username>] [-h <new_hostname>] [-d <new_displayname>", StrSty.CMD_FUL)} - Change names of your node.
	{Util.Format("[-u <new_username>]", StrSty.CMD_FARG)} - New username to change to.
	{Util.Format("[-h <new_hostname>]", StrSty.CMD_FARG)} - New hostname to change to.
	{Util.Format("[-d <new_displayname>]", StrSty.CMD_FARG)} - New displayname to change to.";
}
