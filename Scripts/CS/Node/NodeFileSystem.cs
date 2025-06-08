using Godot;
using System.Collections.Generic;

public abstract class NodeSystemItem {
    public string Name { get; set; }
    public NodeDirectory Parent { get; set; }
    public NodeSystemItem(string name) {
        Name = name;
    }
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
    public List<NodeSystemItem> Childrens { get; } = [];

    public NodeDirectory(string name) : base(name) { }  
    public CError RemoveFile(string pathName) {
        NodeFile file = GetFile(pathName);
        if (file == null) return CError.NOT_FOUND;
        NodeDirectory nodeDirectory = file.Parent;
        if (nodeDirectory == null) return CError.INVALID; // File has no parent, which is invalid.
        if (nodeDirectory.Childrens.Remove(file)) return CError.OK;
        return CError.UNKNOWN;
    }
    public CError RemoveDir(string pathName) {
        NodeDirectory dir = GetDirectory(pathName);
        if (dir == null) return CError.NOT_FOUND;
        NodeDirectory parentDir = dir.Parent;
        if (parentDir == null) return CError.INVALID; // Directory has no parent, which is invalid.
        if (parentDir.Childrens.Remove(dir)) return CError.OK;
        return CError.UNKNOWN;
    }
    public CError AddFile(string pathName) {
        string path = System.IO.Path.GetDirectoryName(pathName), name = System.IO.Path.GetFileName(pathName);
        NodeDirectory dir = GetDirectory(pathName);
        if (dir == null) return CError.NOT_FOUND;
        NodeFile file = new(name) { Parent = dir };
        return dir.Add(file);
    }
    public CError AddDir(string pathName) {
        string path = System.IO.Path.GetDirectoryName(pathName), name = System.IO.Path.GetFileName(pathName);
        NodeDirectory dir = GetDirectory(pathName);
        if (dir == null) return CError.NOT_FOUND;
        NodeDirectory newDir = new(name) { Parent = dir };
        return dir.Add(newDir);
    }
    public CError Add(NodeSystemItem item) {
        if (item == Parent) return CError.INVALID;
        foreach (NodeSystemItem children in Childrens) {
            if (item.Name == children.Name) { return CError.DUPLICATE; }
        }
        item.Parent = this;
        Childrens.Add(item);
        return CError.OK;
    }
    
    public NodeDirectory GetDirectory(string pathName) {
        string[] components = StringExtensions.Split(pathName, "/", false);
        NodeDirectory curDir = pathName.StartsWith('/') ? GetRoot() : this;
        foreach (string component in components) {
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
    public NodeDirectory GetRoot() {
        NodeDirectory current = this;
        while (current.Parent != null) {
            current = current.Parent;
        }
        return current;
    }
}