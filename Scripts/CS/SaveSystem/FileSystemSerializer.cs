using Godot;
using System.IO;

public static class FileSystemSerializer {
    // Entry point to export the entire virtual FS to real disk
    public static void ExportToDisk(NodeDirectory root, string basePath) {
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);
        WriteDirectoryRecursive(root, basePath);
    }
    // Recursive function that creates directories and files
    private static void WriteDirectoryRecursive(NodeDirectory dir, string currentPath) {
        foreach (var item in dir.Childrens) {
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
}
