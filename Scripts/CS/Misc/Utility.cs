using Godot;
using System.Collections.Generic;
using System.Linq;
public static partial class Util {
    public static bool haveFinalWord = true; // Used to prevent final word in the terminal
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
            Cc.___ => "#000000",   // Black
            Cc.rgb => "#282828",   // Dark gray
            Cc.RGB => "#ffffff",   // White

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
            Cc.LC => "#66ffff",    // Light cyan
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
                return $"[color={Util.CC(Cc.rgb)}]{input}[/color]";
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
                    string[] parts = input.Split('.');
                    if (parts.Length < 2) { parts = [parts[0], "00"]; }
                    string integerPart = parts[0];
                    string decimalPart = parts[1].PadLeft(2, '0');
                    if (addons.Length > 0 && addons[0][0] != '/') decimalPart = parts[1][..2];
                    return $"[color={Util.CC(Cc.rgb)}]{integerPart}.{decimalPart}[/color][color={Util.CC(Cc.RGB)}]{addons[0]}[/color]";
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
                return $"[color={Util.CC(Cc.c)}]{input}[/color]";
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
                string[] commands = input.Split(';');
                for (int i = 0; i < commands.Length; i++) {
                    if (i > 0) { output += ';'; }
                    if (commands[i].Length == 0) { continue; }

                    string[] tokens = commands[i].Split(' '); bool firstToken = true;
                    for (int j = 0; j < tokens.Length; ++j) {
                        if (j > 0) { output += ' '; }
                        if (tokens[j].Length == 0) { continue; }

                        if (firstToken) { output += $"{Util.Format(tokens[j], StrType.CMD_CMD)}"; firstToken = false; } 
                        else if (tokens[j].StartsWith('-')) { output += $"{Util.Format(tokens[j], StrType.CMD_FLAG)}"; } 
                        else { output += $"{Util.Format(tokens[j], StrType.CMD_ARG)}"; }
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
                    if (!double.TryParse(input, out double value)) return $"[color={Util.CC(Cc.y)}]{input}[/color][color={Util.CC(Cc.y)}]GC[/color]";
                    string[] units =    ["S" , "Q" , "T" , "B" , "M" , "K" ];
                    double[] divisors = [1e21, 1e15, 1e12, 1e9 , 1e6 , 1e3 ];
                    Cc[] colors =       [Cc.R, Cc.Y, Cc.M, Cc.G, Cc.B, Cc.C];
                    string sb = ""; double remainder = value;
                    for (int i = 0; i < units.Length; i++) {
                        if (remainder >= divisors[i]) {
                            int unitValue = (int)Mathf.Floor(remainder / divisors[i]);
                            remainder -= unitValue * divisors[i];
                            sb += $"[color={Util.CC(Cc.rgb)}]{unitValue}[/color][color={Util.CC(colors[i])}]{units[i]}[/color]";
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
                    return sb + $"[color={Util.CC(Cc.rgb)}]{gcValue}[/color][color={Util.CC(Cc.Y)}]GC[/color]";
                }
            case StrType.MINERAL: {
                    if (string.IsNullOrWhiteSpace(input)) return "";
                    if (!double.TryParse(input, out double value))
                        return $"[color={Util.CC(Cc.y)}]{input}[/color][color={Util.CC(Cc.y)}]GC[/color]";
                    int index = int.Parse(addons[0]);
                    MineralProfile profile = MINERAL_PROFILES[index];
                    string[] units    = [ "S",  "Q",  "T",  "B",  "M",  "K"];
                    double[] divisors = [1e21, 1e15, 1e12,  1e9,  1e6,  1e3];
                    Cc[] unitColors   = [Cc.R, Cc.Y, Cc.M, Cc.G, Cc.B, Cc.C];
                    Cc[] mineralColor = [Cc.LG, Cc.C, Cc.bR, Cc.gR, Cc.RGB, Cc.LB, Cc.r, Cc.LC, Cc.M, Cc.Y];
                    for (int i = 0; i < units.Length; i++) {
                        if (value >= divisors[i]) {
                            double unitValue = value / divisors[i];
                            string formatted = unitValue.ToString("0.0");
                            return $"[color={Util.CC(Cc.rgb)}]{formatted}[/color][color={Util.CC(unitColors[i])}]{units[i]}[/color][color={Util.CC(mineralColor[index])}]{profile.Name}[/color]";
                        }
                    }
                    // If less than 1K, just show the number with GC
                    string formattedValue = Mathf.Floor(value).ToString("0.##");
                    return $"[color={Util.CC(Cc.rgb)}]{formattedValue}[/color][color={Util.CC(mineralColor[index])}]{profile.Name}[/color]";
                }
            case StrType.USERNAME:
                return $"[color={Util.CC(Cc.M)}]{input}[/color]";
            case StrType.WARNING:
                GD.PrintErr("Not implemented!");
                return "";
            case StrType.PART_SUCCESS:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrType.FULL_SUCCESS:
                return $"[color={Util.CC(Cc.G)}]{input}[/color]";
            case StrType.CMD_FLAG:
                return $"[color={Util.CC(Cc.gR)}]{input}[/color]";
            case StrType.CMD_ARG:
                return $"[color={Util.CC(Cc.rgb)}]{input}[/color]";
            case StrType.CMD_CMD:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrType.UNKNOWN_ERROR:
                return Util.Format($"Unknown error encountered{(addons.Length != 0 ? $" with {addons[0]}" : "")}. Error code: {input}", StrType.ERROR);
            case StrType.SECTOR:
                return $"[color={Util.CC(Cc.RGB)}]{input}[/color]";
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
}