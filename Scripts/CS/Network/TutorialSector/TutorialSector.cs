using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class TutorialSector : StaticSector {
    public TutorialSector() {
        SurfaceNodes.Add(new KaraxeTutorialNode(null));
        Name = "SCALE_HOUSE_MetroClean_847137_nw";
    }
}
public class KaraxeTutorialNode : NetworkNode {
    public KaraxeTutorialNode(NetworkNode parentNode)
        : base(
            "cl3ver-b0y_fnZ3yX",
            "Zugzwang Washing Machine I748927423478278429374272357T14_06_2003",
            NetworkManager.GetRandomIP(),
            NodeType.VM,
            parentNode,
            false,
            [LockType.I4X]
        ) {
        ShellCore.OnCommandProcessed += PlayerRunHelp;
    }
    async void PlayerRunHelp(string command, Dictionary<string, string> farg, string[] parg) {
        if (command == "help") {
            DialogueManager.Interrupt();
            await SpeakHelpAsync();
            ShellCore.OnCommandProcessed -= PlayerRunHelp; // Unsubscribe to avoid multiple calls
        }
    }

    public override void NotifyConnected() {
        InitialWelcomeAsync();
    }
    ChatManager cm = new("cl3ver-b0y");
    void InitialWelcomeAsync() {
        cm.Enqueue("Hello! I'm Cl3ver B0y, one of your many guides on how to play this game.");
        cm.Enqueue("I will help you learn the basic commands.");
        cm.Enqueue("Let's start with a simple task: type 'help' to see what commands are available.");
        cm.Enqueue(15, "Or you can read the documentation. It's at the bottom of the menu bar.");
        cm.Enqueue(30, "..."); // Secret dialouge 1
        cm.Enqueue(15, $"Tbh, I'm not a very good teacher, so if you don't like me, just type {Util.Format($"unlink SCALE_HOUSE_MetroClean_847137_nw", StrType.CMD_FUL)} and be on your way.");
        cm.Enqueue($"If you miss me (you probably won't), do a visit here every once in a while with {Util.Format("link SCALE_HOUSE_MetroClean_847137_nw", StrType.CMD_FUL)}.");
        cm.Enqueue("It's neither lonely or boring in here (I'm connected to the internet lol) but if you come here, I will say hi to you :)");
        _ = cm.RunDialogueAsync();
    }

    async Task SpeakHelpAsync() {
        cm.Enqueue("Oh...");
        cm.Enqueue("So there's *quite* a lot of stuff here...");
        cm.Enqueue("Well, let's start with the basics.");
        cm.Enqueue($"`{Util.Format("sector", StrType.CMD_FUL)}` lists out sectors you can `{Util.Format("link <sector_name>", StrType.CMD_FUL)}` to.");
        cm.Enqueue($"`{Util.Format("scan", StrType.CMD_FUL)}` lists out neighbouring node.");
        cm.Enqueue($"`{Util.Format("home", StrType.CMD_FUL)}` returns you to your node. Use it when you get lost.");
        cm.Enqueue($"`{Util.Format("connect <ip_or_hostname>", StrType.CMD_FUL)}` connects you to other node (you just did it 2 minutes ago, but jk, jic)");
        cm.Enqueue($"`{Util.Format("ls", StrType.CMD_FUL)}` lists out files and directories.");
        cm.Enqueue($"`{Util.Format("mkf <filename>", StrType.CMD_FUL)}` makes a new file and `{Util.Format("edit <filename>", StrType.CMD_FUL)}` edits it.");
        cm.Enqueue($"`{Util.Format("run <filename>", StrType.CMD_FUL)}` will run any file you provide (if possible).");
        cm.Enqueue($"`{Util.Format("say <message>", StrType.CMD_FUL)}` outputs a message to terminal.");
        cm.Enqueue($"`{Util.Format("help", StrType.CMD_FUL)}` shows you the command documentation again.");
        cm.Enqueue($"`{Util.Format("stats", StrType.CMD_FUL)}` shows you your current stats.");
        _ = cm.RunDialogueAsync();
    }
    public override (CError, string, string, string)[] AttemptCrackNode(Dictionary<string, string> ans, double endEpoch) {
        (CError, string, string, string)[] result = base.AttemptCrackNode(ans, endEpoch);
        return result;
    }
}
public static class DialogueManager {
    private static CancellationTokenSource currentToken;

