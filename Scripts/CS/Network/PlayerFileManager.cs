using Godot;
using System;
using System.IO;

public class PlayerFileManager {
	public static string GetLoadStatusMsg(int statusCode) {
		string[] LOAD_STATUS_MSG = [
			Util.Format("Loaded virtual file system successfully", StrType.FULL_SUCCESS),
			Util.Format("No virtual file system found in save. Fall back to empty virtual disk", StrType.ERROR)
		];
		return (statusCode < LOAD_STATUS_MSG.Length) ? LOAD_STATUS_MSG[statusCode]
			: Util.Format(statusCode.ToString(), StrType.UNKNOWN_ERROR, "loading virtual file system");
	}
	public static string GetSaveStatusMsg(int statusCode) {
		string[] SAVE_STATUS_MSG = [
			Util.Format("Saved virtual file system successfully", StrType.FULL_SUCCESS)
		];
		return (statusCode < SAVE_STATUS_MSG.Length) ? SAVE_STATUS_MSG[statusCode] : 
			Util.Format(statusCode.ToString(), StrType.UNKNOWN_ERROR, "saving virtual file system"); }
	static NodeDirectory fileSystem;
	public static NodeDirectory FileSystem { get => fileSystem; }
	
	public static void Ready() {
		fileSystem = new("~");
	}
	
	public static int SaveFileSysData(string basePath) {
		if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
		WriteDirectoryRecursive(fileSystem, basePath);
		return 0;
	}
	public static int LoadFileSysData(string realPath) {
		NodeDirectory root = new("~");
		if (!Directory.Exists(realPath)) { return 1; }
		ReadDirectoryRecursive(root, realPath);
		fileSystem = root; return 0;
	}
	
	private static void WriteDirectoryRecursive(NodeDirectory virtualDir, string currentPath) {
		foreach (var item in virtualDir.Childrens) {
			string itemPath = Path.Combine(currentPath, item.Name);

			if (item is NodeDirectory childDir) {
				if (!Directory.Exists(itemPath))
					Directory.CreateDirectory(itemPath);

				WriteDirectoryRecursive(childDir, itemPath);
			} else if (item is NodeFile file) {
				File.WriteAllText(itemPath, file.Content ?? "");
			}
		}
	}
	private static void ReadDirectoryRecursive(NodeDirectory virtualDir, string currentPath) {
		foreach (string dir in Directory.GetDirectories(currentPath)) {
			string dirName = Path.GetFileName(dir);
			NodeDirectory childDir = new NodeDirectory(dirName);
			virtualDir.Add(childDir);
			ReadDirectoryRecursive(childDir, dir);
		}

		foreach (string file in Directory.GetFiles(currentPath)) {
			string fileName = Path.GetFileName(file);
			string content = File.ReadAllText(file);
			NodeFile virtualFile = new NodeFile(fileName, content);
			virtualDir.Add(virtualFile);
		}
	}
}
