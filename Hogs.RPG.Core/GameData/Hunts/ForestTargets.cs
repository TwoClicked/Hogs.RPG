namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities;

public static class ForestTargets
{
    public static readonly HuntTarget Wolf = new()
    {
        Id = "wolf",
        Name = "Wolf",
        Icon = "🐺",
        DropItem = "fur",
        MinXP = 10,
        MaxXP = 18,
        MinGold = 5,
        MaxGold = 12,
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 1
    };

    public static readonly HuntTarget Boar = new()
    {
        Id = "boar",
        Name = "Boar",
        Icon = "🐗",
        DropItem = "leather",
        MinXP = 12,
        MaxXP = 20,
        MinGold = 6,
        MaxGold = 14,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1
    };

    public static readonly HuntTarget Stag = new()
    {
        Id = "stag",
        Name = "Stag",
        Icon = "🦌",
        DropItem = "bone",
        MinXP = 11,
        MaxXP = 19,
        MinGold = 6,
        MaxGold = 13,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1
    };

    public static readonly HuntTarget Raven = new()
    {
        Id = "raven",
        Name = "Raven",
        Icon = "🐦",
        DropItem = "feather",
        MinXP = 9,
        MaxXP = 16,
        MinGold = 4,
        MaxGold = 10,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1
    };

    public static readonly HuntTarget Fox = new()
    {
        Id = "fox",
        Name = "Fox",
        Icon = "🦊",
        DropItem = "Claws",
        MinXP = 9,
        MaxXP = 15,
        MinGold = 4,
        MaxGold = 9,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1
    };
}