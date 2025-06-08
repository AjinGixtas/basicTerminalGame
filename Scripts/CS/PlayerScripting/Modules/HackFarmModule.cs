public class HackFarmModule {
    public string[] ListBotnets() {
        return NetworkManager.GetBotnetNames();
    }
}