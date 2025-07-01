using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
public static partial class Util {
    /// <summary>
    /// Use to prevent final word of objects before getting freed.
    /// </summary>
    public const bool HaveFinalWord = false;
    /// <summary>
    /// Skip dialogues in the game, used for testing purposes.
    /// </summary>
    public const bool SkipDialogues = false;
    public static T[] Shuffle<T>(T[] array) {
        for (int i = 0; i < array.Length; i++) {
            int j = GD.RandRange(0, array.Length - 1); 
            (array[i], array[j]) = (array[j], array[i]); 
        }
        return array;
    }
    public static List<T> Shuffle<T>(List<T> array) {
        for (int i = 0; i < array.Count; i++) {
            int j = GD.RandRange(0, array.Count- 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        return array;
    }
    /// <summary>
    /// Converts a Cc enum value to a color code string.
    /// </summary>
    /// <param name="color"></param>
    /// <returns>
    /// <br/><c>Cc._</c>: Black, <c>Cc.w</c>: Dark gray, <c>Cc.W</c>: White
    /// <br/><c>Cc.r</c>: Deep red, <c>Cc.R</c>: Bright red, <c>Cc.LR</c>: Light red (coral)
    /// <br/><c>Cc.g</c>: Deep green, <c>Cc.G</c>: Bright green, <c>Cc.LG</c>: Light green
    /// <br/><c>Cc.b</c>: Deep blue, <c>Cc.B</c>: Bright blue, <c>Cc.LB</c>: Light blue (periwinkle)
    /// <br/><c>Cc.c</c>: Dark cyan (teal), <c>Cc.C</c>: Bright cyan, <c>Cc.LC</c>: Light cyan
    /// <br/><c>Cc.m</c>: Deep magenta, <c>Cc.M</c>: Bright magenta, <c>Cc.LM</c>: Light magenta
    /// <br/><c>Cc.y</c>: Dark yellow (olive), <c>Cc.Y</c>: Bright yellow, <c>Cc.LY</c>: Light yellow
    /// <br/><br/>Mixed/blended colors:
    /// <br/><c>Cc.bR</c>: Hot pink, <c>Cc.gR</c>: Amber (orange-yellow), <c>Cc.gB</c>: Ocean blue
    /// <br/><c>Cc.rB</c>: Violet, <c>Cc.rG</c>: Lime green, <c>Cc.bG</c>: Mint green
    /// <br/><br/>Defaults to <c>Cc.W</c> in all other cases.
    /// </returns>
    public static string CC(Cc color) {
        // CC and Cc are both used for color codes, but Cc is an enum and CC is a method to get the color code string.
        return color switch {
            Cc._ => "#000000",   // Black
            Cc.w => "#666666",   // Dark gray
            Cc.W => "#ffffff",   // White

            Cc.R => "#ff0000",     // Bright red
            Cc.G => "#00ff00",     // Bright green
            Cc.B => "#0000ff",     // Bright blue
            Cc.C => "#00ffff",     // Bright cyan
            Cc.M => "#ff00ff",     // Bright magenta
            Cc.Y => "#ffff00",     // Bright yellow

            Cc.r => "#cc0000",     // Deep red
            Cc.g => "#00cc00",     // Deep green
            Cc.b => "#0000cc",     // Deep blue
            Cc.c => "#005C5C",     // Dark teal
            Cc.m => "#cc00cc",     // Deep magenta
            Cc.y => "#cccc00",     // Olive yellow

            Cc.bR => "#ff00aa",    // Hot pink
            Cc.gR => "#ffaa00",    // Amber
            Cc.gB => "#00aaff",    // Aqua blue
            Cc.rB => "#aa00ff",    // Violet
            Cc.rG => "#aaff00",    // Lime green
            Cc.bG => "#00ffaa",    // Mint green

            Cc.LR => "#ff6666",    // Light coral
            Cc.LG => "#66ff66",    // Light green
            Cc.LB => "#6666ff",    // Periwinkle
            Cc.LC => "#adffff",    // Light cyan
            Cc.LM => "#ff66ff",    // Light magenta
            Cc.LY => "#ffff66",    // Light yellow

            _ => "#ffffff",        // Default white
        };
    }
    public static string Obfuscate(string input) {
        char[] OBFUSCATE_CHAR = ['!', '@', '#', '$', '%', '^', '&', '*'];
        char[] chars = input.ToCharArray();
        for (int i = 0; i < input.Length; ++i) {
            chars[i] = (GD.Randf() < .9 ? OBFUSCATE_CHAR[GD.RandRange(0, OBFUSCATE_CHAR.Length-1)] : chars[i]);
        }
        return new string(chars);
    }
    /// <summary>
    /// Formats the given <paramref name="input"/> string with color tags based on the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="input">The text to wrap in color tags.</param>
    /// <param name="type">One of the <see cref="StrSty"/> values indicating how to colorize <paramref name="input"/>.</param>
    /// <param name="addons">
    /// Optional parameters for certain types:
    /// <list type="bullet">
    ///   <item><description><see cref="StrSty.UNIT"/>: addons[0] is the unit suffix (e.g. “MB”).</description></item>
    ///   <item><description><see cref="StrSty.NUMBER"/>: addons[0] is the number of decimal places.</description></item>
    ///   <item><description><see cref="StrSty.T_MINERAL"/>: addons[0] is the mineral index.</description></item>
    ///   <item><description><see cref="StrSty.COLORED_ITEM_NAME"/>: addons[0] is the mineral index for lookup.</description></item>
    ///   <item><description><see cref="StrSty.UNKNOWN_ERROR"/>: addons[0] is optional extra context.</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// A string wrapped in color tags:
    /// <list type="bullet">
    ///   <item><description><c>StrType.DECOR</c>: [color=<c>Cc.w</c>]</description></item>
    ///   <item><description><c>StrType.IP</c>:    [color=<c>Cc.C</c>]</description></item>
    ///   <item><description><c>StrType.HOSTNAME</c>: [color=<c>Cc.gR</c>]</description></item>
    ///   <item><description><c>StrType.DISPLAY_NAME</c>: [color=<c>Cc.G</c>]</description></item>
    ///   <item><description><c>StrType.SECTOR</c>: uses <c>ColorMapperSecLvl</c> to pick a <c>Cc</c> based on sector level.</description></item>
    ///   <item><description><c>StrType.UNIT</c>: formats input as a number then appends [color=<c>Cc.W</c>]addons[0]</description></item>
    ///   <item><description><c>StrType.SEC_LVL</c>, <c>StrType.SEC_TYPE</c>, <c>StrType.DEF_LVL</c>: all use <c>ColorMapperSecLvl</c> with input as both value and level.</description></item>
    ///   <item><description><c>StrType.USERNAME</c>: [color=<c>Cc.M</c>]</description></item>
    ///   <item><description><c>StrType.DESC</c>:     [color=<c>Cc.g</c>]</description></item>
    ///   <item><description><c>StrType.SYMBOL</c>:   [color=<c>Cc.m</c>]</description></item>
    ///   <item><description><c>StrType.NUMBER</c>: wraps a parsed and formatted numeric value in [color=<c>Cc.C</c>]</description></item>
    ///   <item><description><c>StrType.DIR</c>:       [color=<c>Cc.C</c>]</description></item>
    ///   <item><description><c>StrType.FILE</c>: splits path into directory ([color=<c>Cc.C</c>]) + name ([color=<c>Cc.G</c>])</description></item>
    ///   <item><description><c>StrType.CMD_FUL</c>: full command string, with<br/>
    ///     - name formatted via <c>StrType.CMD_ACT</c>,<br/>
    ///     - flags via <c>StrType.CMD_FLAG</c>,<br/>
    ///     - args via <c>StrType.CMD_ARG</c>,<br/>
    ///     and semicolon separators in [color=<c>Cc.W</c>]</description></item>
    ///   <item><description><c>StrType.CMD_FLAG</c>: [color=<c>Cc.gR</c>]</description></item>
    ///   <item><description><c>StrType.CMD_ARG</c>:  [color=<c>Cc.LR</c>]</description></item>
    ///   <item><description><c>StrType.CMD_ACT</c>:  [color=<c>Cc.gB</c>]</description></item>
    ///   <item><description><c>StrType.ERROR</c>:    [color=<c>Cc.R</c>]</description></item>
    ///   <item><description><c>StrType.HEADER</c>:   [color=<c>Cc.gR</c>]</description></item>
    ///   <item><description><c>StrType.MONEY</c>: formats large integer into units [<c>Cc.w</c>]&lt;value&gt; + [<c>Cc.*</c>]&lt;unit&gt; + “GC” in [<c>Cc.Y</c>]</description></item>
    ///   <item><description><c>StrType.T_MINERAL</c>: splits large value into mineral units with [<c>Cc.w</c>] value + [<c>Cc.*</c>] unit + profile shorthand color</description></item>
    ///   <item><description><c>StrType.G_MINERAL</c>: [color=<c>Cc.ItemCrafter.MINERALS[index].ColorCode</c>]</description></item>
    ///   <item><description><c>StrType.WARNING</c>: [color=<c>Cc.Y</c>] prefix “[WARNING] ”</description></item>
    ///   <item><description><c>StrType.PART_SUCCESS</c>: [color=<c>Cc.C</c>]</description></item>
    ///   <item><description><c>StrType.FULL_SUCCESS</c>: [color=<c>Cc.G</c>]</description></item>
    ///   <item><description><c>StrType.UNKNOWN_ERROR</c>: calls <see cref="Format(string, StrSty)"/> recursively with error text in [color=<c>Cc.R</c>]</description></item>
    ///   <item><description><c>StrType.GAME_WINDOW</c>: [color=<c>Cc.gR</c>]</description></item>
    ///   <item><description><c>StrType.CODE_MODULE</c>: [color=<c>Cc.LB</c>]</description></item>
    ///   <item><description>default: returns <paramref name="input"/> unchanged.</description></item>
    /// </list>
    /// </returns>
    public static string Format(string input, StrSty type, params string[] addons) {
        switch (type) {
            case StrSty.DECOR:
                return $"[color={Util.CC(Cc.w)}]{input}[/color]";
            case StrSty.IP:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrSty.HOSTNAME:
                return $"[color={Util.CC(Cc.gR)}]{input}[/color]";
            case StrSty.DISPLAY_NAME:
                return $"[color={Util.CC(Cc.G)}]{input}[/color]";
            case StrSty.SECTOR:
                return $"[color={ColorMapperSecLvl(NetworkManager.GetDriftSectorByName(input)?.SectorLevel ?? 0)}]{input}[/color]";
            case StrSty.UNIT: {
                    if (string.IsNullOrWhiteSpace(input)) return "";
                    return $"{Util.Format(input, StrSty.NUMBER)}[color={Util.CC(Cc.W)}]{addons[0]}[/color]";
                }
            case StrSty.SEC_LVL:
                    return $"[color={ColorMapperSecLvl(input)}]{input}[/color]";
            case StrSty.SEC_TYPE:
                return $"[color={ColorMapperSecLvl(input)}]{input}[/color]";
            case StrSty.DEF_LVL:
                return $"[color={ColorMapperSecLvl(input)}]{input}[/color]";
            case StrSty.USERNAME:
                return $"[color={Util.CC(Cc.M)}]{input}[/color]";
            case StrSty.DESC:
                return $"[color={Util.CC(Cc.g)}]{input}[/color]";
            case StrSty.VARIABLE:
                return $"[color={Util.CC(Cc.LR)}]{input}[/color]";
            case StrSty.NUMBER:
                string number = addons.Length > 0 ? double.Parse(input).ToString($"F{addons[0]}") :
                    double.Parse(input).ToString("F2");
                return $"[color={Util.CC(Cc.C)}]{number}[/color]";
            case StrSty.DIR:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrSty.FILE: {
                    int lastSlash = input.LastIndexOf('/');
                    if (lastSlash == -1 || lastSlash == input.Length - 1)
                        return $"[color={Util.CC(Cc.G)}]{input}[/color]"; // fallback if no slash or only slash
                    return $"[color={Util.CC(Cc.C)}]{input[..(lastSlash + 1)]}[/color][color={Util.CC(Cc.G)}]{input[(lastSlash + 1)..]}[/color]";
                }
            case StrSty.CMD_FUL: {
                    string output = "";
                    string[] commands = SplitCommands(input);
                    for (int i = 0; i < commands.Length; ++i) {
                        string[] tokens = TokenizeCommand(commands[i]);
                        if (i > 0) output += $"[color={Util.CC(Cc.W)}];[/color]";

                        output += Util.Format(tokens[0], StrSty.CMD_ACT);
                        if (tokens.Length == 1) continue; // No args, just the command
                        output += " "; // Add space after command
                        for (int j = 1; j < tokens.Length; ++j) {
                            if (tokens[j].StartsWith('-')) output += Util.Format(tokens[j], StrSty.CMD_FLAG);
                            else output += Util.Format(tokens[j], StrSty.CMD_ARG);
                            if (j < tokens.Length - 1) output += " ";
                        }
                    }
                    return output;
                }
            case StrSty.CMD_FLAG:
                return $"[color={Util.CC(Cc.gR)}]{input}[/color]";
            case StrSty.CMD_ARG:
                return $"[color={Util.CC(Cc.LR)}]{input}[/color]";
            case StrSty.CMD_ACT:
                return $"[color={Util.CC(Cc.gB)}]{input}[/color]";
            case StrSty.CMD_FARG: {
                    string[] parts = input.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return string.Join(" ", parts.Select(static p =>
                        p.StartsWith('-') ? Util.Format(p, StrSty.CMD_FLAG) : Util.Format(p, StrSty.CMD_ARG)
                    ));
                }
            case StrSty.ERROR:
                return $"[color={Util.CC(Cc.R)}]{input}[/color]";
            case StrSty.HEADER:
                return $"[color={Util.CC(Cc.gR)}]{input}[/color]";
            case StrSty.MONEY: { // GC is short for Gold Coin. That's all. lol
                    if (string.IsNullOrWhiteSpace(input)) return "";
                    if (!double.TryParse(input, out double value))
                        return Util.Format("NUMBER_PARSE_FAILED", StrSty.ERROR);
                    string[] units = ["S", "Q", "T", "B", "M", "K"];
                    double[] divisors = [1e21, 1e15, 1e12, 1e9, 1e6, 1e3];
                    Cc[] colors = [Cc.r, Cc.y, Cc.m, Cc.c, Cc.g, Cc.b];
                    string sb = ""; double remainder = value;
                    Cc numberColor = (addons.Length > 0 && addons[0].Length > 0) ? (Cc)int.Parse(addons[0]) : Cc.C;
                    for (int i = 0; i < units.Length; i++) {
                        if (remainder >= divisors[i]) {
                            int unitValue = (int)System.Math.Floor(remainder / divisors[i]);
                            remainder -= unitValue * divisors[i];
                            sb += $"[color={Util.CC(numberColor)}]{unitValue}[/color][color={Util.CC(colors[i])}]{units[i]}[/color]";
                        }
                    }
                    string gcValue;
                    string lastPartLength = (addons.Length > 1 && addons[1].Length > 0) ? addons[1] : "0";
                    gcValue = remainder.ToString($"F{lastPartLength}");
                    if (gcValue == "000") { gcValue = ""; }
                    return sb + $"[color={Util.CC(numberColor)}]{gcValue}[/color]{Util.Format("GC", StrSty.AUTO_KWORD)}";
                }
            case StrSty.T_MINERAL: {
                    if (string.IsNullOrWhiteSpace(input)) return "";
                    if (!double.TryParse(input, out double value))
                        return Util.Format("NUMBER_PARSE_FAILED", StrSty.ERROR);
                    int index = int.Parse(addons[0]);
                    ItemData profile = ItemCrafter.MINERALS[index];
                    string[] units = ["S", "Q", "T", "B", "M", "K"];
                    double[] divisors = [1e21, 1e15, 1e12, 1e9, 1e6, 1e3];
                    Cc[] unitColors = [Cc.R, Cc.Y, Cc.M, Cc.G, Cc.B, Cc.C];
                    string mineralColor = "";
                    mineralColor = Util.CC(profile.ColorCode);
                    double remainder = value; string sb = "";
                    for (int i = 0; i < units.Length; i++) {
                        if (remainder >= divisors[i]) {
                            int unitValue = (int)Mathf.Floor(remainder / divisors[i]);
                            remainder -= unitValue * divisors[i];
                            sb += $"[color={Util.CC(Cc.w)}]{unitValue}[/color][color={Util.CC(unitColors[i])}]{units[i]}[/color]";
                        }
                    }
                    string gcValue;
                    gcValue = Mathf.Ceil(remainder).ToString("F0");
                    if (gcValue == "000") { gcValue = ""; }
                    return sb + $"[color={Util.CC(Cc.w)}]{gcValue}[/color][color={mineralColor}]{profile.Shorthand}[/color]";
                }
            case StrSty.COLORED_ITEM_NAME: {
                    if (!long.TryParse(addons[0], out long index)) return Util.Format("ITEM_INDEX_PARSE_FAILED", StrSty.ERROR);
                    if (index >= ItemCrafter.ALL_ITEMS.Length && index < 0) return Util.Format("ITEM_INDEX_NOT_EXISTS", StrSty.ERROR);
                    return $"[color={Util.CC(ItemCrafter.ALL_ITEMS[index].ColorCode)}]{input}[/color]";
                }
            case StrSty.WARNING:
                return $"[color={Util.CC(Cc.Y)}][WARNING] {input}[/color]";
            case StrSty.PART_SUCCESS:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrSty.FULL_SUCCESS:
                return $"[color={Util.CC(Cc.G)}]{input}[/color]";
            case StrSty.UNKNOWN_ERROR:
                return Util.Format($"Unknown error encountered{(addons.Length != 0 ? $" with {addons[0]}" : "")}. Error code: {input}", StrSty.ERROR);
            case StrSty.GAME_WINDOW:
                return $"[color={Util.CC(Cc.gR)}]{input}[/color]";
            case StrSty.CODE_MODULE:
                return $"[color={Util.CC(Cc.LB)}]{input}[/color]";
            case StrSty.AUTO_KWORD: {
                    if (input == "GC") return $"[color={Util.CC(Cc.Y)}]GC[/color]";
                    if (input == "g01d-pouch") return $"[color={Util.CC(Cc.Y)}]g01d-pouch[/color]";
                    return Util.Format("NOT_SUPPORTED", StrSty.ERROR);
                }
            case StrSty.NODE_LOCK:
                return $"[color={Util.CC(Cc.rG)}]{input}[/color]";
            default:
                return input;
        }

    }
    public static TEnum MapEnum<TEnum>(int value) where TEnum : System.Enum {
        System.Type enumType = typeof(TEnum);

        if (enumType == typeof(SecurityType)) {
            SecurityType result = value switch {
                < 1 => SecurityType.NOSEC,
                < 4 => SecurityType.LOSEC,
                < 8 => SecurityType.MISEC,
                < 10 => SecurityType.HISEC,
                >= 10 => SecurityType.MASEC,
            };

            return (TEnum)(object)result;
        }

        GD.PrintErr($"Enum type {enumType.Name} is not supported by MapEnum.");
        return default;
    }
    public static string LoadUnicodeArt(string path, bool escapeBBcode=false) {
        FileAccess fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string[] firstLine = StringExtensions.Split(fileAccess.GetLine(), " ", false);
        (int h, int w) = (firstLine[0].ToInt(), firstLine[1].ToInt());
        string art = "";
        for(int i = 0; i < h; ++i) {
            string line = fileAccess.GetLine();
            line = line[..Mathf.Min(w, line.Length)];
            art += line + "\n";
        }
        return art;
    }
    public static string EscapeBBCode(string input) { return input.Replace("[", "[lb]"); }
    private static readonly Regex bbcodeRegex = SelectBBCode();
    public static string RemoveBBCode(string input) { return bbcodeRegex.Replace(input, ""); }
    public static string[] SplitCommands(string input) {
        var commands = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '\0';
        bool escape = false;

        foreach (char c in input) {
            if (escape) {
                current.Append(c);
                escape = false;
            } else if (c == '\\') {
                escape = true;
            } else if (inQuotes) {
                if (c == quoteChar) {
                    inQuotes = false;
                }
                current.Append(c);
            } else {
                if (c == '"' || c == '\'') {
                    inQuotes = true;
                    quoteChar = c;
                    current.Append(c);
                } else if (c == ';') {
                    // semicolon outside quotes means split here
                    var cmd = current.ToString().Trim();
                    if (cmd.Length > 0)
                        commands.Add(cmd);
                    current.Clear();
                } else {
                    current.Append(c);
                }
            }
        }

        var lastCmd = current.ToString().Trim();
        if (lastCmd.Length > 0)
            commands.Add(lastCmd);

        return [.. commands];
    }
    public static string[] TokenizeCommand(string input) {
        var args = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '\0';
        bool escape = false;

        foreach (char c in input) {
            if (escape) {
                current.Append(c);
                escape = false;
            } else if (c == '\\') {
                escape = true;
            } else if (inQuotes) {
                if (c == quoteChar) {
                    inQuotes = false;
                } else {
                    current.Append(c);
                }
            } else {
                if (char.IsWhiteSpace(c)) {
                    if (current.Length > 0) {
                        args.Add(current.ToString());
                        current.Clear();
                    }
                } else if (c == '"' || c == '\'') {
                    inQuotes = true;
                    quoteChar = c;
                } else {
                    current.Append(c);
                }
            }
        }

        if (current.Length > 0)
            args.Add(current.ToString());

        return [.. args];
    }
    public static string GenerateStringByProportions(double[] proportions, Cc[] colorCode, int length) {
        if (proportions.Length != colorCode.Length)
            throw new System.ArgumentException("Proportions and characters must be of the same length.");

        int[] counts = new int[proportions.Length];
        int assigned = 0;

        // First pass: calculate initial counts using floor
        for (int i = 0; i < proportions.Length; i++) {
            counts[i] = (int)System.Math.Floor(proportions[i] * length);
            assigned += counts[i];
        }

        // Distribute the remainder
        int remainder = length - assigned;

        // Get decimal parts and their indices
        double[] decimalParts = new double[proportions.Length];
        for (int i = 0; i < proportions.Length; i++) {
            decimalParts[i] = (proportions[i] * length) - counts[i];
        }

        // Assign remaining characters based on largest decimal parts
        while (remainder > 0) {
            int maxIndex = 0;
            for (int i = 1; i < decimalParts.Length; i++) {
                if (decimalParts[i] > decimalParts[maxIndex]) {
                    maxIndex = i;
                }
            }

            counts[maxIndex]++;
            decimalParts[maxIndex] = 0; // Mark as used
            remainder--;
        }

        // Build final string
        StringBuilder sb = new StringBuilder(length);
        for (int i = 0; i < counts.Length; i++) {
            sb.Append($"[color={Util.CC(colorCode[i])}]" + new string('█', counts[i]) + "[/color]");
        }

        return sb.ToString();
    }
    public static string TimeDifferenceFriendly(double time) {
        int min = (int)(time / 60);
        if (min < 1) return $"Less than {Util.Format("1", StrSty.UNIT, "min")}";
        return $"Aprox.{Util.Format($"{min}", StrSty.UNIT, "min")} remaining";
    }
    public static Lock LockEnumMapper(LockType type) {
        return type switch {
            LockType.I4X => new I4X(),
            LockType.F8X => new F8X(),
            LockType.I16 => new I16(),
            LockType.P16X => new P16X(),
            LockType.P90 => new P90(),
            LockType.M2 => new M2(),
            LockType.M3 => new M3(),
            LockType.C0 => new C0(),
            LockType.C1 => new C1(),
            LockType.C3 => new C3(),
            _ => throw new System.NotImplementedException($"Lock type {type} is not implemented.")
        };
    }
    public static readonly NodeData PLAYER_NODE_DATA_DEFAULT = new([], [], [],
        [.. System.Enum.GetValues(typeof(LockType)).Cast<int>()], NodeType.PLAYER, "home",
            "Home", 1, 1, 1, 1);
    public static string GetArg(Dictionary<string, string> dict, params string[] aliases) => aliases.FirstOrDefault(flag => dict.ContainsKey(flag)) is string found ? dict[found] : null;
    public static bool ContainKeys(Dictionary<string, string> dict, params string[] aliases) => aliases.Any(flag => dict.ContainsKey(flag));
    public static string GenerateRandomString(int length, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") {
        StringBuilder result = new StringBuilder(length);
        for (int i = 0; i < length; i++) {
            result.Append(chars[GD.RandRange(0, chars.Length - 1)]);
        }
        return result.ToString();
    }
    public static uint GetFn1vHash(byte[] input) {
        const uint FNV_prime = 0x01000193;
        uint hash = 0x811C9DC5;
        foreach (byte b in input)
            hash = (hash ^ b) * FNV_prime;
        return hash;
    }
    public static string GetFnv1aTimeHash() {
        uint hash = Util.GetFn1vHash(BitConverter.GetBytes(Util.GetFn1vHash(BitConverter.GetBytes(Util.GetFn1vHash(BitConverter.GetBytes(Time.GetUnixTimeFromSystem()))))));
        return Convert.ToBase64String(BitConverter.GetBytes(hash)).TrimEnd('=');
    }
    public static string InvertHexColor(string hex) {
        // Remove the '#' if it's there
        hex = hex.TrimStart('#');

        // Parse RGB components
        int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

        // Invert each component
        int rInv = 255 - r;
        int gInv = 255 - g;
        int bInv = 255 - b;

        // Return inverted color as hex string
        return $"#{rInv:X2}{gInv:X2}{bInv:X2}";
    }
    public static string ColorMapperSecLvl(object lvl) {
        return $"{lvl switch {
            "NOSEC" or SecurityType.NOSEC or 0 or "0" => Util.CC(Cc.G),
            "LOSEC" or SecurityType.LOSEC or 1 or 2 or 3 or "1" or "2" or "3" => Util.CC(Cc.LB),
            "MISEC" or SecurityType.MISEC or 4 or 5 or 6 or "4" or "5" or "6" => Util.CC(Cc.y),
            "HISEC" or SecurityType.HISEC or 7 or 8 or 9 or "7" or "8" or "9" => Util.CC(Cc.r),
            "MASEC" or SecurityType.MASEC or "10" => Util.CC(Cc.m),
            _ => Util.CC(Cc.m)
        }}";
    }

    [GeneratedRegex(@"\[(\/?)(b|i|u|color|size|url|img|quote|code|spoiler|lb|rb|br)[^\]]*\]", RegexOptions.IgnoreCase, "en-150")]
    private static partial Regex SelectBBCode();
}