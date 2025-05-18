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


    // Entry point to import a real directory tree into a virtual one
    public static NodeDirectory ImportFromDisk(string realPath) {
        if (!Directory.Exists(realPath))
            throw new DirectoryNotFoundException($"Path not found: {realPath}");
        NodeDirectory root = new("~");
        ReadDirectoryRecursive(realPath, root);
        return root;
    }
    // Recursive function to populate virtual FS
    private static void ReadDirectoryRecursive(string currentPath, NodeDirectory virtualDir) {
        foreach (string dir in Directory.GetDirectories(currentPath)) {
            string dirName = Path.GetFileName(dir);
            NodeDirectory childDir = new NodeDirectory(dirName);
            virtualDir.Add(childDir);
            ReadDirectoryRecursive(dir, childDir);
        }

        foreach (string file in Directory.GetFiles(currentPath)) {
            string fileName = Path.GetFileName(file);
            string content = File.ReadAllText(file);
            NodeFile virtualFile = new NodeFile(fileName, content);
            virtualDir.Add(virtualFile);
        }
    }
}