    public static void Interrupt() {
        currentToken?.Cancel();
    }
    public static string GetFnv1aTimeHash() {
        uint hash = Util.GetFn1vHash(BitConverter.GetBytes(Time.GetUnixTimeFromSystem()));
        return Convert.ToBase64String(BitConverter.GetBytes(hash)).TrimEnd('=');
    }
    public static async Task SaySequenceAsync((double, string)[] lines, string ircName = null) {
        Interrupt(); // Cancel any running dialogue
        currentToken = new CancellationTokenSource();
        var token = currentToken.Token;

        for(int i = 0; i < lines.Length; ++i) {
            if (token.IsCancellationRequested) break;
            string line = lines[i].Item2;
            await Task.Delay(Util.SkipDialogues ? 1000 : Mathf.CeilToInt(lines[i].Item1 * 1000), token);
            ShellCore.Say(ircName != null ? $"[{Util.Format(GetFnv1aTimeHash(), StrType.DECOR)}] <{Util.Format(ircName, StrType.USERNAME)}> {line}" : line);
        }
    }
}
public class ChatManager {
    public string CharacterName { get; set; }
    private readonly Queue<(string content, double delay)> _dialogueQueue = new();
    private CancellationTokenSource _cts = new();
    private readonly ManualResetEventSlim _pauseEvent = new(true); // starts unpaused
    public bool IsPaused => !_pauseEvent.IsSet;
    public ChatManager(string characterName) { CharacterName = characterName; }

    public void Enqueue(double delay, string content) => _dialogueQueue.Enqueue((content, delay));
    public void Enqueue(string content) => Enqueue(EstimateReadingTime(content), content);
    public void Pause() => _pauseEvent.Reset();
    public void Resume() => _pauseEvent.Set();
    public void Cancel() => _cts.Cancel();

    public async Task RunDialogueAsync() {
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        while (_dialogueQueue.Count > 0) {
            token.ThrowIfCancellationRequested();
            _pauseEvent.Wait(); // Wait here if paused
            (string content, double delay) = _dialogueQueue.Dequeue();
            await Task.Delay(Util.SkipDialogues ? 50 : (Mathf.CeilToInt(delay * 1000)), token);
            _pauseEvent.Wait(); // Check again before printing
            string timeHash = GetFnv1aTimeHash();
            ShellCore.Say($"[{Util.Format(timeHash, StrType.DECOR)}] <{Util.Format(CharacterName, StrType.USERNAME)}> {content}");
            LogToFileSys(CharacterName, $"[{timeHash}] <{CharacterName}> {Util.RemoveBBCode(content)}\n");
        }
    }
    static void LogToFileSys(string user, string msg) {
        string folder = "/irc-log", file = StringExtensions.PathJoin(folder, $"{user}.txt");
        if (PlayerFileManager.FileSystem.GetDir(folder) == null)
            PlayerFileManager.FileSystem.AddDir(folder);
        if (PlayerFileManager.FileSystem.GetFile(file) == null)
            PlayerFileManager.FileSystem.AddFile(file);
        PlayerFileManager.FileSystem.GetFile(file).Content += msg;
    }
    public static string GetFnv1aTimeHash() {
        uint hash = Util.GetFn1vHash(BitConverter.GetBytes(Time.GetUnixTimeFromSystem()));
        return Convert.ToBase64String(BitConverter.GetBytes(hash)).TrimEnd('=');
    }
    static double EstimateReadingTime(string message) {
        const double baseTime = 1.0;          // seconds, for prompt comprehension
        const double charsPerSecond = 25.0;   // customizable pace

        string clean = Util.RemoveBBCode(message);
        return baseTime + (clean.Length / charsPerSecond);
    }
}
