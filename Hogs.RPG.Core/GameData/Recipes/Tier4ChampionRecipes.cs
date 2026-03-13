namespace Hogs.RPG.Core.GameData.Recipes;

using Hogs.RPG.Core.Entities;

public static class Tier4ChampionRecipes
{
    public static readonly Recipe TitanBlade = new()
    {
        Id = "titan_blade",
        Name = "Titan Blade",
        ResultItem = "titan_blade",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "shadow_claw", 40 },
            { "titan_antler", 30 }
        }
    };

    public static readonly Recipe TitanShield = new()
    {
        Id = "titan_shield",
        Name = "Titan Shield",
        ResultItem = "titan_shield",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "titan_antler", 40 },
            { "ancient_hide", 30 }
        }
    };

    public static readonly Recipe ChampionPlate = new()
    {
        Id = "champion_plate",
        Name = "Champion Plate",
        ResultItem = "champion_plate",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "ancient_hide", 45 },
            { "thick_hide", 25 }
        }
    };

    public static readonly Recipe ChampionHelm = new()
    {
        Id = "champion_helm",
        Name = "Champion Helm",
        ResultItem = "champion_helm",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "titan_antler", 30 },
            { "ancient_hide", 20 }
        }
    };

    public static readonly Recipe ChampionGreaves = new()
    {
        Id = "champion_greaves",
        Name = "Champion Greaves",
        ResultItem = "champion_greaves",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "ancient_hide", 40 }
        }
    };

    public static readonly Recipe ShadowstepBoots = new()
    {
        Id = "shadowstep_boots",
        Name = "Shadowstep Boots",
        ResultItem = "shadowstep_boots",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "shadow_claw", 30 },
            { "ancient_hide", 20 }
        }
    };

    public static readonly Recipe StormGauntlets = new()
    {
        Id = "storm_gauntlets",
        Name = "Storm Gauntlets",
        ResultItem = "storm_gauntlets",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "shadow_claw", 28 }
        }
    };

    public static readonly Recipe StormRing = new()
    {
        Id = "storm_ring",
        Name = "Storm Ring",
        ResultItem = "storm_ring",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "storm_feather", 35 }
        }
    };

    public static readonly Recipe VoidPendant = new()
    {
        Id = "void_pendant",
        Name = "Void Pendant",
        ResultItem = "void_pendant",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "void_feather", 35 },
            { "storm_feather", 15 }
        }
    };
}