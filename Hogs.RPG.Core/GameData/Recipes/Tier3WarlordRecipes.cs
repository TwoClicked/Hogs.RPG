namespace Hogs.RPG.Core.GameData.Recipes;

using Hogs.RPG.Core.Entities;

public static class Tier3WarlordRecipes
{
    public static readonly Recipe SaberFangBlade = new()
    {
        Id = "saber_fang_blade",
        Name = "Saber Fang Blade",
        ResultItem = "saber_fang_blade",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "saber_fang", 35 },
            { "giant_antler", 25 }
        }
    };

    public static readonly Recipe AntlerShield = new()
    {
        Id = "antler_shield",
        Name = "Antler Shield",
        ResultItem = "antler_shield",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "giant_antler", 35 },
            { "thick_hide", 25 }
        }
    };

    public static readonly Recipe WarlordArmor = new()
    {
        Id = "warlord_armor",
        Name = "Warlord Armor",
        ResultItem = "warlord_armor",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "thick_hide", 40 },
            { "hide", 20 }
        }
    };

    public static readonly Recipe WarlordHelm = new()
    {
        Id = "warlord_helm",
        Name = "Warlord Helm",
        ResultItem = "warlord_helm",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "giant_antler", 25 },
            { "thick_hide", 20 }
        }
    };

    public static readonly Recipe WarlordGreaves = new()
    {
        Id = "warlord_greaves",
        Name = "Warlord Greaves",
        ResultItem = "warlord_greaves",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "thick_hide", 35 }
        }
    };

    public static readonly Recipe SabertoothBoots = new()
    {
        Id = "sabertooth_boots",
        Name = "Sabertooth Boots",
        ResultItem = "sabertooth_boots",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "saber_fang", 25 },
            { "thick_hide", 15 }
        }
    };

    public static readonly Recipe ClawGauntlets = new()
    {
        Id = "claw_gauntlets",
        Name = "Claw Gauntlets",
        ResultItem = "claw_gauntlets",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "saber_fang", 25 }
        }
    };

    public static readonly Recipe GriffinBand = new()
    {
        Id = "griffin_band",
        Name = "Griffin Band",
        ResultItem = "griffin_band",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "griffin_feather", 30 }
        }
    };

    public static readonly Recipe RavenEyePendant = new()
    {
        Id = "raven_eye_pendant",
        Name = "Raven Eye Pendant",
        ResultItem = "raven_eye_pendant",
        ResultAmount = 1,
        Materials = new Dictionary<string, int>
        {
            { "dark_feather", 30 },
            { "griffin_feather", 15 }
        }
    };
}