using Hogs.RPG.Core.Entities.RecipeObjects;

public static class Tier5MythicRecipes
{
    public static readonly Recipe WorldbreakerBlade = new()
    {
        Id = "worldbreaker_blade",
        Name = "Worldbreaker Blade",
        ResultItem = "worldbreaker_blade",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "titan_blade", 1 },
            { "abyss_claw", 800 },
            { "colossus_antler", 600 },
            { "shadow_claw", 300 },
            { "titan_antler", 250 },
            { "saber_relic", 10 },
            { "griffin_core", 10 },
            { "storm_relic", 10 },
            { "ancient_core", 10 },
            { "mythic_heart", 10 }
        }
    };

    public static readonly Recipe ColossusShield = new()
    {
        Id = "colossus_shield",
        Name = "Colossus Shield",
        ResultItem = "colossus_shield",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
    {
        { "titan_shield", 1 },

        { "colossus_antler", 600 },
        { "mythic_hide", 450 },

        // carry over
        { "titan_antler", 250 },
        { "ancient_hide", 200 },

        // full gate (but lighter than weapon)
        { "ancient_core", 8 },
        { "storm_relic", 8 },
        { "mythic_heart", 8 }
    }
    };

    public static readonly Recipe BeastslayerPlate = new()
    {
        Id = "beastslayer_plate",
        Name = "Beastslayer Plate",
        ResultItem = "beastslayer_plate",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "champion_plate", 1 },
            { "mythic_hide", 500 },
            { "ancient_hide", 300 },
            { "mythic_heart", 8 }
        }
    };

    public static readonly Recipe MythicCrown = new()
    {
        Id = "mythic_crown",
        Name = "Mythic Crown",
        ResultItem = "mythic_crown",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "champion_helm", 1 },
            { "colossus_antler", 350 },
            { "mythic_hide", 200 }
        }
    };

    public static readonly Recipe ColossusLegguards = new()
    {
        Id = "colossus_legguards",
        Name = "Colossus Legguards",
        ResultItem = "colossus_legguards",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "champion_greaves", 1 },
            { "mythic_hide", 420 }
        }
    };

    public static readonly Recipe SkystriderBoots = new()
    {
        Id = "skystrider_boots",
        Name = "Skystrider Boots",
        ResultItem = "skystrider_boots",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "shadowstep_boots", 1 },
            { "sky_talon", 280 },
            { "mythic_hide", 180 }
        }
    };

    public static readonly Recipe AbyssGauntlets = new()
    {
        Id = "abyss_gauntlets",
        Name = "Abyss Gauntlets",
        ResultItem = "abyss_gauntlets",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "storm_gauntlets", 1 },
            { "abyss_claw", 300 }
        }
    };

    public static readonly Recipe RavenKingBand = new()
    {
        Id = "raven_king_band",
        Name = "Raven King Band",
        ResultItem = "raven_king_band",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "storm_ring", 1 },
            { "death_feather", 300 },
            { "sky_relic", 6 },
            { "mythic_heart", 6 }
        }
    };

    public static readonly Recipe PendantOfTheWild = new()
    {
        Id = "pendant_of_the_wild",
        Name = "Pendant of the Wild",
        ResultItem = "pendant_of_the_wild",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "void_pendant", 1 },
            { "death_feather", 280 },
            { "sky_talon", 150 },
            { "sky_relic", 6 }
        }
    };
}