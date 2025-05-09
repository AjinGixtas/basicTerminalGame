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
}