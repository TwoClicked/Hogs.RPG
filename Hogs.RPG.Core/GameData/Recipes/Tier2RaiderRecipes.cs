namespace Hogs.RPG.Core.GameData.Recipes;

using Hogs.RPG.Core.Entities;

public static class Tier2RaiderRecipes
{
    public static readonly Recipe FangBlade = new()
    {
        Id = "fang_blade",
        Name = "Fang Blade",
        ResultItem = "fang_blade",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "fang", 30 },
            { "sharp_claw", 20 }
        }
    };

    public static readonly Recipe HornShield = new()
    {
        Id = "horn_shield",
        Name = "Horn Shield",
        ResultItem = "horn_shield",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "horn", 30 },
            { "hide", 20 }
        }
    };

    public static readonly Recipe HideWarcoat = new()
    {
        Id = "hide_warcoat",
        Name = "Hide Warcoat",
        ResultItem = "hide_warcoat",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "hide", 35 },
            { "leather", 15 }
        }
    };

    public static readonly Recipe RaiderHelm = new()
    {
        Id = "raider_helm",
        Name = "Raider Helm",
        ResultItem = "raider_helm",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "horn", 20 },
            { "hide", 15 }
        }
    };

    public static readonly Recipe RaiderLegguards = new()
    {
        Id = "raider_legguards",
        Name = "Raider Legguards",
        ResultItem = "raider_legguards",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "hide", 30 }
        }
    };

    public static readonly Recipe RaiderBoots = new()
    {
        Id = "raider_boots",
        Name = "Raider Boots",
        ResultItem = "raider_boots",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "hide", 20 },
            { "leather", 10 }
        }
    };

    public static readonly Recipe TrackerGloves = new()
    {
        Id = "tracker_gloves",
        Name = "Tracker Gloves",
        ResultItem = "tracker_gloves",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "hide", 18 }
        }
    };

    public static readonly Recipe RaiderBand = new()
    {
        Id = "raider_band",
        Name = "Raider Band",
        ResultItem = "raider_band",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "talon", 20 }
        }
    };

    public static readonly Recipe TalonCharm = new()
    {
        Id = "talon_charm",
        Name = "Talon Charm",
        ResultItem = "talon_charm",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "talon", 25 },
            { "fang", 10 }
        }
    };
}