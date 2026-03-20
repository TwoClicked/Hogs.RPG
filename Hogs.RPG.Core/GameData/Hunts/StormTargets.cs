namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

public static class StormTargets
{
    public static readonly HuntTarget StormEagle = new()
    {
        Id = "storm_eagle",
        Name = "Storm Eagle",
        Icon = "🦅",
        DropItem = "storm_feather",
        MinXP = 40,
        MaxXP = 60,
        MinGold = 20,
        MaxGold = 40,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 15,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget AncientBear = new()
    {
        Id = "ancient_bear",
        Name = "Ancient Bear",
        Icon = "🐻",
        DropItem = "ancient_hide",
        MinXP = 42,
        MaxXP = 62,
        MinGold = 22,
        MaxGold = 42,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 15,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget NightStalker = new()
    {
        Id = "night_stalker",
        Name = "Night Stalker",
        Icon = "🐆",
        DropItem = "shadow_claw",
        MinXP = 40,
        MaxXP = 58,
        MinGold = 20,
        MaxGold = 38,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 15,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget TitanElk = new()
    {
        Id = "titan_elk",
        Name = "Titan Elk",
        Icon = "🦌",
        DropItem = "titan_antler",
        MinXP = 42,
        MaxXP = 60,
        MinGold = 22,
        MaxGold = 40,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 15,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget VoidRaven = new()
    {
        Id = "void_raven",
        Name = "Void Raven",
        Icon = "🐦‍⬛",
        DropItem = "void_feather",
        MinXP = 40,
        MaxXP = 58,
        MinGold = 20,
        MaxGold = 38,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 15,
        Category = HuntCategory.Normal
    };
}