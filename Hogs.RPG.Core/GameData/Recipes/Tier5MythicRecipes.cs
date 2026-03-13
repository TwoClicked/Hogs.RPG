namespace Hogs.RPG.Core.GameData.Recipes;

using Hogs.RPG.Core.Entities;

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
            { "abyss_claw", 50 },
            { "colossus_antler", 35 }
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
            { "colossus_antler", 50 },
            { "mythic_hide", 35 }
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
            { "mythic_hide", 60 },
            { "ancient_hide", 30 }
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
            { "colossus_antler", 40 },
            { "mythic_hide", 25 }
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
            { "mythic_hide", 55 }
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
            { "sky_talon", 40 },
            { "mythic_hide", 25 }
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
            { "abyss_claw", 40 }
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
            { "death_feather", 45 }
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
            { "death_feather", 40 },
            { "sky_talon", 20 }
        }
    };
}