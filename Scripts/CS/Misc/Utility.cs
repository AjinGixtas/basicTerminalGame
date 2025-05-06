using Godot;
using System.Collections.Generic;
public static class Utilitiy {
    public static T[] Shuffle<T>(T[] array) {
        for (int i = 0; i < array.Length; i++) {
            int j = GD.RandRange(0, array.Length - 1); 
            (array[i], array[i]) = (array[j], array[i]); 
        }
        return array;
    }
    public static List<T> Shuffle<T>(List<T> array) {
        for (int i = 0; i < array.Count; i++) {
            int j = GD.RandRange(0, array.Count- 1);
            (array[i], array[i]) = (array[j], array[i]);
        }
        return array;
    }
}