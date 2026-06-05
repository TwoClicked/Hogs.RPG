using Hogs.RPG.Core.Entities.RecipeObjects;

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
            { "saber_fang_blade", 1 },
            { "shadow_claw", 400 },
            { "titan_antler", 300 },
            { "saber_fang", 150 },
            { "giant_antler", 120 },
            { "saber_relic", 6 },
            { "griffin_core", 5 }
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
            { "warlord_armor", 1 },
            { "ancient_hide", 300 },
            { "thick_hide", 180 },
            { "ancient_core", 5 }
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
            { "warlord_helm", 1 },
            { "titan_antler", 220 },
            { "ancient_hide", 140 }
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
            { "warlord_greaves", 1 },
            { "ancient_hide", 260 }
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
        { "antler_shield", 1 },

        { "titan_antler", 300 },
        { "ancient_hide", 220 },

        // carry over
        { "giant_antler", 140 },
        { "thick_hide", 120 },

        { "ancient_core", 5 },
        { "saber_relic", 4 }
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
            { "sabertooth_boots", 1 },
            { "shadow_claw", 220 },
            { "ancient_hide", 140 }
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
            { "claw_gauntlets", 1 },
            { "shadow_claw", 200 }
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
            { "griffin_band", 1 },
            { "storm_feather", 180 },
            { "void_feather", 80 },
            { "storm_relic", 4 }
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
            { "raven_eye_pendant", 1 },
            { "void_feather", 200 },
            { "storm_feather", 120 },
            { "storm_relic", 4 }
        }
    };
}