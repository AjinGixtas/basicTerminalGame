using Godot;
using System.Collections.Generic;
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
            Cc.___ => "#000000",   // Black (no intensity)
            Cc.rgb => "#282828",   // Dim gray, more distinct from white
            Cc.RGB => "#ffffff",   // Full white

            Cc.R => "#ff0000",     // Red (unchanged)
            Cc.G => "#00ff00",     // Green (unchanged)
            Cc.B => "#0000ff",     // Blue (unchanged)
            Cc.C => "#00ffff",     // Cyan (unchanged)
            Cc.M => "#ff00ff",     // Magenta (unchanged)
            Cc.Y => "#ffff00",     // Yellow (unchanged)

            Cc.r => "#cc0000",     // Brighter half red
            Cc.g => "#00cc00",     // Brighter half green
            Cc.b => "#0000cc",     // Brighter half blue
            Cc.c => "#005C5C",     // Brighter cyan (half green + half blue)
            Cc.m => "#cc00cc",     // Brighter magenta (half red + half blue)
            Cc.y => "#cccc00",     // Brighter yellow (half red + half green)

            Cc.gB => "#00aaff",    // Strong green-blue (cyan-leaning)
            Cc.rB => "#aa00ff",    // Strong red-blue (violet-leaning)
            Cc.rG => "#aaff00",    // Strong red-green (lime-leaning)
            Cc.bG => "#00ffaa",    // Strong green with touch of blue
            Cc.bR => "#ff00aa",    // Strong red with touch of blue
            Cc.gR => "#ffaa00",    // Strong orange (red + green)

            Cc.LB => "#6666ff",    // Softer but clear blue tint
            Cc.LG => "#66ff66",    // Softer green tint
            Cc.LC => "#66ffff",    // Softer cyan tint
            Cc.LR => "#ff6666",    // Softer red tint
            Cc.LM => "#ff66ff",    // Softer magenta tint
            Cc.LY => "#ffff66",    // Softer yellow tint

            _ => "#ffffff",        // Default: white
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
}