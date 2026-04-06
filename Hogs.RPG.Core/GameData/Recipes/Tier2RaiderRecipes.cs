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
            { "claw_dagger", 1 },
            { "fang", 120 },
            { "sharp_claw", 80 },
            { "claws", 60 },
            { "bone", 30 }
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
            { "leather_vest", 1 },
            { "hide", 120 },
            { "leather", 60 },
            { "fur", 40 }
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
            { "bone_helm", 1 },
            { "horn", 80 },
            { "hide", 60 }
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
            { "hunter_leggings", 1 },
            { "hide", 100 }
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
            { "hide_boots", 1 },
            { "hide", 80 },
            { "leather", 40 }
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
            { "fur_gloves", 1 },
            { "hide", 70 }
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
        { "bone_buckler", 1 },

        { "horn", 120 },
        { "hide", 100 },

        // carry over
        { "bone", 40 },
        { "leather", 30 }
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
            { "raven_charm", 1 },
            { "talon", 80 },
            { "feather", 40 }
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
            { "feather_band", 1 },
            { "talon", 100 },
            { "fang", 40 }
        }
    };
}