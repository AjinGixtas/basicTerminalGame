using Godot;
using System.Xml.Linq;
public partial class SaveManager : Node {
    [Export] Overseer overseer;
    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("saveGame")) {
            SaveGame();
        }
    }
    void SaveGame() {
        TerminalProcessor.Say("Saving game...");
        NetworkNode network = overseer.terminal.networkManager.playerNode;

    }
}
