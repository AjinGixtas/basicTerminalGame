using Godot;
using System.Collections.Generic;

public abstract class NodeSystemItem {
    public string Name { get; set; }
    public NodeDirectory Parent { get; set; }
    public NodeSystemItem(string name) {
        Name = name;
    }
    /// <summary>
    /// Get the full path of this item in the file system.
    /// </summary>
    /// <returns></returns>
    public string GetPath() {
        if (Parent == null) return $"{Name}/";
        return $"{Parent.GetPath()}{Name}/";
    }
}
public class NodeFile : NodeSystemItem {
    public string Content { get; set; }
    public NodeFile(string name, string content = "") : base(name) {
        Content = content;
    }
}
public class NodeDirectory : NodeSystemItem {
    /// <summary>
    /// List of generic child items in this directory. Includes both files and directories.
    /// </summary>
    public List<NodeSystemItem> Childrens { get; } = [];

    public NodeDirectory(string name) : base(name) { }
    /// <summary>
    /// Remove file by its path name.
    /// </summary>
    /// <param name="pathName"></param>
    /// <returns>
    /// <c>CError.OK</c>: File removed successfully.<br/>
    /// <c>CError.NOT_FOUND</c>: File not found.<br/>
    /// <c>CError.INVALID</c>: Invalid operation, e.g. trying to remove a file that has no parent.<br/>
    /// <c>CError.UNKNOWN</c>: Unknown error, e.g. file not removed from parent.
    /// </returns>
    public CError RemoveFile(string pathName) {
        NodeFile file = GetFile(pathName);
        if (file == null) return CError.NOT_FOUND;
        NodeDirectory nodeDirectory = file.Parent;
        if (nodeDirectory == null) return CError.INVALID; // File has no parent, which is invalid.
        if (nodeDirectory.Childrens.Remove(file)) return CError.OK;
        return CError.UNKNOWN;
    }
    /// <summary>
    /// Remove directory by its path name.
    /// </summary>
    /// <param name="pathName"></param>
    /// <returns>
    /// <c>CError.OK</c>: Directory removed successfully.<br/>
    /// <c>CError.NOT_FOUND</c>: Directory not found.<br/>
    /// <c>CError.INVALID</c>: Invalid operation, e.g. trying to remove a directory that has no parent.<br/>
    /// <c>CError.UNKNOWN</c>: Unknown error, e.g. directory not removed from parent.
    /// </returns>
    public CError RemoveDir(string pathName) {
        NodeDirectory dir = GetDir(pathName);
        if (dir == null) return CError.NOT_FOUND;
        NodeDirectory parentDir = dir.Parent;
        if (parentDir == null) return CError.INVALID; // Directory has no parent, which is invalid.
        if (parentDir.Childrens.Remove(dir)) return CError.OK;
        return CError.UNKNOWN;
    }
    /// <summary>
    /// Add a file by its path name.
    /// </summary>
    /// <param name="pathName">File path, must have existed.</param>
    /// <returns>
    /// <c>CError.OK</c>: File added successfully.<br/>
    /// <c>CError.NOT_FOUND</c>: Directory not found.<br/>
    /// <c>CError.DUPLICATE</c>: File with the same name already exists in the directory.<br/>
    /// <c>CError.INVALID</c>: Invalid operation, e.g. trying to add a file to itself.
    /// </returns>
    public CError AddFile(string pathName) {
        string path = System.IO.Path.GetDirectoryName(pathName).Replace('\\', '/'), name = System.IO.Path.GetFileName(pathName).Replace('\\', '/');
        NodeDirectory dir = GetDir(path);
        if (dir == null) return CError.NOT_FOUND;
        NodeFile file = new(name) { Parent = dir };
        return dir.Add(file);
    }
    /// <summary>
    /// Add a directory by its path name.
    /// </summary>
    /// <param name="pathName">File path, must have existed.</param>
    /// <returns>
    /// <c>CError.OK</c>: File added successfully.<br/>
    /// <c>CError.NOT_FOUND</c>: Directory not found.<br/>
    /// <c>CError.DUPLICATE</c>: File with the same name already exists in the directory.<br/>
    /// <c>CError.INVALID</c>: Invalid operation, e.g. trying to add a file to itself.
    /// </returns>
    public CError AddDir(string pathName) {
        string path = System.IO.Path.GetDirectoryName(pathName).Replace('\\', '/'), name = System.IO.Path.GetFileName(pathName).Replace('\\', '/');
        NodeDirectory dir = GetDir(path);
        if (dir == null) return CError.NOT_FOUND;
        NodeDirectory newDir = new(name) { Parent = dir };
        return dir.Add(newDir);
    }
    /// <summary>
    /// Add a NodeSystemItem to this directory.
    /// </summary>
    /// <param name="item">This object will be checks to prevent self-referencing.</param>
    /// <returns>
    /// <c>CError.OK</c>: File added successfully.<br/>
    /// <c>CError.DUPLICATE</c>: File with the same name already exists in the directory.<br/>
    /// <c>CError.INVALID</c>: Invalid operation, e.g. trying to add a file to itself.
    /// </returns>
    public CError Add(NodeSystemItem item) {
        if (item == Parent) return CError.INVALID;
        foreach (NodeSystemItem children in Childrens) {
            if (item.Name == children.Name) { return CError.DUPLICATE; }
        }
        item.Parent = this;
        Childrens.Add(item);
        return CError.OK;
    }
    /// <summary>
    /// Get a directory object by its path name.
    /// </summary>
    /// <param name="pathName">Note: Path must exists</param>
    /// <returns>Resulting object</returns>
    public NodeDirectory GetDir(string pathName) {
        if (string.IsNullOrWhiteSpace(pathName) || pathName == "") return this; // Empty path returns current directory.
        string[] components = StringExtensions.Split(pathName, "/", false);
        NodeDirectory curDir = pathName.StartsWith('/') ? GetRoot() : this;
        foreach (string component in components) {
            if (component == ".") continue; // Current directory, do nothing.
            if (component == "..") {
                if (curDir.Parent == null) return null; // Root dir is characterized by a lack of parent.
                curDir = curDir.Parent;
                continue;
            }
            curDir = (NodeDirectory)(curDir?.Childrens.Find(item => (item.GetType() == typeof(NodeDirectory) && item.Name == component)));
            if (curDir == null || curDir.Name != component) return null;
            continue;
        }
        return curDir;
    }
    /// <summary>
    /// Get a file object by its path name.
    /// </summary>
    /// <param name="pathName">Note: Path must exists</param>
    /// <returns>Resulting object</returns>
    public NodeFile GetFile(string pathName) {
        string[] components = StringExtensions.Split(pathName, "/", false);
        NodeDirectory curDir = pathName.StartsWith('/') ? GetRoot() : this;
        string[] pathComponents = components[..^1]; string fileName = components[^1];
        foreach (string component in pathComponents) {
            if (component == "..") {
                if (curDir.Parent == null) return null; // Root dir is characterized by a lack of parent.
                curDir = curDir.Parent;
                continue;
            }
            curDir = (NodeDirectory)(curDir?.Childrens.Find(item => (item.GetType() == typeof(NodeDirectory) && item.Name == component)));
            if (curDir == null || curDir.Name != component) return null;
            continue;
        }
        return (NodeFile)(curDir?.Childrens.Find(item => (item.GetType() == typeof(NodeFile) && item.Name == fileName)));
    }
    /// <summary>
    /// Get the root directory of this directory tree.
    /// </summary>
    /// <returns>Resulting object</returns>
    public NodeDirectory GetRoot() {
        NodeDirectory current = this;
        while (current.Parent != null) {
            current = current.Parent;
        }
        return current;
    }
}