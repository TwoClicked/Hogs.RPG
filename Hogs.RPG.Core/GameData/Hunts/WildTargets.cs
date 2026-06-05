namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities.GameLoopObjects;
using Hogs.RPG.Core.Enums;

public static class WildTargets
{
    public static readonly HuntTarget Bear = new()
    {
        Id = "bear",
        Name = "Bear",
        Icon = "🐻",
        DropItem = "hide",
        MinXP = 7,
        MaxXP = 12,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 10,
        Category = HuntCategory.Normal,
        RareDropItem = "bear_heart"
    };

    public static readonly HuntTarget Shark = new()
    {
        Id = "shark",
        Name = "Shark",
        Icon = "🦈",
        DropItem = "fang",
        MinXP = 7,
        MaxXP = 12,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 10,
        Category = HuntCategory.Normal,
        RareDropItem = "alpha_fang"
    };

    public static readonly HuntTarget Eagle = new()
    {
        Id = "eagle",
        Name = "Eagle",
        Icon = "🦅",
        DropItem = "talon",
        MinXP = 7,
        MaxXP = 12,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 10,
        Category = HuntCategory.Normal,
        RareDropItem = "storm_talon"
    };

    public static readonly HuntTarget Bull = new()
    {
        Id = "bull",
        Name = "Bull",
        Icon = "🐂",
        DropItem = "horn",
        MinXP = 7,
        MaxXP = 12,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 10,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget Lynx = new()
    {
        Id = "lynx",
        Name = "Lynx",
        Icon = "🐆",
        DropItem = "sharp_claw",
        MinXP = 7,
        MaxXP = 12,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 10,
        Category = HuntCategory.Normal
    };
}