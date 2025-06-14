using Godot;

public class MainModule {
    /// <summary>
    /// Says the given content to the terminal processor.
    /// 
    /// </summary>
    /// <param name="content">The string to be said.</param>
    public void Say(string content) {
		ShellCore.SayM(content);
	}
    /// <summary>
    /// Clears the terminal processor.
    /// </summary>
    public void Clear() {
        ShellCore.Clear();
    }
    /// <summary>
    /// Sets the username of the player.
    /// </summary>
    /// <param name="username">The new username.</param>
    public void SetUsername(string username) {
        ShellCore.SetUsername(username);
    }
}
