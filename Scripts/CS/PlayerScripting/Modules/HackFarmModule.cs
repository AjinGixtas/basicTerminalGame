public class HackFarmModule {
    public string[] ListBotnets() {
        return NetworkManager.GetBotsIP();
    }

    public double GetBotXferDelayValue(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        return bot == null ? -1 : bot.XferDelay;
    }
    public double GetBotMineSpeedValue(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        return bot == null ? -1 : bot.MineSpeed;
    }
    public double GetBotBatchSizeValue(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        return bot == null ? -1 : bot.BatchSize;
    }

    public double GetBotXferDelayCost(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        return bot == null ? -1 : bot.GetXferDelayCost();
    }
    public double GetBotMineSpeedCost(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        return bot == null ? -1 : bot.GetMineSpeedCost();
    }
    public double GetBotBatchSizeCost(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        return bot == null ? -1 : bot.GetBatchSizeCost();
    }

    public int GetBotXferDelayLevel(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        return bot == null ? -1 : bot.XferDelayLVL;
    }
    public int GetBotMineSpeedLevel(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        return bot == null ? -1 : bot.MineSpeedLVL;
    }
    public int GetBotBatchSizeLevel(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        return bot == null ? -1 : bot.BatchSizeLVL;
    }

    public bool UpgradeBotXferDelay(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        if (bot == null) return false;
        return bot.UpgradeTime() == 0;
    }
    public bool UpgradeBotMineSpeed(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        if (bot == null) return false;
        return bot.UpgradeGrow() == 0;
    }
    public bool UpgradeBotBatchSize(string botIP) {
        BotFarm bot = NetworkManager.GetBotByIP(botIP);
        if (bot == null) return false;
        return bot.UpgradeHack() == 0;
    }
}