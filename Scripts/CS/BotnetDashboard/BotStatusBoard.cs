using Godot;
using System;

public partial class BotStatusBoard : Control {
    WeakReference<BotFarm> botFarmRef = new(null);
    public override void _Process(double delta) {
        botFarmRef.TryGetTarget(out BotFarm botFarm);

    }
    public void ChangeFocusedBotFarm(BotFarm bot) {
        if (botFarmRef.TryGetTarget(out BotFarm cur)) {
            if (cur == bot) { botFarmRef.SetTarget(null); return; }
        }
        botFarmRef.SetTarget(bot);
    }
}
