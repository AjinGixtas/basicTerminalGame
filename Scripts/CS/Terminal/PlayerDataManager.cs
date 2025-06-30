using Godot;

public static partial class PlayerDataManager {
	static PlayerDataSaveResource saveObj;
	public static long[] MineInv => _mineInv;
	public static long GC_Cur => _gc_cur;
	public static long GC_Max => _gc_max;
	public static bool CompletedTutorial {
		get => saveObj.CompletedTutorial;
		set => saveObj.CompletedTutorial = value;
	}
	public static int tutorialProgress = 0;
	public static string Username {
		get => _username;
		set => _username = value;
	}
	static long _gc_cur {
		get => saveObj.GC_cur;
		set {
			saveObj.GC_cur = System.Math.Clamp(value, 0, GC_Max);
		}
	}
	static long _gc_max {
		get => saveObj.GC_max;
		set {
			saveObj.GC_max = System.Math.Max(value, 0);
		}
	}
	static string _username {
		get => saveObj.username;
		set { saveObj.username = value; }
	}
	static long[] _mineInv {
		get => saveObj.MineralInventory;
		set {
			if (value.Length != saveObj.MineralInventory.Length) {
				GD.PrintErr("Invalid mineral inventory length.");
				return;
			}
			for (int i = 0; i < value.Length; i++) {
				if (value[i] < 0) {
					GD.PrintErr($"Invalid mineral amount for type {i}: {value[i]}");
					return;
				}
			}
			saveObj.MineralInventory = value;
		}
	}

	public static void Setup() {
		saveObj = new();
	}

	public static CError WithdrawGC(long amount) {
		if (amount < 0) return CError.INVALID;
		if (amount > _gc_max) return CError.INSUFFICIENT;
		_gc_cur -= amount;
        ShellCore.Say($"[{Util.Format(Util.GetFnv1aTimeHash(), StrSty.DECOR)}] <{Util.Format("g01d-pouch", StrSty.AUTO_KWORD)}> Withdrew {Util.Format($"{amount}", StrSty.MONEY)}");
        return CError.OK;
	}
	static bool needWarn = false, warned = false;
	public static CError DepositGC(long amount) {
		if (amount <= 0) return CError.INVALID;
		_gc_cur += amount;

		if (_gc_cur <= GC_Max) { needWarn = false; warned = false; } else needWarn = true;
		if (needWarn) {
			if (!warned) ShellCore.Say(Util.Format($"[{Util.Format(Util.GetFnv1aTimeHash(), StrSty.DECOR)}] <{Util.Format("g01d-pouch", StrSty.AUTO_KWORD)}> GC total is over the limit of {GC_Max}. Remaining GC [color={Util.CC(Cc.R)}]LOST[/color].", StrSty.WARNING));
			warned = true;
		} else ShellCore.Say($"[{Util.Format(Util.GetFnv1aTimeHash(),StrSty.DECOR)}] <{Util.Format("g01d-pouch", StrSty.AUTO_KWORD)}> Deposited {Util.Format($"{amount}", StrSty.MONEY)}");
		return CError.OK;
	}
	public static CError WithdrawMineral(long[] amounts) {
		if (amounts.Length != _mineInv.Length) return CError.INVALID; // Invalid amount
		for (int i = 0; i < amounts.Length; ++i) {
			if (amounts[i] < 0 || amounts[i] > _mineInv[i]) return CError.INSUFFICIENT; // Invalid amount
		}
		for (int i = 0; i < amounts.Length; ++i) {
			_mineInv[i] -= amounts[i];
		}   
		return CError.OK;
	}
	public static CError WithdrawMineral(int type, long amount) {
		if (_mineInv.Length <= type) return CError.INVALID;
		if (amount < 0 || amount > _mineInv[type]) return CError.INSUFFICIENT;
		_mineInv[type] -= amount;
		return CError.OK;
	}
	public static long DepositMineral(long[] amounts) {
		if (amounts.Length != _mineInv.Length) return 1; // Invalid amount
        for (int i = 0; i < amounts.Length; ++i) {
			if (amounts[i] < 0) return 2; // Invalid amount
		}
		for (int i = 0; i < amounts.Length; ++i) {
			_mineInv[i] += amounts[i];
		}
		return 0;
	}
	public static long DepositMineral(int type, long amount) {
		if (type < 0 || type >= _mineInv.Length) return 1; // Invalid type
		if (amount < 0) return 2; // Invalid amount
        _mineInv[type] += amount;
		return 0;
	}

	public static string GetLoadStatusMsg(int statusCode) {
		string[] LOAD_STATUS_MSG = [
			Util.Format("Loaded player data", StrSty.FULL_SUCCESS),
			Util.Format("No player data file found in save. Fall back to new user setting.", StrSty.ERROR),
			Util.Format("Unable to parse player data file. Fall back to new user setting. Check for potentional file malfunction.", StrSty.ERROR),
			Util.Format("File parse successfully yet doesn't get registered. Fall back to new user setting. Check for potentional file malfunction.", StrSty.ERROR)
		];
		return (statusCode < LOAD_STATUS_MSG.Length) ? LOAD_STATUS_MSG[statusCode]
			: Util.Format($"{statusCode}", StrSty.UNKNOWN_ERROR, "loading player data");
	}
	public static string GetSaveStatusMsg(int statusCode) {
		string[] SAVE_STATUS_MSG = [
			Util.Format("Saved player data", StrSty.FULL_SUCCESS),
		];
		return (statusCode < SAVE_STATUS_MSG.Length) ? SAVE_STATUS_MSG[statusCode]
	: Util.Format($"{((CError)statusCode)}", StrSty.UNKNOWN_ERROR, "saving player data");
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
