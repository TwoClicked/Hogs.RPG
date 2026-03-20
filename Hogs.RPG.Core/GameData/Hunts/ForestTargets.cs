namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

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
        RequiredLevel = 1,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget Pig = new()
    {
        Id = "pig",
        Name = "Pig",
        Icon = "🐖",
        DropItem = "leather",
        MinXP = 12,
        MaxXP = 20,
        MinGold = 6,
        MaxGold = 14,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget Raccoon = new()
    {
        Id = "raccoon",
        Name = "Raccoon",
        Icon = "🦝",
        DropItem = "bone",
        MinXP = 11,
        MaxXP = 19,
        MinGold = 6,
        MaxGold = 13,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget Turkey = new()
    {
        Id = "turkey",
        Name = "Turkey",
        Icon = "🦃",
        DropItem = "feather",
        MinXP = 9,
        MaxXP = 16,
        MinGold = 4,
        MaxGold = 10,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget Fox = new()
    {
        Id = "fox",
        Name = "Fox",
        Icon = "🦊",
        DropItem = "claws",
        MinXP = 9,
        MaxXP = 15,
        MinGold = 4,
        MaxGold = 9,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1,
        Category = HuntCategory.Normal
    };

    // Sub item for Health Potions (Lowest level
    public static readonly HuntTarget BloodBoar = new()
    {
        Id = "blood_boar",
        Name = "Blood Boar",
        Icon = "🐗",
        MinXP = 28,
        MaxXP = 38,
        MinGold = 8,
        MaxGold = 14,
        DropItem = "monster_blood",
        MinDrop = 1,
        MaxDrop = 5,
        RequiredLevel = 1,
        Category = HuntCategory.Alchemy
    };
}