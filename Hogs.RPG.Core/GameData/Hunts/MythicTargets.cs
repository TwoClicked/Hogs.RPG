namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

public static class MythicTargets
{
    public static readonly HuntTarget MythicBear = new()
    {
        Id = "mythic_bear",
        Name = "Mythic Bear",
        Icon = "🐻",
        DropItem = "mythic_hide",
        MinXP = 60,
        MaxXP = 90,
        MinGold = 30,
        MaxGold = 55,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 20,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget SkyTyrant = new()
    {
        Id = "sky_tyrant",
        Name = "Sky Tyrant",
        Icon = "🦅",
        DropItem = "sky_talon",
        MinXP = 60,
        MaxXP = 88,
        MinGold = 30,
        MaxGold = 50,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 20,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget AbyssStalker = new()
    {
        Id = "abyss_stalker",
        Name = "Abyss Stalker",
        Icon = "🐆",
        DropItem = "abyss_claw",
        MinXP = 62,
        MaxXP = 90,
        MinGold = 32,
        MaxGold = 55,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 20,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget ColossusElk = new()
    {
        Id = "colossus_elk",
        Name = "Colossus Elk",
        Icon = "🦌",
        DropItem = "colossus_antler",
        MinXP = 60,
        MaxXP = 88,
        MinGold = 30,
        MaxGold = 52,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 20,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget DeathRaven = new()
    {
        Id = "death_raven",
        Name = "Death Raven",
        Icon = "🐦‍⬛",
        DropItem = "death_feather",
        MinXP = 60,
        MaxXP = 85,
        MinGold = 30,
        MaxGold = 50,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 20,
        Category = HuntCategory.Normal
    };
}