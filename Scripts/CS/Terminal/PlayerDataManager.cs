using Godot;

public static partial class PlayerDataManager {
	static PlayerDataSaveResource saveObj;
	public static double[] MineInv => _mineInv;
	public static double GC_Cur => _gc_cur;
	public static double GC_Max => _gc_max;
	public static bool CompletedTutorial {
		get => saveObj.CompletedTutorial;
		set => saveObj.CompletedTutorial = value;
	}
	public static int tutorialProgress = 0;
	public static string Username {
		get => _username;
		set => _username = value;
	}
	static double _gc_cur {
		get => saveObj.GC_cur;
		set {
			saveObj.GC_cur = Mathf.Clamp(value, 0, GC_Max);
		}
	}
	static double _gc_max {
		get => saveObj.GC_max;
		set {
			saveObj.GC_max = Mathf.Max(value, 0);
		}
	}
	static string _username {
		get => saveObj.username;
		set { saveObj.username = value; }
	}
	static double[] _mineInv {
		get => saveObj.MineralInventory;
		set {
			if (value.Length != saveObj.MineralInventory.Length) {
				GD.PushError("Invalid mineral inventory length.");
				return;
			}
			for (int i = 0; i < value.Length; i++) {
				if (value[i] < 0) {
					GD.PushError($"Invalid mineral amount for type {i}: {value[i]}");
					return;
				}
			}
			saveObj.MineralInventory = value;
		}
	}

	public static void Ready() {
		saveObj = new();
	}

	public static int WithdrawGC(double amount) { // 0-Successful withdraw; 1-Invalid amount; 2-Not enough money
		if (amount < 0) { return 1; }
		if (amount > _gc_max) { return 2; }
		_gc_cur -= amount;
		return 0;
	}
	static bool needWarn = false, warned = false;
	public static int DepositGC(double amount) { // 0-Successful deposit; 1-Invalid amount
		if (amount <= 0) { return 1; }
		_gc_cur += amount;

		if (_gc_cur <= GC_Max) { needWarn = false; warned = false; } else needWarn = true;
		if (needWarn && !warned) {
			ShellCore.Say($"[color={Util.CC(Cc.gR)}]Warning:[/color] GC total is over the limit of {_gc_max}. Remaining GC lost.");
			warned = true;
		}
		return 0;
	}

	public static int WithdrawMineral(double[] amounts) {
		if (amounts.Length != _mineInv.Length) return 1; // Invalid amount
		for (int i = 0; i < amounts.Length; ++i) {
			if (amounts[i] < 0 || amounts[i] > _mineInv[i]) return 2; // Invalid amount
		}
		for (int i = 0; i < amounts.Length; ++i) {
			_mineInv[i] -= amounts[i];
		}   
		return 0;
	}
	public static int DepositMineral(double[] amounts) {
		if (amounts.Length != _mineInv.Length) return 1; // Invalid amount
		for (int i = 0; i < amounts.Length; ++i) {
			if (amounts[i] < 0) return 2; // Invalid amount
		}
		for (int i = 0; i < amounts.Length; ++i) {
			_mineInv[i] += amounts[i];
		}
		return 0;
	}
	public static int DepositMineral(int type, double amount) {
		if (type < 0 || type >= _mineInv.Length) return 1; // Invalid type
		if (amount < 0) return 2; // Invalid amount
		_mineInv[type] += amount;
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
	: Util.Format($"{((CError)statusCode)}", StrType.UNKNOWN_ERROR, "saving player data");
	}
	
	public static int LoadPlayerData(string filePath) {
		saveObj = new();
		needWarn = false; warned = false;
		if (!FileAccess.FileExists(filePath)) { return 1; }
		PlayerDataSaveResource data;
		try { data = GD.Load<PlayerDataSaveResource>(filePath); } catch { return 2; }
		if (data == null) { return 3; }
		saveObj = data;
		return 0;
	}
	public static int SavePlayerData(string filePath) {
		PlayerDataSaveResource data = saveObj;
		Error error = ResourceSaver.Save(data, filePath);
		return (int)error;
	}
}
