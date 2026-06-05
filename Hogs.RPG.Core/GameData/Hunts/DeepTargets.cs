namespace Hogs.RPG.GameData.Hunts;

using Hogs.RPG.Core.Entities.GameLoopObjects;
using Hogs.RPG.Core.Enums;
using System.Net.NetworkInformation;

public static class DeepTargets
{
    public static readonly HuntTarget Sabertooth = new()
    {
        Id = "sabertooth",
        Name = "Sabertooth",
        Icon = "🐅",
        DropItem = "saber_fang",
        MinXP = 10,
        MaxXP = 15,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 15,
        Category = HuntCategory.Normal,
        RareDropItem = "saber_relic"
    };

    public static readonly HuntTarget Griffin = new()
    {
        Id = "griffin",
        Name = "Griffin",
        Icon = "🦅",
        DropItem = "griffin_feather",
        MinXP = 10,
        MaxXP = 15,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 15,
        Category = HuntCategory.Normal,
        RareDropItem = "griffin_core"
    };

    public static readonly HuntTarget WarElk = new()
    {
        Id = "war_elk",
        Name = "War Elk",
        Icon = "🦌",
        DropItem = "giant_antler",
        MinXP = 10,
        MaxXP = 15,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 15,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget DireBear = new()
    {
        Id = "dire_bear",
        Name = "Dire Bear",
        Icon = "🐻",
        DropItem = "thick_hide",
        MinXP = 10,
        MaxXP = 15,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 15,
        Category = HuntCategory.Normal
    };

    public static readonly HuntTarget ShadowRaven = new()
    {
        Id = "shadow_raven",
        Name = "Shadow Raven",
        Icon = "🐦‍⬛",
        DropItem = "dark_feather",
        MinXP = 10,
        MaxXP = 15,
        MinDrop = 1,
        MaxDrop = 2,
        RequiredLevel = 15,
        Category = HuntCategory.Normal
    };


}