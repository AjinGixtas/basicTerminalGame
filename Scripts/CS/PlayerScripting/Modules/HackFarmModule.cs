public class HackFarmModule {
    public string[] ListBotnets() {
        return NetworkManager.GetBotsIP();
    }

    public double? GetBotXferDelayValue(string botIP) {
        return NetworkManager.GetBotByIP(botIP)?.XferDelay;
    }
    public double? GetBotMineSpeedValue(string botIP) {
        return NetworkManager.GetBotByIP(botIP)?.MineSpeed;
    }
    public double? GetBotBatchSizeValue(string botIP) {
        return NetworkManager.GetBotByIP(botIP)?.BatchSize;
    }

    public double? GetBotXferDelayCost(string botIP) {
        return NetworkManager.GetBotByIP(botIP)?.GetXferDelayCost();
    }
    public double? GetBotMineSpeedCost(string botIP) {
        return NetworkManager.GetBotByIP(botIP)?.GetMineSpeedCost();
    }
    public double? GetBotBatchSizeCost(string botIP) {
        return NetworkManager.GetBotByIP(botIP)?.GetBatchSizeCost();
    }

    public int? GetBotXferDelayLevel(string botIP) {
        return NetworkManager.GetBotByIP(botIP)?.XferDelayLVL;
    }
    public int? GetBotMineSpeedLevel(string botIP) {
        return NetworkManager.GetBotByIP(botIP)?.MineSpeedLVL;
    }
    public int? GetBotBatchSizeLevel(string botIP) {
        return NetworkManager.GetBotByIP(botIP)?.BatchSizeLVL;
    }

    public CError UpgradeBotXferDelay(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        if (bot == null) return CError.NOT_FOUND;
        return bot.UpgradeTime();
    }
    public CError? UpgradeBotMineSpeed(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        if (bot == null) return CError.NOT_FOUND;
        return bot.UpgradeGrow();
    }
    public CError? UpgradeBotBatchSize(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        if (bot == null) return CError.NOT_FOUND;
        return bot.UpgradeHack();
    }
}