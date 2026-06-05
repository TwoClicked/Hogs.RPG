namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities.GameLoopObjects;
using Hogs.RPG.Core.Enums;

public static class MythicTargets
{
    public static readonly HuntTarget MythicBear = new()
    {
        Id = "mythic_bear",
        Name = "Mythic Bear",
        Icon = "🐻",
        DropItem = "mythic_hide",
        MinXP = 20,
        MaxXP = 25,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 25,
        Category = HuntCategory.Normal,
        RareDropItem = "mythic_heart"
    };

    public static readonly HuntTarget SkyTyrant = new()
    {
        Id = "sky_tyrant",
        Name = "Sky Tyrant",
        Icon = "🦅",
        DropItem = "sky_talon",
        MinXP = 20,
        MaxXP = 25,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 25,
        Category = HuntCategory.Normal,
        RareDropItem = "sky_relic"
    };

    public static readonly HuntTarget AbyssStalker = new()
    {
        Id = "abyss_stalker",
        Name = "Abyss Stalker",
        Icon = "🐆",
        DropItem = "abyss_claw",
        MinXP = 20,
        MaxXP = 25,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 25,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget ColossusElk = new()
    {
        Id = "colossus_elk",
        Name = "Colossus Elk",
        Icon = "🦌",
        DropItem = "colossus_antler",
        MinXP = 20,
        MaxXP = 25,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 25,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget DeathRaven = new()
    {
        Id = "death_raven",
        Name = "Death Raven",
        Icon = "🐦‍⬛",
        DropItem = "death_feather",
        MinXP = 20,
        MaxXP = 25,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 25,
        Category = HuntCategory.Normal
    };
}