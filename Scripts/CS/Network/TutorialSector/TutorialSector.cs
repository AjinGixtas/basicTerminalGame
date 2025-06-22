using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class TutorialSector : StaticSector {
    public TutorialSector() {
        Name = "SCALE_HOUSE_MetroClean_847137_nw";
        NetworkNode node0 = new FileSysTutorialNode(null);
        NetworkNode node1 = new ScriptTutorialNode(node0);
        SurfaceNodes.Add(node0);
    }
}
public class TutorialNode : NetworkNode {
    public TutorialNode(string hostName, string displayName, string IP, NodeType NodeType, NetworkNode parentNode, bool ownedByPlayer, LockType[] lockCode) : 
        base(hostName, displayName, IP, NodeType, parentNode, ownedByPlayer, lockCode) {
    }
    /// <summary>
    /// Indicates whether the tutorial has been completed by the player.
    /// This usually disable dialogue and other tutorial-related features.
    /// </summary>
    public bool FinishedTutorial { get; protected set; } = false;
    /// <summary>
    /// Chat manager for the tutorial node, used to handle dialogue and interactions with the player.
    /// Each object represents a character in the game, such as a guide or mentor.
    /// Each object is only distinguished by its name, which is used to identify the character in the dialogue system.
    /// Which means you can have multiple objects throughout the codebase with the same name, and they will all be treated as the same character.
    /// </summary>
    protected ChatManager cm = null;
    /// <summary>
    /// Enqueue a dialogue message to be displayed to the player.
    /// This block only tutorial dialouge if needed.
    /// It is recommended to use Enqueue() directly for non-tutorial dialogue.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="delay"></param>
    protected void EnqueueTutorialDialouge(string message, double delay = -1) {
        if (PlayerDataManager.CompletedTutorial) return; // Don't enqueue if tutorial is already completed
        cm.Enqueue(message);
    }
}
public class FileSysTutorialNode : TutorialNode {
    public FileSysTutorialNode(NetworkNode parentNode)
        : base(
            "cl3ver-b0y_fnZ3yX",
            "Zugzwang_WashingMachine_ID7489274234_EXP3099-06-14",
            NetworkManager.GetRandomIP(),
            NodeType.VM,
            parentNode,
            false,
            [LockType.I4X]
        ) {
        Init(10, 10);
        cm = new("cl3ver-b0y");
        NodeConnected += OnNodeConnected;
    }
    public void OnNodeConnected() {
        if (PlayerDataManager.CompletedTutorial) return; // Don't start tutorial if already completed
        TutorialSpeak0();
    }
    void TutorialSpeak0() {
        EnqueueTutorialDialouge("Hello! I'm one of your many guides on how to play this game.", 20);
        EnqueueTutorialDialouge("I will help you learn the basic commands.");
        EnqueueTutorialDialouge($"Let's start with a simple task: type `{Util.Format("help", StrType.CMD_FUL)}` to see what commands are available.");
        _ = cm.RunDialogueAsync();
        ShellCore.OnHelpCMDrun += TutorialSpeak1;
    }
    void TutorialSpeak1() {
        ShellCore.OnHelpCMDrun -= TutorialSpeak1;
        EnqueueTutorialDialouge("Oh...");
        EnqueueTutorialDialouge("So there's *quite* a lot of stuff here...");
        EnqueueTutorialDialouge("Well, let's start with the basics.");
        EnqueueTutorialDialouge($"To begin, run `{Util.Format("ls", StrType.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.LSrunCMD += TutorialSpeak2;
    }
    void TutorialSpeak2(string actedPath, string finalPath) {
        ShellCore.LSrunCMD -= TutorialSpeak2;
        EnqueueTutorialDialouge("Great! Now you know how to list files and directories.");
        EnqueueTutorialDialouge($"Next, let's navigate through directories. Try running `{Util.Format("cd /irc-log", StrType.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.CDrunCMD += TutorialSpeak3;
    }
    void TutorialSpeak3(string actedPath, string finalPath, CError cer) {
        ShellCore.CDrunCMD -= TutorialSpeak3;
        if (finalPath != "/irc-log/") {
            EnqueueTutorialDialouge("Hmm... not in the correct folder, but it'll do.");
            EnqueueTutorialDialouge($"You can see a file's content with `{Util.Format("cat <file_name>", StrType.CMD_FUL)}`");
            EnqueueTutorialDialouge($"And you can edit it with `{Util.Format("edit <file_name>", StrType.CMD_FUL)}`");
            EnqueueTutorialDialouge($"Try running `{Util.Format("cat /irc-log/cl3ver-b0y.txt", StrType.CMD_FUL)}` to see the log of our conversation.");
            _ = cm.RunDialogueAsync();
            ShellCore.CATrunCMD += TutorialSpeak4;
            ShellCore.EDITrunCMD += TutorialSpeak4;
            return;
        }
        EnqueueTutorialDialouge("Good job! Now you know how to change directories.");
        EnqueueTutorialDialouge($"You can see a file's content with `{Util.Format("cat <file_name>", StrType.CMD_FUL)}`");
        EnqueueTutorialDialouge($"And you can edit it with `{Util.Format("edit <file_name>", StrType.CMD_FUL)}`");
        EnqueueTutorialDialouge($"Try running `{Util.Format("cat cl3ver-b0y.txt", StrType.CMD_FUL)}` to see the log of our conversation.");
        _ = cm.RunDialogueAsync();
        ShellCore.CATrunCMD += TutorialSpeak4;
        ShellCore.EDITrunCMD += TutorialSpeak4;
    }
    void TutorialSpeak4(string actedPath, string finalPath, CError cer)  {
        if (cer != CError.OK) { return; }
        ShellCore.CATrunCMD -= TutorialSpeak4;
        ShellCore.EDITrunCMD -= TutorialSpeak4;
        EnqueueTutorialDialouge("Now you know how to view files. As for managing them...");
        EnqueueTutorialDialouge($"It's just `{Util.Format("mkf", StrType.CMD_FUL)}`, `{Util.Format("rmf", StrType.CMD_FUL)}`, `{Util.Format("mkdir", StrType.CMD_FUL)}`, `{Util.Format("rmdir", StrType.CMD_FUL)}`");
        EnqueueTutorialDialouge("Pretty simple, ja?");
        EnqueueTutorialDialouge("Go on to the next node. They will teach you scripting!");
        EnqueueTutorialDialouge("Byebye ^^");
        FinishedTutorial = true;
        _ = cm.RunDialogueAsync();
    }
    public override (CError, string, string, string)[] AttemptCrackNode(Dictionary<string, string> ans, double endEpoch) {
        return [(CError.NO_PERMISSION, "9001", "--power", "I am far beyond your comprehension...")];
        (CError, string, string, string)[] result = base.AttemptCrackNode(ans, endEpoch);
        return result;
    }
    public override bool RequestConnectPermission() {
        return base.RequestConnectPermission();
    }
}
public class ScriptTutorialNode : TutorialNode {
    public ScriptTutorialNode(NetworkNode parentNode)
        : base(
            "cthuwu",
            "ChizuNoWa_Motorcycle_ID32758234792832_EXP2974-03-28",
            NetworkManager.GetRandomIP(),
            NodeType.VM,
            parentNode,
            false,
            [LockType.I4X]
        ) {
        cm = new ("cthuwu");
        Init(10, 10);
        NodeConnected += OnNodeConnected;
    }
    public void OnNodeConnected() {
        if (PlayerDataManager.CompletedTutorial) return; // Don't start tutorial if already completed
        TutorialSpeak0();
    }
    void TutorialSpeak0() {
        NodeFile fileSuprise = new("EVIL_VIRUS.lua", @$"ax:Say(""Hi :)"")");
        EnqueueTutorialDialouge("Hey! Catch this >:)");
        PlayerFileManager.FileSystem.AddDir("Download"); PlayerFileManager.FileSystem.GetDir("Download").Add(fileSuprise);
        ShellCore.Say($"{Util.Format("EVIL_VIRUS.lua", StrType.FILE)} {Util.Format("has been added to your file system", StrType.DECOR)}");
        EnqueueTutorialDialouge($"Try running it with `{Util.Format("run /Download/EVIL_VIRUS.lua", StrType.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();

        ShellCore.RUNrunCMD += TutorialSpeak1;
    }
    void TutorialSpeak1(string actedPath, string finalPath, CError cer) {
        ShellCore.RUNrunCMD -= TutorialSpeak1;
        if (cer != CError.OK) {
            EnqueueTutorialDialouge("Hmm... something went wrong.");
            EnqueueTutorialDialouge($"Make sure the script is there and `{Util.Format("run <file_path>", StrType.CMD_FUL)}`");
            _ = cm.RunDialogueAsync();
            return;
        }
        EnqueueTutorialDialouge("Good job! You just ran your first script!");
        EnqueueTutorialDialouge("Now you know how to run scripts.");
        EnqueueTutorialDialouge($"You can also edit them with `{Util.Format("edit <file_path>", StrType.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.EDITrunCMD += TutorialSpeak2;
    }
    void TutorialSpeak2(string actedPath, string finalPath, CError cer) {
        ShellCore.EDITrunCMD -= TutorialSpeak2;
        if (cer != CError.OK) {
            EnqueueTutorialDialouge("Hmm... something went wrong.");
            EnqueueTutorialDialouge($"Try running `{Util.Format("edit /Download/EVIL_VIRUS.lua", StrType.CMD_FUL)}` to see the script you just ran.");
            _ = cm.RunDialogueAsync();
            return;
        }
        EnqueueTutorialDialouge("Great! Now you know how to edit scripts.");
        EnqueueTutorialDialouge($"You can also create new scripts with `{Util.Format("mkf <file_name>", StrType.CMD_FUL)}`");
        EnqueueTutorialDialouge($"And remove them with `{Util.Format("rmf <file_name>", StrType.CMD_FUL)}`");
        EnqueueTutorialDialouge($"Scripting is quite complicated. Open the [color={Util.CC(Cc.gR)}]Documentaion[/color] whenever you need to. Treat it like a dictionary.");
        EnqueueTutorialDialouge("Here's a little gift. Bye bye now ( ´ ▽ `)ﾉシ");
        FinishedTutorial = true;
        _ = cm.RunDialogueAsync();
    }
    public override bool RequestConnectPermission() {
        if ((ParentNode as TutorialNode).FinishedTutorial) return base.RequestConnectPermission();
        return false;
    }
}

public class ChatManager {
    public string CharacterName { get; set; }
    private readonly Queue<(string content, double delay)> _dialogueQueue = new();
    private CancellationTokenSource _cts = new();
    private readonly ManualResetEventSlim _pauseEvent = new(true); // starts unpaused
    public bool IsPaused => !_pauseEvent.IsSet;
    public ChatManager(string characterName) { CharacterName = characterName; }

    /// <summary>
    /// Enqueue a dialogue message with an explicit delay.
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="content"></param>
    public void Enqueue(double delay, string content) => _dialogueQueue.Enqueue((content, delay));
    /// <summary>
    /// Enqueue a dialogue message with an estimated reading time based on the content length.
    /// </summary>
    /// <param name="content"></param>
    public void Enqueue(string content) => Enqueue(EstimateReadingTime(content), content);
    /// <summary>
    /// Pause the dialogue processing.
    /// </summary>
    public void Pause() => _pauseEvent.Reset();
    /// <summary>
    /// Resume the dialogue processing if it was paused.
    /// </summary>
    public void Resume() => _pauseEvent.Set();
    /// <summary>
    /// Cancel the dialogue processing and clear the queue.
    /// </summary>
    public void Cancel() => _cts.Cancel();

    /// <summary>
    /// Run the dialogue processing asynchronously.
    /// </summary>
    /// <returns></returns>
    public async Task RunDialogueAsync() {
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        string cache = "";
        while (_dialogueQueue.Count > 0) {
            token.ThrowIfCancellationRequested();
            _pauseEvent.Wait(); // Wait here if paused
            (string content, double delay) = _dialogueQueue.Dequeue();
            await Task.Delay(Util.SkipDialogues ? 50 : (Mathf.CeilToInt(delay * 1000)), token);
            _pauseEvent.Wait(); // Check again before printing
            string timeHash = GetFnv1aTimeHash();
            ShellCore.Say($"[{Util.Format(timeHash, StrType.DECOR)}] <{Util.Format(CharacterName, StrType.USERNAME)}> {content}");
            cache += $"[{timeHash}] <{CharacterName}> {Util.RemoveBBCode(content)}\n";
        }
        await Task.Delay(GD.RandRange(1000, 3000));
        LogToFileSys(CharacterName, cache); 
    }
    /// <summary>
    /// Log the dialogue to the file system for player reference in the future.
    /// </summary>
    /// <param name="user">If this parameter is the same across multiple object, they will are write to same file.</param>
    /// <param name="msg">Content of the message, include all custom IRC-styled formatting, removed BBCode</param>
    static void LogToFileSys(string user, string msg) {
        string folder = "/irc-log", file = StringExtensions.PathJoin(folder, $"{user}.txt");
        if (PlayerFileManager.FileSystem.GetDir(folder) == null)
            PlayerFileManager.FileSystem.AddDir(folder);
        if (PlayerFileManager.FileSystem.GetFile(file) == null)
            PlayerFileManager.FileSystem.AddFile(file);
        PlayerFileManager.FileSystem.GetFile(file).Content += msg;
        ShellCore.Say($"[color={Util.CC(Cc.w)}]Communication log saved to {Util.Format(file, StrType.FILE)}[/color]");
    }
    /// <summary>
    /// Hash the currect time using Fnv1a Hash twice in a row.
    /// </summary>
    /// <returns></returns>
    public static string GetFnv1aTimeHash() {
        uint hash = Util.GetFn1vHash(BitConverter.GetBytes(Util.GetFn1vHash(BitConverter.GetBytes(Util.GetFn1vHash(BitConverter.GetBytes(Time.GetUnixTimeFromSystem()))))));
        return Convert.ToBase64String(BitConverter.GetBytes(hash)).TrimEnd('=');
    }
    /// <summary>
    /// Estimate the reading time for a message based on its length.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    static double EstimateReadingTime(string message) {
        const double baseTime = 1.0;          // seconds, for prompt comprehension
        const double charsPerSecond = 25.0;   // customizable pace

        string clean = Util.RemoveBBCode(message);
        return baseTime + (clean.Length / charsPerSecond);
    }
}
