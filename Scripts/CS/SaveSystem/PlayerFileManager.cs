using Godot;

public class PlayerFileManager {
	public static string GetLoadStatusMsg(int statusCode) {
		string[] LOAD_STATUS_MSG = [
			Util.Format("Loaded virtual file system successfully", StrType.FULL_SUCCESS),
			Util.Format("No virtual file system found in save. Fall back to empty virtual disk", StrType.ERROR)
		];
		return (statusCode < LOAD_STATUS_MSG.Length) ? LOAD_STATUS_MSG[statusCode]
			: Util.Format($"{statusCode}", StrType.UNKNOWN_ERROR, "loading virtual file system");
	}
	public static string GetSaveStatusMsg(int statusCode) {
		string[] SAVE_STATUS_MSG = [
			Util.Format("Saved virtual file system successfully", StrType.FULL_SUCCESS)
		];
		return (statusCode < SAVE_STATUS_MSG.Length) ? SAVE_STATUS_MSG[statusCode] : 
			Util.Format($"{statusCode}", StrType.UNKNOWN_ERROR, "saving virtual file system"); }
	static NodeDirectory fileSystem;
	public static NodeDirectory FileSystem { get => fileSystem; }
	
	public static void Ready() {
		fileSystem = new("~");
	}
	
	public static int SaveFileSysData(string basePath) {
		basePath = ProjectSettings.GlobalizePath(basePath);
        if (!DirAccess.DirExistsAbsolute(basePath)) DirAccess.MakeDirAbsolute(basePath);
		WriteDirectoryRecursive(fileSystem, basePath);
		return 0;
	}
	public static int LoadFileSysData(string basePath) {
		NodeDirectory root = new("~");
        if (!DirAccess.DirExistsAbsolute(basePath)) { return 1; }
		ReadDirectoryRecursive(root, basePath);
		fileSystem = root; return 0;
	}
	
	private static void WriteDirectoryRecursive(NodeDirectory virtualDir, string currentPath) {
		foreach (var item in virtualDir.Childrens) {
			string itemPath = StringExtensions.PathJoin(currentPath, item.Name);

			if (item is NodeDirectory childDir) {
				if (!DirAccess.DirExistsAbsolute(itemPath))
					DirAccess.MakeDirAbsolute(itemPath);

				WriteDirectoryRecursive(childDir, itemPath);
			} else if (item is NodeFile file) {
				FileAccess.Open(itemPath, FileAccess.ModeFlags.Write).StoreString(file.Content ?? "");
			}
		}
	}
	private static void ReadDirectoryRecursive(NodeDirectory virtualDir, string currentPath) {
		foreach (string dir in DirAccess.GetDirectoriesAt(currentPath)) {
			NodeDirectory childDir = new(dir);
			virtualDir.Add(childDir);
			ReadDirectoryRecursive(childDir, StringExtensions.PathJoin(currentPath, dir));
		}

		foreach (string file in DirAccess.GetFilesAt(currentPath)) {
			string filePath = StringExtensions.PathJoin(currentPath, file);
			string content = FileAccess.Open(filePath, FileAccess.ModeFlags.Read).GetAsText(true);
			NodeFile virtualFile = new(file, content);
			virtualDir.Add(virtualFile);
		}
	}
}
