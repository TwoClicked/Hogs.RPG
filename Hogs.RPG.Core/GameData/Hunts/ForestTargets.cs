namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities.GameLoopObjects;
using Hogs.RPG.Core.Enums;

public static class ForestTargets
{
    public static readonly HuntTarget Wolf = new()
    {
        Id = "wolf",
        Name = "Wolf",
        Icon = "🐺",
        DropItem = "fur",
        MinXP = 5,
        MaxXP = 10,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1,
        Category = HuntCategory.Normal,
        RareDropItem = "wolf_trophy"
    };

    public static readonly HuntTarget Pig = new()
    {
        Id = "pig",
        Name = "Pig",
        Icon = "🐖",
        DropItem = "leather",
        MinXP = 5,
        MaxXP = 10,
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
        MinXP = 5,
        MaxXP = 10,
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
        MinXP = 5,
        MaxXP = 10,
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
        MinXP = 5,
        MaxXP = 10,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 1,
        Category = HuntCategory.Normal,
    };

    // Sub item for Health Potions (Lowest level
    public static readonly HuntTarget BloodBoar = new()
    {
        Id = "blood_boar",
        Name = "Blood Boar",
        Icon = "🐗",
        MinXP = 5,
        MaxXP = 10,
        DropItem = "monster_blood",
        MinDrop = 1,
        MaxDrop = 3,
        RequiredLevel = 1,
        Category = HuntCategory.Alchemy
    };
}