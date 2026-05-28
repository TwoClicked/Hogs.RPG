using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.Raids
{
    public static class MaterialConversionData
    {
        public const int MaterialsPerKey = 2500;

        // Maps material item ID → raid key tier
        public static readonly IReadOnlyDictionary<string, int> MaterialToKeyTier =
            new Dictionary<string, int>
            {
                // Tier 1 — Forest
                { "fur",             1 },
                { "leather",         1 },
                { "bone",            1 },
                { "feather",         1 },
                { "claws",           1 },
                { "monster_blood",   1 },

                // Tier 2 — Wild
                { "hide",            2 },
                { "fang",            2 },
                { "talon",           2 },
                { "horn",            2 },
                { "sharp_claw",      2 },

                // Tier 3 — Deep
                { "saber_fang",      3 },
                { "griffin_feather", 3 },
                { "giant_antler",    3 },
                { "thick_hide",      3 },
                { "dark_feather",    3 },

                // Tier 4 — Storm
                { "storm_feather",   4 },
                { "ancient_hide",    4 },
                { "shadow_claw",     4 },
                { "titan_antler",    4 },
                { "void_feather",    4 },

                // Tier 5 — Mythic
                { "mythic_hide",     5 },
                { "sky_talon",       5 },
                { "abyss_claw",      5 },
                { "colossus_antler", 5 },
                { "death_feather",   5 },
            };

        public static readonly IReadOnlyDictionary<int, string> TierKeyNames =
            new Dictionary<int, string>
            {
                { 1, "Scorched Lair Key" },
                { 2, "Stronghold Key" },
                { 3, "Fortress Key" },
                { 4, "Citadel Key" },
                { 5, "World Boss Key" },
            };
    }
}