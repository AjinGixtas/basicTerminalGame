using Godot;

public class MainModule {
    /// <summary>
    /// Says the given content to the terminal processor.
    /// 
    /// </summary>
    /// <param name="content">The string to be said.</param>
    public void Say(string content) {
		TerminalProcessor.Say(content);
	}
}
