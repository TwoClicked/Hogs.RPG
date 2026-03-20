namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

public static class WildTargets
{
    public static readonly HuntTarget Bear = new()
    {
        Id = "bear",
        Name = "Bear",
        Icon = "🐻",
        DropItem = "hide",
        MinXP = 18,
        MaxXP = 28,
        MinGold = 10,
        MaxGold = 20,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 5,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget Shark = new()
    {
        Id = "shark",
        Name = "Shark",
        Icon = "🦈",
        DropItem = "fang",
        MinXP = 20,
        MaxXP = 30,
        MinGold = 10,
        MaxGold = 22,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 5,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget Eagle = new()
    {
        Id = "eagle",
        Name = "Eagle",
        Icon = "🦅",
        DropItem = "talon",
        MinXP = 18,
        MaxXP = 26,
        MinGold = 9,
        MaxGold = 20,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 5,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget Bull = new()
    {
        Id = "bull",
        Name = "Bull",
        Icon = "🐂",
        DropItem = "horn",
        MinXP = 20,
        MaxXP = 30,
        MinGold = 10,
        MaxGold = 24,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 5,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget Lynx = new()
    {
        Id = "lynx",
        Name = "Lynx",
        Icon = "🐆",
        DropItem = "sharp_claw",
        MinXP = 20,
        MaxXP = 28,
        MinGold = 10,
        MaxGold = 20,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 5,
        Category = HuntCategory.Normal
    };
}