using Godot;

public static class PlayerDataManager {
	static double GC_Max;
	static double _gc_total;
	static bool needWarn = false, warned = false;
	static double GC_Accountant {
		get => _gc_total;
		set {
			_gc_total = Mathf.Clamp(value, 0, GC_Max);
			if (_gc_total <= GC_Max) { needWarn = false; warned = false; } else needWarn = true;
			if (needWarn && !warned) {
				TerminalProcessor.Say($"[color={Util.CC(Cc.gR)}]Warning:[/color] GC total is over the limit of {GC_Max}. Remaining GC lost.");
				warned = true;
			}
		}
	}
	public static double GC_PublicDisplay {
		get { return GC_Accountant; }
	}
	static string _username;
	public static string Username { 
		get => _username; 
		set { _username = value; }
	}

	public static void Ready() {
		GC_Max = 2_000_000; GC_Accountant = 0; Username = "UN1NTIALiZED_USER";
    }

	public static int WithDraw(double amount) { // 0-Successful withdraw; 1-Invalid amount; 2-Not enough money
        if (amount < 0) { return 1; }
		if (amount > GC_Accountant) { return 2; }
		GC_Accountant -= amount;
		return 0;
	}
	public static int Deposit(double amount) { // 0-Successful deposit; 1-Invalid amount
        if (amount <= 0) { return 1; }
		GC_Accountant += amount;
		return 0;
	}

	public static string GetLoadStatusMsg(int statusCode) {
		string[] LOAD_STATUS_MSG = [
			Util.Format("Loaded player data successfully", StrType.FULL_SUCCESS),
			Util.Format("No player data file found in save. Fall back to new user setting.", StrType.ERROR),
			Util.Format("Unable to parse player data file. Fall back to new user setting. Check for potentional file malfunction.", StrType.ERROR),
			Util.Format("File parse successfully yet doesn't get registered. Fall back to new user setting. Check for potentional file malfunction.", StrType.ERROR)
		];
		return (statusCode < LOAD_STATUS_MSG.Length) ? LOAD_STATUS_MSG[statusCode]
			: Util.Format($"{statusCode}", StrType.UNKNOWN_ERROR, "loading player data");
	}
	public static string GetSaveStatusMsg(int statusCode) {
		string[] SAVE_STATUS_MSG = [
			Util.Format("Saved player data successfully", StrType.FULL_SUCCESS),
		];
		return (statusCode < SAVE_STATUS_MSG.Length) ? SAVE_STATUS_MSG[statusCode]
	: Util.Format($"{((Error)statusCode)}", StrType.UNKNOWN_ERROR, "saving player data");
	}
    
	public static int LoadPlayerData(string filePath) {
		GC_Max = 2_000_000; GC_Accountant = 0; Username = "UN1NTIALiZED_USER";
		needWarn = false; warned = false;
        if (!FileAccess.FileExists(filePath)) { return 1; }
		PlayerDataSaveResource data;
        try { data = GD.Load<PlayerDataSaveResource>(filePath); } catch { return 2; }
        if (data == null) { return 3; }
		GC_Max = data.GC_max;
        GC_Accountant = data.GC_total;
        Username = data.username;
        return 0;
    }
    public static int SavePlayerData(string filePath) {
        PlayerDataSaveResource data = new() { GC_max = GC_Max, GC_total = GC_Accountant, username = Username };
        Error error = ResourceSaver.Save(data, filePath);
		return (int)error;
	}
}
