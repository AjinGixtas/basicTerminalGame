using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
public static class Util {
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
                    return $"[color={Util.CC(Cc.rgb)}]{integerPart}.{decimalPart}[/color][color={Util.CC(Cc.RGB)}]{addons[0]}[/color]";
                }
            case StrType.SEC_LVL: {
                    string colorCode = "";
                    if (addons.Length > 0) {
                        colorCode = addons[0] switch {
                            "0" or "1" or "2" or "3" => Util.CC(Cc.B),
                            "4" or "5" or "6" or "7" => Util.CC(Cc.Y),
                            "8" or "9" or "10" => Util.CC(Cc.R),
                            _ => "_"
                        };
                    }
                    if (colorCode == "_" || colorCode == "") {
                        colorCode = input switch {
                            "0" or "1" or "2" or "3" => Util.CC(Cc.B),
                            "4" or "5" or "6" or "7" => Util.CC(Cc.Y),
                            "8" or "9" or "10" => Util.CC(Cc.R),
                            _ => Util.CC(Cc.R)
                        };
                    }
                    return $"[color={colorCode}]{input}[/color]";
                }
            case StrType.DEF_LVL:
                return $"[color={input switch { 
                    "NOSEC" or "LOSEC" => Util.CC(Cc.R), 
                    "MISEC" => Util.CC(Cc.Y), 
                    "HISEC" or "MASEC" => Util.CC(Cc.B),
                    // Fall back
                    "0" or "1" or "2" or "3" => Util.CC(Cc.B),
                    "4" or "5" or "6" or "7" => Util.CC(Cc.Y),
                    "8" or "9" or "10" => Util.CC(Cc.R),
                    // Final backup
                    _ => Util.CC(Cc.R) }
                }]{input}[/color]";
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

                    string dir = input[..(lastSlash + 1)];
                    string file = input[(lastSlash + 1)..];

                    return $"[color={Util.CC(Cc.C)}]{dir}[/color][color={Util.CC(Cc.G)}]{file}[/color]";
                }
            case StrType.CMD: {
                string output = "";
                string[] commands = input.Split(';');
                for (int i = 0; i < commands.Length; i++) {
                    if (i > 0) { output += ';'; }
                    if (commands[i].Length == 0) { continue; }

                    string[] tokens = commands[i].Split(' '); bool firstToken = true;
                    for (int j = 0; j < tokens.Length; ++j) {
                        if (j > 0) { output += ' '; }
                        if (tokens[j].Length == 0) { continue; }

                        if (firstToken) { output += $"[color={Util.CC(Cc.C)}]{tokens[j]}[/color]"; firstToken = false; } else if (tokens[j].StartsWith('-')) { output += $"[color={Util.CC(Cc.gR)}]{tokens[j]}[/color]"; } else { output += $"[color={Util.CC(Cc.rgb)}]{tokens[j]}[/color]"; }
                    }
                }
                return output;
            }
            case StrType.ERROR:
                return $"[color={Util.CC(Cc.R)}]{input}[/color]";
            case StrType.HEADER:
                return $"[color={Util.CC(Cc.gR)}]{input}[/color]";
            case StrType.MONEY: {
                    if (string.IsNullOrWhiteSpace(input)) return "";

                    // Parse the input as a decimal number
                    if (!double.TryParse(input, out double value))
                        return $"[color={Util.CC(Cc.y)}]{input}[/color][color={Util.CC(Cc.y)}]GC[/color]";

                    // Define units and their values
                    string[] units =    ["Q" , "T" , "B" , "M" , "K" ];
                    double[] divisors = [1e15, 1e12, 1e9 , 1e6 , 1e3 ];
                    Cc[] colors =       [Cc.R, Cc.M, Cc.G, Cc.B, Cc.C]; // Q, T, B, M, K

                    StringBuilder sb = new();
                    double remainder = value;

                    for (int i = 0; i < units.Length; i++) {
                        if (remainder >= divisors[i]) {
                            int unitValue = (int)Math.Floor(remainder / divisors[i]);
                            remainder -= unitValue * divisors[i];
                            sb.Append($"[color={Util.CC(Cc.rgb)}]{unitValue}[/color][color={Util.CC(colors[i])}]{units[i]}[/color]");
                        }
                    }

                    string gcValue;
                    if (value > 10_000) {
                        // Round up to the next integer if value is big
                        gcValue = Math.Ceiling(remainder).ToString("F0");
                        if (gcValue == "000") { gcValue = ""; }
                    } else {
                        // Keep two decimals for small values
                        gcValue = remainder.ToString("N2").Replace(",", "");
                        if (gcValue == "000.00") { gcValue = ""; }
                    }
                    sb.Append($"[color={Util.CC(Cc.rgb)}]{gcValue}[/color]");
                    sb.Append($"[color={Util.CC(Cc.Y)}]GC[/color]");
                    return sb.ToString();
                }
            case StrType.USERNAME:
                return $"[color={Util.CC(Cc.M)}]{input}[/color]";
            case StrType.WARNING:
                throw new Exception("Not implemented!");
            case StrType.PARTIAL_SUCCESS:
                return $"[color={Util.CC(Cc.C)}]{input}[/color]";
            case StrType.FULL_SUCCESS:
                return $"[color={Util.CC(Cc.G)}]{input}[/color]";
            case StrType.CMD_ARG:
                return $"[color={Util.CC(Cc.gR)}]{input}[/color]";
            default:
                return input;
        }
    }
    public static TEnum MapEnum<TEnum>(int securityPoint) where TEnum : Enum {
        Type enumType = typeof(TEnum);

        if (enumType == typeof(SecurityType)) {
            var result = securityPoint switch {
                < 1 => SecurityType.NOSEC,
                < 4 => SecurityType.LOSEC,
                < 7 => SecurityType.MISEC,
                < 10 => SecurityType.HISEC,
                _ => SecurityType.MASEC,
            };

            return (TEnum)(object)result;
        }

        throw new NotSupportedException($"Enum type {enumType.Name} is not supported by MapEnum.");
    }
}