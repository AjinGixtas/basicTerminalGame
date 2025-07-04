using Godot;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class TutorialSector : StaticSector {
    public const string TUTORIAL_SECTOR_NAME = "YES_I_AM_THE_SECTOR_YOU_ARE_LOOKING_FOR_PLEASE_LINK_2_ME_NOW";
    public TutorialSector() {
        Name = TUTORIAL_SECTOR_NAME;
        NetworkNode node0 = new FileSysTutorialNode(null);
        NetworkNode node1 = new ScriptTutorialNode(node0);
        NetworkNode node2 = new KaraxeTutorialNode(node1);
        NetworkNode node3 = new ScriptedNetworkNode(new([], [new() { mineralProfile = ItemCrafter.MINERALS[0], weight = 1.0 }], [], [(int)LocT.C0,(int)LocT.I4X], NodeType.VM, "robo-cl4m-vm", "ROBO CL4M VM DO NOT TOUCH", 128, 1, 1, 1, 2, SecLvl.LOSEC), node2);
        SurfaceNodes.Add(node0);
    }
}
public class TutorialNode : NetworkNode {
    public TutorialNode(string hostName, string displayName, string IP, NodeType NodeType, NetworkNode parentNode, bool ownedByPlayer, LocT[] lockCode) : 
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
    protected void EnqueueTutorialDialouge(string message, double delay = -1, ChatManager _cm = null) {
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
            [LocT.I4X]
        ) {
        Init(10, SecLvl.NOSEC);
        cm = new("cl3ver-b0y");
        NodeConnected += TutorialNodeActivate;
    }
    public void TutorialNodeActivate() {
        NodeConnected -= TutorialNodeActivate;
        if (PlayerDataManager.CompletedTutorial) return; // Don't start tutorial if already completed
        TutorialSpeak0();
    }
    async void TutorialSpeak0() {
        await Task.Delay(7_000);
        EnqueueTutorialDialouge("Hello! I'm one of your many guides on how to play this game.");
        EnqueueTutorialDialouge("I will help you learn the basic commands.");
        EnqueueTutorialDialouge($"Let's start with a simple task: type `{Util.Format("help", StrSty.CMD_FUL)}` to see what commands are available.");
        _ = cm.RunDialogueAsync();
        ShellCore.OnHelpCMDrun += TutorialSpeak1;
    }
    async void TutorialSpeak1() {
        ShellCore.OnHelpCMDrun -= TutorialSpeak1;
        await Task.Delay(1000);
        EnqueueTutorialDialouge("Hm.");
        EnqueueTutorialDialouge("So there's *quite* a lot of stuff here.");
        EnqueueTutorialDialouge("Well. Let's start with the basics.");
        EnqueueTutorialDialouge($"To begin, run `{Util.Format("ls", StrSty.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.LSrunCMD += TutorialSpeak2;
    }
    async void TutorialSpeak2(string actedPath, string finalPath) {
        ShellCore.LSrunCMD -= TutorialSpeak2;
        await Task.Delay(1000);
        EnqueueTutorialDialouge("Great! Now you know how to list files and directories.");
        EnqueueTutorialDialouge($"Next, let's navigate through directories. Try running `{Util.Format("cd /irc-log", StrSty.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.CDrunCMD += TutorialSpeak3;
    }
    async void TutorialSpeak3(string actedPath, string finalPath, CError cer) {
        ShellCore.CDrunCMD -= TutorialSpeak3;
        if (finalPath != "/irc-log/") {
            await Task.Delay(1000);
            EnqueueTutorialDialouge("Hmm... not in the correct folder, but it'll do.");
            EnqueueTutorialDialouge($"You can see a file's content with `{Util.Format("read <file_name>", StrSty.CMD_FUL)}`");
            EnqueueTutorialDialouge($"And you can edit it with `{Util.Format("edit <file_name>", StrSty.CMD_FUL)}`");
            EnqueueTutorialDialouge($"Try running `{Util.Format("read /irc-log/cl3ver-b0y.txt", StrSty.CMD_FUL)}` to see the log of our conversation.");
            _ = cm.RunDialogueAsync();
            ShellCore.CATrunCMD += TutorialSpeak4;
            ShellCore.EDITrunCMD += TutorialSpeak4;
            return;
        }
        await Task.Delay(1000);
        EnqueueTutorialDialouge("Good job! Now you know how to change directories.");
        EnqueueTutorialDialouge($"You can see a file's content with `{Util.Format("read <file_name>", StrSty.CMD_FUL)}`");
        EnqueueTutorialDialouge($"Try running `{Util.Format("read cl3ver-b0y.txt", StrSty.CMD_FUL)}` to see the log of our conversation.");
        _ = cm.RunDialogueAsync();
        ShellCore.CATrunCMD += TutorialSpeak4;
        ShellCore.EDITrunCMD += TutorialSpeak4;
    }
    async void TutorialSpeak4(string actedPath, string finalPath, CError cer)  {
        if (cer != CError.OK) { return; }
        ShellCore.CATrunCMD -= TutorialSpeak4;
        ShellCore.EDITrunCMD -= TutorialSpeak4;
        await Task.Delay(1000);
        EnqueueTutorialDialouge("Now you know how to view files. As for managing them...");
        EnqueueTutorialDialouge($"It's just `{Util.Format("mkf", StrSty.CMD_FUL)}`, `{Util.Format("rmf", StrSty.CMD_FUL)}`, `{Util.Format("mkdir", StrSty.CMD_FUL)}`, `{Util.Format("rmdir", StrSty.CMD_FUL)}`");
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
            [LocT.I4X]
        ) {
        cm = new ("cthuwu");
        Init(10, SecLvl.NOSEC);
        NodeConnected += TutorialNodeActivate;
    }
    public void TutorialNodeActivate() {
        NodeConnected -= TutorialNodeActivate;
        if (PlayerDataManager.CompletedTutorial) return; // Don't start tutorial if already completed
        TutorialSpeak0();
    }
    async void TutorialSpeak0() {
        NodeFile fileSuprise = new("FunniFile.lua", @$"MainModule:Say(""Hi, I'm a script that do stuff. (JS variant available)\n"")");
        ShellCore.Say($"{Util.Format("FunniFile.lua", StrSty.FILE)} {Util.Format("has been added to your file system", StrSty.DECOR)}");
        PlayerFileManager.FileSystem.AddDir("Download"); PlayerFileManager.FileSystem.GetDir("Download").Add(fileSuprise);
        
        EnqueueTutorialDialouge($"Hey! `{Util.Format("run <that_file_path_i_gave_ya>", StrSty.CMD_FUL)}`", 0);
        await cm.RunDialogueAsync(silentSave:true);
        ShellCore.RUNrunCMD += TutorialSpeak1;
    }
    async void TutorialSpeak1(string actedPath, string finalPath, CError cer) {
        if (cer != CError.OK) {
            await Task.Delay(500);
            EnqueueTutorialDialouge("Hmm... something went wrong.");
            EnqueueTutorialDialouge($"Make sure the script is there and do `{Util.Format("run <file_path>", StrSty.CMD_FUL)}`");
            _ = cm.RunDialogueAsync();
            return;
        }
        ShellCore.RUNrunCMD -= TutorialSpeak1;
        await Task.Delay(1000);
        EnqueueTutorialDialouge("Good job! You just ran your first script!");
        EnqueueTutorialDialouge("Now you know how to run scripts.");
        EnqueueTutorialDialouge($"You can also edit them with `{Util.Format("edit <file_path>", StrSty.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.EDITrunCMD += TutorialSpeak2;
    }
    async void TutorialSpeak2(string actedPath, string finalPath, CError cer) {
        ShellCore.EDITrunCMD -= TutorialSpeak2;
        if (cer != CError.OK) {
            await Task.Delay(500);
            EnqueueTutorialDialouge("Hmm... something went wrong.");
            EnqueueTutorialDialouge($"Run `{Util.Format("edit <file_path>", StrSty.CMD_FUL)}` with a valid file to progress.");
            _ = cm.RunDialogueAsync();
            return;
        }
        await Task.Delay(1000);
        EnqueueTutorialDialouge("Great! Now you know how to edit scripts.");
        EnqueueTutorialDialouge($"You can also create new scripts with `{Util.Format("mkf <file_name>", StrSty.CMD_FUL)}`");
        EnqueueTutorialDialouge($"Scripting is quite complicated. So the {Util.Format("Documentation", StrSty.GAME_WINDOW)} will be very helpful.");
        EnqueueTutorialDialouge("Here's a little gift.");
        await cm.RunDialogueAsync(silentSave:true);
        NodeFile fileGift = new("Attack.lua",
@"-- You will see the use for them soon :)

-- DO NOT TOUCH THIS PART!!!
local xtractors = {""fl1p"",""2bin"",""der3f"",""bl4nk""}
local colors = {""red"",""cyan"",""green"",""yellow"",""blue"",""magenta"",""white"",""black""}
-- DO NOT TOUCH THIS PART!!! end

-- Shortened name of the modules for convenience.
local mai, net, fio, bot, kar = MainModule, NetworkModule, FileModule, BotNetModule, KaraxeModule");
        PlayerFileManager.FileSystem.AddDir("Download"); PlayerFileManager.FileSystem.GetDir("Download").Add(fileGift);
        ShellCore.Say($"{Util.Format("Attack.lua", StrSty.FILE)} {Util.Format("has been added to your file system", StrSty.DECOR)}");
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
            [LocT.I4X]
        ) {
        cm = new("Karaxe");
        Init(10, SecLvl.NOSEC);
        NodeConnected += TutorialNodeActivate;
    }
    public void TutorialNodeActivate() {
        NodeConnected -= TutorialNodeActivate;
        if (PlayerDataManager.CompletedTutorial) return; // Don't start tutorial if already completed
        TutorialSpeak0();
    }
    async void TutorialSpeak0() {
        await Task.Delay(1_000);
        EnqueueTutorialDialouge("I'm gonna teach you \"hacking\" in this game.");
        EnqueueTutorialDialouge("You're not allowed to hack me though. Go to the next node and I will teach ya.");
        _ = cm.RunDialogueAsync();
        ShellCore.CONNECTrunCMD += TutorialSpeak1;
    }
    bool tutorialSpeak1Block0 = false, tutorialSpeak1Block1 = false;
    async void TutorialSpeak1(CError cer, string target) {
        if (ShellCore.CurrNode == this) { return; }
        if (ShellCore.CurrNode != ChildNode[0]) { 
            if (ShellCore.CurrNode.HostName == "cthuwu") {
                if (tutorialSpeak1Block0) tutorialSpeak1Block0 = true;
                await Task.Delay(333);
                EnqueueTutorialDialouge("Are you stupid or do you not see my VM? Go there now.");
                _ = cm.RunDialogueAsync();
                return;
            }
            if (tutorialSpeak1Block1) return; tutorialSpeak1Block1 = true;
            await Task.Delay(333);
            EnqueueTutorialDialouge("You're going to the wrong node.");
            EnqueueTutorialDialouge("I own the node next to me, but not whatever you're trying to break in right now");
            _ = cm.RunDialogueAsync();
            return;
        }
        ShellCore.CONNECTrunCMD -= TutorialSpeak1;
        await Task.Delay(1_000);
        EnqueueTutorialDialouge($"Alright cool, now run `{Util.Format("analyze", StrSty.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
        ShellCore.ANALYZErunCMD += TutorialSpeak2;
    }
    int tutorialSpeak2_IncorrectAnalyzeHost = 0;
    async void TutorialSpeak2(CError cer, string target) {
        await Task.Delay(500); // Give player time to read the previous message
        if (ShellCore.CurrNode == this) {
            EnqueueTutorialDialouge("...", 1);
            EnqueueTutorialDialouge("uh huh.", 3);
            EnqueueTutorialDialouge("Now go scan my vm.");
            _ = cm.RunDialogueAsync();
            return;
        }
        if (ShellCore.CurrNode != ChildNode[0]) {
            tutorialSpeak2_IncorrectAnalyzeHost++;
            if (tutorialSpeak2_IncorrectAnalyzeHost < 2)
                EnqueueTutorialDialouge($"You know you are on the correct node from earlier. Do it again and run `{Util.Format("analyze", StrSty.CMD_FUL)}`");
            else { 
                EnqueueTutorialDialouge($"Did you forget? The host target is {Util.Format($"{ChildNode[0].HostName}", StrSty.HOSTNAME)}"); 
                EnqueueTutorialDialouge("I put a lot of effort into it y'know?");
            }
            _ = cm.RunDialogueAsync();
            return;
        }
        ShellCore.ANALYZErunCMD -= TutorialSpeak2;
        EnqueueTutorialDialouge("Cool. There's number and a fancy word there.");
        EnqueueTutorialDialouge("Ignore everything");
        EnqueueTutorialDialouge("The only important thing now is the `Firewall rating` and `Security Level`");
        EnqueueTutorialDialouge("`Firewall rating` is the the amount of locks a node security system have.");
        EnqueueTutorialDialouge("`Security Level` is how hard the lock is.");
        EnqueueTutorialDialouge($"Until you are prepared, see the `Lock details` section in the {Util.Format("Documentation", StrSty.GAME_WINDOW)}.");
        EnqueueTutorialDialouge($"Unless it's [color={Util.ColorMapperSecLvl("NOSEC")}]NOSEC[/color], that is free food.");
        EnqueueTutorialDialouge($"For now, let's try to break it, run `{Util.Format("karaxe --flr", StrSty.CMD_FUL)}` to begin.");
        _ = cm.RunDialogueAsync();
        if (ShellCore.RemainingTime > 10) RushTutorialSpeak3();
        else ShellCore.KaraxeBegin += TutorialSpeak3;
    }
    async void RushTutorialSpeak3() {
        await Task.Delay(2000);
        EnqueueTutorialDialouge("WAIT WHAT?");
        EnqueueTutorialDialouge("YOU STARTED IT?");
        EnqueueTutorialDialouge("Uh, sohere'showthisworks...");
        _ = cm.RunDialogueAsync();
        TutorialSpeak3();
    }
    async void TutorialSpeak3() {
        ShellCore.KaraxeBegin -= TutorialSpeak3;
        await Task.Delay(2000);
        EnqueueTutorialDialouge("Alright, so we are VERY tight on time here.");
        EnqueueTutorialDialouge("You have 120 seconds to break nodes until it is over.");
        EnqueueTutorialDialouge($"Normally the sector will [color={Util.CC(Cc.R)}]BAN[/color] you after this period is over, but this is exempted for training purpose");
        EnqueueTutorialDialouge($"You can use {Util.Format("arrows key", StrSty.HEADER)} to cycle between old command.");
        EnqueueTutorialDialouge($"Or copy them from the terminal to save time typing.");
        EnqueueTutorialDialouge($"Now run `{Util.Format("karaxe --atk", StrSty.CMD_FUL)}`. Just do it.");
        _ = cm.RunDialogueAsync();
        ShellCore.KaraxeAttack += TutorialSpeak4_Attack;
        ShellCore.KaraxeEnd += TutorialSpeak4_End;
    }
    void TutorialSpeak4_End() {
        EnqueueTutorialDialouge("...", 2);
        EnqueueTutorialDialouge("You failed.");
        EnqueueTutorialDialouge("We go again.");
        EnqueueTutorialDialouge($"Run `{Util.Format("karaxe --flr", StrSty.CMD_FUL)}`");
        _ = cm.RunDialogueAsync();
    }
    async void TutorialSpeak4_Attack((CError, string, string, string)[] result) {
        await Task.Delay(1000);
        (CError cer, string locN, string locF, string msg) = result[^1];
        if (cer == CError.OK) {
            ShellCore.KaraxeAttack -= TutorialSpeak4_Attack;
            ShellCore.KaraxeEnd -= TutorialSpeak4_End;
            EnqueueTutorialDialouge("YES! YOU DID IT!!!!!");
            EnqueueTutorialDialouge("Good job buddy B)");
            EnqueueTutorialDialouge("Now get out there and enjoy the game.");
            EnqueueTutorialDialouge("Hacked node will give you materials over time, and help crafting stuff that sells money, erghh etc...");
            EnqueueTutorialDialouge($"The game is complicated, so use the {Util.Format("help", StrSty.CMD_FUL)} command and the {Util.Format("Documentation", StrSty.GAME_WINDOW)} often.");
            PlayerDataManager.CompletedTutorial = true; // Mark the tutorial as completed
            _ = cm.RunDialogueAsync();
            return;
        }
        if (cer == CError.MISSING) {
            EnqueueTutorialDialouge("That means you are missing the flag given, copy the earlier command with that flag at the end.");
            _ = cm.RunDialogueAsync();
            return;
        }
        if (cer == CError.INCORRECT) {
            if (locN == "I4X"){
                if (locF == "--i4") EnqueueTutorialDialouge("The answer is one of the first 4 positive natural number. Try each one.");
                else if (locF == "--2xtract") EnqueueTutorialDialouge($"The answer is one of the 4 phrase `{Util.Format("fl1p", StrSty.CMD_ARG)}`, `{Util.Format("2bin", StrSty.CMD_ARG)}`, `{Util.Format("der3f", StrSty.CMD_ARG)}`, `{Util.Format("bl4nk", StrSty.CMD_ARG)}`. Try each one.");
            } else if (locN == "C0"){
                if (locF == "--c0") EnqueueTutorialDialouge($"The answer is the color name opposite to the one you see in the terminal. The full color list is: [color={Util.CC(Cc.R)}]red[/color], [color={Util.CC(Cc.G)}]green[/color], [color={Util.CC(Cc.B)}]blue[/color], [color={Util.CC(Cc.C)}]cyan[/color], [color={Util.CC(Cc.M)}]magenta[/color], [color={Util.CC(Cc.Y)}]yellow[/color], [color={Util.CC(Cc.W)}]white[/color] and [color={Util.CC(Cc.w)}]black[/color]");
            }
            _ = cm.RunDialogueAsync();
            return;
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
    public async Task RunDialogueAsync(bool silentSave=false) {
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        string cache = "";
        while (_dialogueQueue.Count > 0) {
            token.ThrowIfCancellationRequested();
            _pauseEvent.Wait(); // Wait here if paused
            (string content, double delay) = _dialogueQueue.Dequeue();
            _pauseEvent.Wait(); // Check again before printing
            string timeHash = Util.GetFnv1aTimeHash();
            ShellCore.Say($"\n[color={Util.CC(Cc.w)}][[/color][color={Util.CC(Cc._)}]{timeHash}[/color][color={Util.CC(Cc.w)}]][/color] <{Util.Format(CharacterName, StrSty.USERNAME)}> {content}");
            await Task.Delay(Util.SkipDialogues ? 0 : delay >= 0 ? Mathf.CeilToInt(delay * 1000) : Mathf.CeilToInt(EstimateReadingTime(content) * 1000), token);
            cache += $"[{timeHash}] <{CharacterName}> {Util.RemoveBBCode(content)}\n";
        }
        await Task.Delay(20);
        LogToFileSys(CharacterName, cache, silentSave);
    }
    /// <summary>
    /// Log the dialogue to the file system for player reference in the future.
    /// </summary>
    /// <param name="user">If this parameter is the same across multiple object, they will are write to same file.</param>
    /// <param name="msg">Content of the message, include all custom IRC-styled formatting, removed BBCode</param>
    static void LogToFileSys(string user, string msg, bool silentSave) {
        string folder = "/irc-log", file = StringExtensions.PathJoin(folder, $"{user}.txt");
        if (PlayerFileManager.FileSystem.GetDir(folder) == null)
            PlayerFileManager.FileSystem.AddDir(folder);
        if (PlayerFileManager.FileSystem.GetFile(file) == null)
            PlayerFileManager.FileSystem.AddFile(file);
        PlayerFileManager.FileSystem.GetFile(file).Content += msg;
        if (!silentSave) ShellCore.Say($"[color={Util.CC(Cc.w)}]Communication log saved to {Util.Format(file, StrSty.FILE)}[/color]");
    }
    /// <summary>
    /// Estimate the reading time for a message based on its length.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    static double EstimateReadingTime(string message) {
        const double baseTime = .333;          // seconds, for prompt comprehension
        const double charsPerSecond = 28.0;   // customizable pace

        string clean = Util.RemoveBBCode(message);
        return baseTime + (clean.Length / charsPerSecond);
    }
}
