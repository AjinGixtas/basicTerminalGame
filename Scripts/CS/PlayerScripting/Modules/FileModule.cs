using System;
using System.IO;
using System.Linq;

public class FileModule {
	public string[] ListFiles(string path = "") {
		NodeDirectory dir = TerminalProcessor.CurrDir.GetDirectory(path);
		if (dir == null) throw new DirectoryNotFoundException($"Directory '{path}' not found.");
		return dir.Childrens.Where(x => x is NodeFile).Select(x => x.Name).ToArray();
	}
	public int MkF(string path) => (int)TerminalProcessor.CurrDir.AddFile(path);
	public int RmF(string path) => (int)TerminalProcessor.CurrDir.RemoveFile(path);
    public bool ExistF(string path) => TerminalProcessor.CurrDir.GetFile(path) != null;
	public int MkDir(string path) => (int)TerminalProcessor.CurrDir.AddDir(path);
	public int RmDir(string path) => (int)TerminalProcessor.CurrDir.RemoveDir(path);
    public bool ExistDir(string path) => TerminalProcessor.CurrDir.GetDirectory(path) != null;
	
    public CError WriteF(string path, string content, WriteMode mode) {
        NodeFile file = TerminalProcessor.CurrDir.GetFile(path);
        if (file == null) return CError.NOT_FOUND;
		if (mode == WriteMode.Append) file.Content += content;
		else if (mode == WriteMode.Write) file.Content = content;
        return CError.OK;
    }
	public string ReadF(string path) {
        NodeFile file = TerminalProcessor.CurrDir.GetFile(path);
        if (file == null) throw new System.Exception($"File '{path}' not found.");
        return file.Content;
    }
    public FileReader OpenF(string path) {
        NodeFile file = TerminalProcessor.CurrDir.GetFile(path);
        if (file == null) throw new System.Exception("File is null.");
        return new FileReader(file);
    }
    public class FileReader {
        private readonly string[] _lines;
        private int _position;
        public FileReader(NodeFile file) {
            _lines = file.Content.Split(new[] { '\n', '\r' }, StringSplitOptions.None);
            _position = 0;
        }
        public string? ReadLine() {
            if (_position >= _lines.Length) return null;
            return _lines[_position++];
        }
        public void Reset() {
            _position = 0;
        }
    }
    // Enum for write modes	
    public enum WriteMode {
        Append = 0, Write = 1
    }
}
