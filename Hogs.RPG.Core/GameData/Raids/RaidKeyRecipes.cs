using Hogs.RPG.Core.Entities.RecipeObjects;

namespace Hogs.RPG.Core.GameData.Raids
{
    public static class RaidKeyRecipes
    {
        public static readonly Recipe LairKey = new()
        {
            Id = "raid_key_t1",
            Name = "Lair Key",
            ResultItem = "raid_key_t1",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "fur", 30 },
                { "leather", 30 },
                { "bone", 30 },
                { "feather", 30 },
                { "claws", 30 },
                { "wolf_trophy", 3 }
            }
        };

        public static readonly Recipe StrongholdKey = new()
        {
            Id = "raid_key_t2",
            Name = "Stronghold Key",
            ResultItem = "raid_key_t2",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "hide", 30 },
                { "fang", 30 },
                { "talon", 30 },
                { "horn", 30 },
                { "sharp_claw", 30 },
                { "bear_heart", 3 },
                { "alpha_fang", 3 }
            }
        };

        public static readonly Recipe FortressKey = new()
        {
            Id = "raid_key_t3",
            Name = "Fortress Key",
            ResultItem = "raid_key_t3",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "saber_fang", 30 },
                { "griffin_feather", 30 },
                { "giant_antler", 30 },
                { "thick_hide", 30 },
                { "dark_feather", 30 },
                { "saber_relic", 3 },
                { "griffin_core", 3 }
            }
        };

        public static readonly Recipe CitadelKey = new()
        {
            Id = "raid_key_t4",
            Name = "Citadel Key",
            ResultItem = "raid_key_t4",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "storm_feather", 30 },
                { "ancient_hide", 30 },
                { "shadow_claw", 30 },
                { "titan_antler", 30 },
                { "void_feather", 30 },
                { "storm_relic", 3 },
                { "ancient_core", 3 }
            }
        };

        public static readonly Recipe WorldBossKey = new()
        {
            Id = "raid_key_t5",
            Name = "World Boss Key",
            ResultItem = "raid_key_t5",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "mythic_hide", 30 },
                { "sky_talon", 30 },
                { "abyss_claw", 30 },
                { "colossus_antler", 30 },
                { "death_feather", 30 },
                { "mythic_heart", 3 },
                { "sky_relic", 3 }
            }
        };
    }
}