using Godot;
public static class Utilitiy {
    public static T[] Shuffle<T>(T[] array) {
        for (int i = 0; i < array.Length; i++) {
            int j = GD.RandRange(0, array.Length - 1); 
            (array[i], array[i]) = (array[j], array[i]); 
        }
        return array;
    }
}