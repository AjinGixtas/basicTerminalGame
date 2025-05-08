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
            // Common color get shortened to one letter
            Cc.R => "#ff0000",   // Neon Red
            Cc.G => "#00ff00",   // Neon Green
            Cc.B => "#0000ff",   // Neon Blue
            Cc.C => "#00ffff",   // Cyan
            Cc.M => "#ff00ff",   // Magenta
            Cc.Y => "#ffff00",   // Bright Neon Yellow
            Cc.___ => "#000000",   // Black
            Cc.RGB => "#ffffff",   // White

            // Named neon colors
            Cc.ORANGE => "#ff5e00",   // Bright neon orange (avoid dark orange tones)
            Cc.PINK => "#ff1493",   // Neon pink
            Cc.LIME => "#bfff00",   // Lime neon
            Cc.AQUA => "#00ffe0",   // Aqua neon
            Cc.VIOLET => "#8f00ff",   // Electric violet
            Cc.PURPLE => "#bf00ff",   // Bright neon purple
            Cc.TEAL => "#00f7a5",   // Bright teal
            Cc.INDIGO => "#4b0082",   // Deep neon indigo
            Cc.GOLD => "#ffd700",   // Bright gold (edge of neon spectrum)
            Cc.BROWN => "#a0522d",   // Not neon, but the brightest reasonable brown
            Cc.GRAY => "#aaaaaa",   // Neutral mid-gray (useful for backgrounds)
            Cc.NAVY => "#0000aa",   // Electric navy
            Cc.SKY => "#87cefa",   // Sky blue (bright, but less neon)

            // Optional fun extras
            Cc.HOTPINK => "#ff69b4",   // Hot pink
            Cc.LAVENDER => "#e066ff",   // Lavender glow
            Cc.MINT => "#98ff98",   // Mint green neon
            _ => "#000000"
        };
    }
}