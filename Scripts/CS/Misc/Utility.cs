using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
public static partial class Util {
    public const bool HaveFinalWord = false; // Used to prevent final word in the terminal
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
    // CC stands for Color Code, shortened for cleaner string format.
    // Cc also stands for Color Code, shortened and have the second 'C' to lowercase for distinguish them apart.
    public static string CC(Cc color) {
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

            Cc.gB => "#00aaff",    // Aqua blue
            Cc.rB => "#aa00ff",    // Violet
            Cc.rG => "#aaff00",    // Lime green
            Cc.bG => "#00ffaa",    // Mint green
            Cc.bR => "#ff00aa",    // Hot pink
            Cc.gR => "#ffaa00",    // Amber

            Cc.LB => "#6666ff",    // Periwinkle
            Cc.LG => "#66ff66",    // Light green
            Cc.LC => "#adffff",    // Light cyan
            Cc.LR => "#ff6666",    // Light coral
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
    public static string Format(string input, StrType type, params string[] addons) {
        switch (type) {
            case StrType.DECOR:
                return $"[color={Util.CC(Cc.w)}]{input}[/color]";
            case StrType.HOSTNAME:
                return $"[color={Util.CC(Cc.G)}]{input}[/color]";
            case StrType.DISPLAY_NAME:
                return $"[color={Util.CC(Cc.y)}]{input}[/color]";
            case StrType.DESC:
                return $"[color={Util.CC(Cc.g)}]{input}[/color]";
            case StrType.SYMBOL:
                return $"[color={Util.CC(Cc.m)}]{input}[/color]";
            case StrType.UNIT: {
                    if (string.IsNullOrWhiteSpace(input)) return "";
                    return $"{Util.Format(input, StrType.NUMBER)}[color={Util.CC(Cc.W)}]{addons[0]}[/color]";
                }
            case StrType.SEC_LVL: {
                    return $"[color={input switch {
                        "0" => Util.CC(Cc.C),
                        "1" or "2" or "3" => Util.CC(Cc.B),
                        "4" or "5" or "6" => Util.CC(Cc.Y),
                        "7" or "8" or "9" => Util.CC(Cc.R),
                        "10" => Util.CC(Cc.M),
                        _ => Util.CC(Cc.M)
                    }}]{input}[/color]";
                }
            case StrType.SEC_TYPE: {
                    return $"[color={input switch {
                        "NOSEC" => Util.CC(Cc.C),
                        "LOSEC" => Util.CC(Cc.B),
                        "MISEC" => Util.CC(Cc.Y),
                        "HISEC" => Util.CC(Cc.R),
                        "MASEC" => Util.CC(Cc.M),
                        _ => Util.CC(Cc.M)
                    }}]{input}[/color]";
                }
            case StrType.DEF_LVL:
                return $"[color={input switch {
                    "0" => Util.CC(Cc.C),
                    "1" or "2" or "3" => Util.CC(Cc.B),
                    "4" or "5" or "6" => Util.CC(Cc.Y),
                    "7" or "8" or "9" => Util.CC(Cc.R),
                    "10" => Util.CC(Cc.M),
                    _ => Util.CC(Cc.M)

                }}]{input}[/color]";
            case StrType.NUMBER:
                string number = addons.Length > 0 ? double.Parse(input).ToString($"F{addons[0]}") :
                    double.Parse(input).ToString("F2");
                return $"[color={Util.CC(Cc.C)}]{number}[/color]";
            case StrType.IP:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrType.DIR:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrType.FILE: {
                    int lastSlash = input.LastIndexOf('/');
                    if (lastSlash == -1 || lastSlash == input.Length - 1)
                        return $"[color={Util.CC(Cc.G)}]{input}[/color]"; // fallback if no slash or only slash
                    return $"[color={Util.CC(Cc.C)}]{input[..(lastSlash + 1)]}[/color][color={Util.CC(Cc.G)}]{input[(lastSlash + 1)..]}[/color]";
                }
            case StrType.CMD_FUL: {
                    string output = "";
                    string[] commands = SplitCommands(input);
                    for (int i = 0; i < commands.Length; ++i) {
                        string[] tokens = TokenizeCommand(commands[i]);
                        if (i > 0) output += $"[color={Util.CC(Cc.W)}];[/color]";

                        output += Util.Format(tokens[0], StrType.CMD_ACT);
                        if (tokens.Length == 1) continue; // No args, just the command
                        output += " "; // Add space after command
                        for (int j = 1; j < tokens.Length; ++j) {
                            if (tokens[j].StartsWith('-')) output += Util.Format(tokens[j], StrType.CMD_FLAG);
                            else output += Util.Format(tokens[j], StrType.CMD_ARG);
                            if (j < tokens.Length - 1) output += " ";
                        }
                    }
                    return output;
                }
            case StrType.ERROR:
                return $"[color={Util.CC(Cc.R)}]{input}[/color]";
            case StrType.HEADER:
                return $"[color={Util.CC(Cc.gR)}]{input}[/color]";
            case StrType.MONEY: { // GC is short for Gold Coin. That's all. lol
                    if (string.IsNullOrWhiteSpace(input)) return "";
                    if (!double.TryParse(input, out double value))
                        return Util.Format("NUMBER_PARSE_FAILED", StrType.ERROR);
                    string[] units =    ["S", "Q" , "T" , "B" , "M" , "K" ];
                    double[] divisors = [1e21, 1e15, 1e12, 1e9 , 1e6 , 1e3 ];
                    Cc[] colors =       [Cc.R, Cc.Y, Cc.M, Cc.G, Cc.B, Cc.C];
                    string sb = ""; double remainder = value;
                    for (int i = 0; i < units.Length; i++) {
                        if (remainder >= divisors[i]) {
                            int unitValue = (int)Mathf.Floor(remainder / divisors[i]);
                            remainder -= unitValue * divisors[i];
                            sb += $"[color={Util.CC(Cc.w)}]{unitValue}[/color][color={Util.CC(colors[i])}]{units[i]}[/color]";
                        }
                    }
                    string gcValue;
                    if (value > 10_000) {
                        // Round up to the next integer if value is big
                        gcValue = Mathf.Ceil(remainder).ToString("F0");
                        if (gcValue == "000") { gcValue = ""; }
                    } else {
                        // Keep two decimals for small values
                        gcValue = remainder.ToString("N2").Replace(",", "");
                        if (gcValue == "000.00") { gcValue = ""; }
                    }
                    return sb + $"[color={Util.CC(Cc.w)}]{gcValue}[/color][color={Util.CC(Cc.Y)}]GC[/color]";
                }
            case StrType.T_MINERAL: {
                    if (string.IsNullOrWhiteSpace(input)) return "";
                    if (!double.TryParse(input, out double value))
                        return Util.Format("NUMBER_PARSE_FAILED", StrType.ERROR);
                    int index = int.Parse(addons[0]);
                    MineralProfile profile = MINERAL_PROFILES[index];
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
            case StrType.G_MINERAL: { // General mineral formatting
                    if (!int.TryParse(addons[0], out int index))
                        return Util.Format("MINERAL_TYPE_INDEX_PARSE_FAILED", StrType.ERROR);
                    return $"[color={Util.CC(MINERAL_PROFILES[index].ColorCode)}]{input}[/color]";
                }

            case StrType.USERNAME:
                return $"[color={Util.CC(Cc.M)}]{input}[/color]";
            case StrType.WARNING:
                return $"[color={Util.CC(Cc.Y)}][WARNING] {input}[/color]";
            case StrType.PART_SUCCESS:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrType.FULL_SUCCESS:
                return $"[color={Util.CC(Cc.G)}]{input}[/color]";
            case StrType.CMD_FLAG:
                return $"[color={Util.CC(Cc.gR)}]{input}[/color]";
            case StrType.CMD_ARG:
                return $"[color={Util.CC(Cc.w)}]{input}[/color]";
            case StrType.CMD_ACT:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrType.UNKNOWN_ERROR:
                return Util.Format($"Unknown error encountered{(addons.Length != 0 ? $" with {addons[0]}" : "")}. Error code: {input}", StrType.ERROR);
            case StrType.SECTOR:
                return $"[color={Util.CC(Cc.W)}]{input}[/color]";
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
    public static string EscapeBBCode(string code) { return code.Replace("[", "[lb]"); }
    public static readonly MineralProfile[] MINERAL_PROFILES =
    [.. Enumerable.Range(0, 10).Select(i =>
            ResourceLoader.Load<MineralProfile>(
                $"res://Utilities/Resources/MineralTypes/MineralResources/MineralT{i}.tres"
                )
            ).Where(m => m != null)
    ];

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
            throw new ArgumentException("Proportions and characters must be of the same length.");

        int[] counts = new int[proportions.Length];
        int assigned = 0;

        // First pass: calculate initial counts using floor
        for (int i = 0; i < proportions.Length; i++) {
            counts[i] = (int)Math.Floor(proportions[i] * length);
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
        if (min < 1) return $"Less than {Util.Format("1", StrType.UNIT, "min")}";
        return $"Aprox.{Util.Format($"{min}", StrType.UNIT, "min")} remaining";
    }
}