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
        if (delay > 0) { cm.Enqueue(delay, message); return; }
        cm.Enqueue(message);
    }
    protected void EnqueueTutorialDialouge(string message, ChatManager _cm, double delay = -1) {
        if (PlayerDataManager.CompletedTutorial) return; // Don't enqueue if tutorial is already completed
        _cm ??= cm;
        if (delay > 0) { _cm.Enqueue(delay, message); return; }
        _cm.Enqueue(message);
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
        NodeConnected += TutorialNodeActivate;
    }
    public void TutorialNodeActivate() {
        NodeConnected -= TutorialNodeActivate;
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
        return [(CError.NO_PERMISSION, "NOPE", "--nein", "10")];
    }
    public override bool RequestConnectPermission() {
        return base.RequestConnectPermission();
    }
}
public class ScriptTutorialNode : TutorialNode {
    public ScriptTutorialNode(NetworkNode parentNode)
        : base(
            "cthuwu",
            "Chīzuhoīru_Vehicle&Transportation_ID32758234792832_EXP2974-03-28",
            NetworkManager.GetRandomIP(),
            NodeType.VM,
            parentNode,
            false,
            [LockType.I4X]
        ) {
        cm = new ("cthuwu");
        Init(10, 10);
        NodeConnected += TutorialNodeActivate;
    }
    public void TutorialNodeActivate() {
        NodeConnected -= TutorialNodeActivate;
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
        EnqueueTutorialDialouge($"Scripting is quite complicated. So the [color={Util.CC(Cc.gR)}]Documentaion[/color] will be very helpful.");
        EnqueueTutorialDialouge("Here's a little gift.");
        NodeFile fileGift = new("Attack.lua",
@"-- You will see the use for them soon :)
local xtractors = {""fl1p"",""2bin"",""der3f"",""bl4nk""}
local colors = {""red"",""cyan"",""green"",""yellow"",""blue"",""magenta"",""white"",""black""}
-- Shortened name of the modules for convenience.
local mai, net, fio, bot, kar = MainModule, NetworkModule, FileModule, BotNetModule, KaraxeModule");
        PlayerFileManager.FileSystem.AddDir("Download"); PlayerFileManager.FileSystem.GetDir("Download").Add(fileGift);
        ShellCore.Say($"{Util.Format("Attack.lua", StrType.FILE)} {Util.Format("has been added to your file system", StrType.DECOR)}");
        EnqueueTutorialDialouge("Bye bye now ( ´ ▽ `)ﾉシ");
        FinishedTutorial = true;
        _ = cm.RunDialogueAsync();
    }
    public override (CError, string, string, string)[] AttemptCrackNode(Dictionary<string, string> ans, double endEpoch) {
        return [(CError.NO_PERMISSION, "?v=kXpmmJBaqIk", "--bggrrfff", "EGGGGGG!!!!!")];
    }
    public override bool RequestConnectPermission() {
        if ((ParentNode as TutorialNode).FinishedTutorial) return base.RequestConnectPermission();
        return false;
    }
}
public class KaraxeTutorialNode : TutorialNode {
    public KaraxeTutorialNode(NetworkNode parentNode)
        : base(
            "robo-cl4m",
            "TinyThingyInAnOldPhone_HaraldGormssonInternational_ID184858A00FD7971F810848266EBCECEE5E8B69972C5FFAED622F5EE078671AED_EXP####-##-##",
            NetworkManager.GetRandomIP(),
            NodeType.VM,
            parentNode,
            false,
            [LockType.I4X]
        ) {
        cm = new("Karaxe");
        Init(10, 10);
        NodeConnected += TutorialNodeActivate;
    }
    public void TutorialNodeActivate() {
        NodeConnected -= TutorialNodeActivate;
        if (PlayerDataManager.CompletedTutorial) return; // Don't start tutorial if already completed
        TutorialSpeak0();
    }
    void TutorialSpeak0() {
        EnqueueTutorialDialouge("I'm gonna teach you \"hacking\" in this game.");
        EnqueueTutorialDialouge("You're not allowed to hack us though. Go to the next node and I will teach ya.");
        _ = cm.RunDialogueAsync();
        ShellCore.CONNECTrunCMD += TutorialSpeak1;
    }
    bool tutorialSpeak1Block0 = false, tutorialSpeak1Block1 = false;
    void TutorialSpeak1(CError cer, string target) {
        if (ShellCore.CurrNode == this) { return; }
        if (ShellCore.CurrNode != ChildNode[0]) { 
            if (ShellCore.CurrNode.HostName == "cthuwu") {
                if (tutorialSpeak1Block0) tutorialSpeak1Block0 = true;
                EnqueueTutorialDialouge("Don't be a smartass, you know what I meant by the \"next node\".");
                EnqueueTutorialDialouge("Keep pestering the guy. It will never works.");
                return;
            }
            if (tutorialSpeak1Block1) return; tutorialSpeak1Block1 = true;
            EnqueueTutorialDialouge("You're going to the wrong node.");
            EnqueueTutorialDialouge("I own the node next to me, but not whatever you're trying to break in right now");
            _ = cm.RunDialogueAsync();
            return;
        }
        ShellCore.ANALYZErunCMD -= TutorialSpeak1;
        EnqueueTutorialDialouge($"Alright cool, now run `{Util.Format("analyze", StrType.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.ANALYZErunCMD += TutorialSpeak2;
    }
    int tutorialSpeak2_IncorrectAnalyzeHost = 0;
    void TutorialSpeak2(CError cer, string target) {
        if (ShellCore.CurrNode == this) {
            EnqueueTutorialDialouge("...", 3);
            EnqueueTutorialDialouge("uh huh.", 2);
            EnqueueTutorialDialouge("Pretty interesting yeah?");
            _ = cm.RunDialogueAsync();
            return;
        }
        if (ShellCore.CurrNode != ChildNode[0]) {
            tutorialSpeak2_IncorrectAnalyzeHost++;
            if (tutorialSpeak2_IncorrectAnalyzeHost < 2)
                EnqueueTutorialDialouge($"You know you are on the correct node from earlier. Do it again and run `{Util.Format("analyze", StrType.CMD_FUL)}`");
            else { 
                EnqueueTutorialDialouge($"Did you forget? The host target is {Util.Format($"{ChildNode[0].HostName}", StrType.HOSTNAME)}"); 
                EnqueueTutorialDialouge("I put a lot of effort into it y'know?");
            }
            _ = cm.RunDialogueAsync();
            return;
        }
        ShellCore.ANALYZErunCMD -= TutorialSpeak2;
        EnqueueTutorialDialouge("Cool. There's a number and a fancy word there.");
        EnqueueTutorialDialouge($"Go to the [color={Util.CC(Cc.gR)}]Documentation[/color] for explanation alright?");
        EnqueueTutorialDialouge("The important thing now is the `Firewall rating` and `Security Level`");
        EnqueueTutorialDialouge("`Security Level` is the approximated amount of locks you have to bypass to break a node.");
        EnqueueTutorialDialouge("`Firewall rating` is the the amount of locks + traps a node security system might have.");
        EnqueueTutorialDialouge("Until you are prepared, a good rule of thumb is to avoid node with Security Level and Firewall rating having different color");
        EnqueueTutorialDialouge($"For now, let's try to break it, run `{Util.Format("karaxe --flare", StrType.CMD_FUL)}` to begin.");
        _ = cm.RunDialogueAsync();
        if (ShellCore.RemainingTime > 10) _ = RushTutorialSpeak3();
        else ShellCore.KaraxeBegin += TutorialSpeak3;
    }
    async Task RushTutorialSpeak3() {
        await Task.Delay(5000);
        EnqueueTutorialDialouge("WAIT WHAT?");
        EnqueueTutorialDialouge("YOU STARTED IT?");
        EnqueueTutorialDialouge("Uh, so here's how this works...");
        _ = cm.RunDialogueAsync();
        TutorialSpeak3();
    }
    void TutorialSpeak3() {
        ShellCore.KaraxeBegin -= TutorialSpeak3;
        EnqueueTutorialDialouge("Alright, so we are VERY tight on time here.");
        EnqueueTutorialDialouge("You have 120 seconds to break nodes until it is over.");
        EnqueueTutorialDialouge("Normally the sector will ban you after this period is over, but this is exempt for training purpose");
        EnqueueTutorialDialouge($"You can use {Util.Format("arrows key", StrType.HEADER)} to cycle between old command.");
        EnqueueTutorialDialouge($"Or copy them from the terminal to save time typing.");
        EnqueueTutorialDialouge($"Now run this `{Util.Format("karaxe --attack", StrType.CMD_FUL)}`. Just do it.");
        _ = cm.RunDialogueAsync();
        ShellCore.KaraxeAttack += TutorialSpeak4_Attack;
    }
    void TutorialSpeak4_Attack((CError, string, string, string)[] result) {
        (CError cer, string target, string flag, string message) = result[^1];
        if (cer == CError.OK) {
            EnqueueTutorialDialouge("YES! YOU DID IT!");
            EnqueueTutorialDialouge("Good job buddy B)");
            ChatManager dev = new("(\\_AJIN_/)");
            EnqueueTutorialDialouge("Now get out there and enjoy the game.", dev);
            EnqueueTutorialDialouge("Hacked node will give you materials over time, and it does some stuff, erghh etc...", dev);
            EnqueueTutorialDialouge("Look, it's a lot. Read the doc, ja? -_-", dev);
            EnqueueTutorialDialouge("That's all now. If you have any problem, email me ^^", dev);
            EnqueueTutorialDialouge("Bye bye ( ´ ▽ `)ﾉシ");
            _ = cm.RunDialogueAsync();
        }
    }
    public override (CError, string, string, string)[] AttemptCrackNode(Dictionary<string, string> ans, double endEpoch) {
        EnqueueTutorialDialouge("Oh, you tried to hack me?");
        EnqueueTutorialDialouge("Whatever, I am hardcoded to not be hackable.");
        EnqueueTutorialDialouge("Now go to the next node.");
        _ = cm.RunDialogueAsync();
        return [(CError.NO_PERMISSION, "9001", "--power", "I am far beyond your comprehension...")];
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
