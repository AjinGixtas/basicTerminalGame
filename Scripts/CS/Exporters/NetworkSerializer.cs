using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class NetworkSerializer
{
    // Save the network structure and file system to a JSON file
    public static void SaveNetwork(NetworkNode rootNode, string filePath)
    {
        var networkData = SerializeNode(rootNode);
        string json = JsonSerializer.Serialize(networkData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    // Load the network structure and file system from a JSON file
    public static NetworkNode LoadNetwork(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var networkData = JsonSerializer.Deserialize<SerializedNode>(json);
        return DeserializeNode(networkData, null);
    }

    // Helper method to serialize a node and its file system
    private static SerializedNode SerializeNode(NetworkNode node)
    {
        return new SerializedNode
        {
            HostName = node.HostName,
            DisplayName = node.DisplayName,
            IP = node.IP,
            NodeType = node.NodeType,
            SecLvl = node.SecLvl,
            DefLvl = node.DefLvl,
            Directory = SerializeDirectory(node.NodeDirectory),
            ChildNodes = node.ChildNode.ConvertAll(SerializeNode)
        };
    }

    // Helper method to serialize a directory and its contents
    private static SerializedDirectory SerializeDirectory(NodeDirectory directory)
    {
        var serializedDirectory = new SerializedDirectory
        {
            Name = directory.Name,
            Files = new List<SerializedFile>(),
            SubDirectories = new List<SerializedDirectory>()
        };

        foreach (var item in directory.Childrens)
        {
            if (item is NodeFile file)
            {
                serializedDirectory.Files.Add(new SerializedFile { Name = file.Name, Content = file.Content });
            }
            else if (item is NodeDirectory subDirectory)
            {
                serializedDirectory.SubDirectories.Add(SerializeDirectory(subDirectory));
            }
        }

        return serializedDirectory;
    }

    // Helper method to deserialize a node and its file system
    private static NetworkNode DeserializeNode(SerializedNode serializedNode, NetworkNode parent)
    {
        var node = NetworkNode.GenerateProceduralNode(
            serializedNode.NodeType,
            serializedNode.HostName,
            serializedNode.DisplayName,
            0, 0, // Index and depth ratios are not serialized
            parent
        );

        node.SecLvl = serializedNode.SecLvl;
        node.DefLvl = serializedNode.DefLvl;

        node.NodeDirectory.Childrens.Clear();
        DeserializeDirectory(serializedNode.Directory, node.NodeDirectory);

        foreach (var child in serializedNode.ChildNodes)
        {
            var childNode = DeserializeNode(child, node);
            node.ChildNode.Add(childNode);
        }

        return node;
    }

    // Helper method to deserialize a directory and its contents
    private static void DeserializeDirectory(SerializedDirectory serializedDirectory, NodeDirectory directory)
    {
        foreach (var file in serializedDirectory.Files)
        {
            directory.Add(new NodeFile(file.Name, file.Content));
        }

        foreach (var subDirectory in serializedDirectory.SubDirectories)
        {
            var newDirectory = new NodeDirectory(subDirectory.Name);
            directory.Add(newDirectory);
            DeserializeDirectory(subDirectory, newDirectory);
        }
    }

    // Data structure for serialized nodes
    private class SerializedNode
    {
        public string HostName { get; set; }
        public string DisplayName { get; set; }
        public string IP { get; set; }
        public NetworkNodeType NodeType { get; set; }
        public int SecLvl { get; set; }
        public int DefLvl { get; set; }
        public SerializedDirectory Directory { get; set; }
        public List<SerializedNode> ChildNodes { get; set; }
    }

    // Data structure for serialized directories
    private class SerializedDirectory
    {
        public string Name { get; set; }
        public List<SerializedFile> Files { get; set; }
        public List<SerializedDirectory> SubDirectories { get; set; }
    }

    // Data structure for serialized files
    private class SerializedFile
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
