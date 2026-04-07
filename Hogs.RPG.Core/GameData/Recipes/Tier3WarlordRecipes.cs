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
            { "fang_blade", 1 },
            { "wolf_trophy", 5 },
            { "saber_fang", 250 },
            { "giant_antler", 180 },
            { "fang", 120 },
            { "sharp_claw", 100 },
            { "saber_relic", 2 }
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
            { "hide_warcoat", 1 },
            { "thick_hide", 180 },
            { "hide", 120 }
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
            { "raider_helm", 1 },
            { "giant_antler", 140 },
            { "thick_hide", 100 }
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
            { "raider_legguards", 1 },
            { "thick_hide", 150 }
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
            { "raider_boots", 1 },
            { "saber_fang", 140 },
            { "thick_hide", 100 }
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
            { "tracker_gloves", 1 },
            { "saber_fang", 130 }
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
            { "raider_band", 1 },
            { "griffin_feather", 120 },
            { "dark_feather", 60 }
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
            { "talon_charm", 1 },
            { "dark_feather", 140 },
            { "griffin_feather", 80 },
            { "griffin_core", 2 }
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
        { "horn_shield", 1 },

        { "giant_antler", 200 },
        { "thick_hide", 150 },

        // carry over
        { "horn", 100 },
        { "hide", 80 },

        { "saber_relic", 2 }
    }
    };
}