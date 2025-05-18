using System.Collections.Generic;
using System;

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
    public List<NodeSystemItem> Childrens { get; } = new List<NodeSystemItem>();

    public NodeDirectory(string name) : base(name) { }
    // 0-Success; 1-None found
    public int RemoveFile(string name) {
        foreach (NodeSystemItem item in Childrens) {
            if (item.GetType() == typeof(NodeFile) && item.Name == name) {
                Childrens.Remove(item);
                return 0;
            }
        }
        return 1;
    }
    // 0-Success; 1-None found
    public int RemoveDir(string name) {
        foreach (NodeSystemItem item in Childrens) {
            if (item.GetType() == typeof(NodeDirectory) && item.Name == name) {
                Childrens.Remove(item);
                return 0;
            }
        }
        return 1;
    }
    // 0-Success; 1-Duplicate name found; 2-Attempted to add parent
    public int Add(NodeSystemItem item) {
        if (item == Parent) return 2;
        foreach (NodeSystemItem children in Childrens) {
            if (item.Name == children.Name) { return 1; }
        }
        item.Parent = this;
        Childrens.Add(item);
        return 0;
    }
    // 0-Success; 1-Duplicate name found
    public int AddFile(string name) {
        foreach (NodeSystemItem children in Childrens) {
            if (children.GetType() == typeof(NodeFile) && name == children.Name) { return 1; }
        }
        NodeFile file = new(name) { Parent = this };
        Childrens.Add(file);
        return 0;
    }
    public int AddDir(string name) {
        foreach (NodeSystemItem children in Childrens) {
            if (children.GetType() == typeof(NodeDirectory) && name == children.Name) { return 1; }
        }
        NodeDirectory dir = new(name) { Parent = this };
        Childrens.Add(dir);
        return 0;
    }
    // TerminalDirectory-Directory found; null-Directory not found
    public NodeDirectory GetDirectory(string name) {
        string[] components = name.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        NodeDirectory curDir = this;
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
    public NodeFile GetFile(string name) {
        string[] components = name.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        NodeDirectory curDir = this;
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
}