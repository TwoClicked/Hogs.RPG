namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities;

public static class DeepTargets
{
    public static readonly HuntTarget Sabertooth = new()
    {
        Id = "sabertooth",
        Name = "Sabertooth",
        Icon = "🐅",
        DropItem = "saber_fang",
        MinXP = 28,
        MaxXP = 40,
        MinGold = 14,
        MaxGold = 30,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 10
    };

    public static readonly HuntTarget Griffin = new()
    {
        Id = "griffin",
        Name = "Griffin",
        Icon = "🦅",
        DropItem = "griffin_feather",
        MinXP = 30,
        MaxXP = 42,
        MinGold = 15,
        MaxGold = 32,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 10
    };

    public static readonly HuntTarget WarElk = new()
    {
        Id = "war_elk",
        Name = "War Elk",
        Icon = "🦌",
        DropItem = "giant_antler",
        MinXP = 28,
        MaxXP = 40,
        MinGold = 14,
        MaxGold = 28,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 10
    };

    public static readonly HuntTarget DireBear = new()
    {
        Id = "dire_bear",
        Name = "Dire Bear",
        Icon = "🐻",
        DropItem = "thick_hide",
        MinXP = 30,
        MaxXP = 42,
        MinGold = 16,
        MaxGold = 32,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 10
    };

    public static readonly HuntTarget ShadowRaven = new()
    {
        Id = "shadow_raven",
        Name = "Shadow Raven",
        Icon = "🐦‍⬛",
        DropItem = "dark_feather",
        MinXP = 28,
        MaxXP = 38,
        MinGold = 14,
        MaxGold = 28,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 10
    };
}