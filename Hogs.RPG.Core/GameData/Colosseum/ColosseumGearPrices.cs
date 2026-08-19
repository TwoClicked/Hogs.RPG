using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;

namespace Hogs.RPG.Core.GameData.Colosseum
{
    public static class ColosseumGearPrices
    {
        public const int Tier1Cost = 0;
        public const int Tier2Cost = 15;
        public const int Tier3Cost = 35;
        public const int Tier4Cost = 70;
        public const int Tier5Cost = 100;
        public const int DungeonBossGearCost = 140;
        public const int GlobalBossGearCost = 160;

        public static readonly Dictionary<string, int> ApCostByItemId = new()
        {
            { "bone_helm",         Tier1Cost },
            { "leather_vest",      Tier1Cost },
            { "leather_leggings",  Tier1Cost },
            { "fur_gloves",        Tier1Cost },
            { "hide_boots",        Tier1Cost },
            { "claw_dagger",       Tier1Cost },
            { "bone_buckler",      Tier1Cost },
            { "feather_band",      Tier1Cost },
            { "raven_charm",       Tier1Cost },

            { "raider_helm",       Tier2Cost },
            { "hide_warcoat",      Tier2Cost },
            { "raider_legguards",  Tier2Cost },
            { "tracker_gloves",    Tier2Cost },
            { "raider_boots",      Tier2Cost },
            { "fang_blade",        Tier2Cost },
            { "horn_shield",       Tier2Cost },
            { "raider_band",       Tier2Cost },
            { "talon_charm",       Tier2Cost },

            { "warlord_helm",        Tier3Cost },
            { "warlord_armor",       Tier3Cost },
            { "warlord_greaves",     Tier3Cost },
            { "claw_gauntlets",      Tier3Cost },
            { "sabertooth_boots",    Tier3Cost },
            { "saber_fang_blade",    Tier3Cost },
            { "antler_shield",       Tier3Cost },
            { "griffin_band",        Tier3Cost },
            { "raven_eye_pendant",   Tier3Cost },

            { "champion_helm",     Tier4Cost },
            { "champion_plate",    Tier4Cost },
            { "champion_greaves",  Tier4Cost },
            { "storm_gauntlets",   Tier4Cost },
            { "shadowstep_boots",  Tier4Cost },
            { "titan_blade",       Tier4Cost },
            { "titan_shield",      Tier4Cost },
            { "storm_ring",        Tier4Cost },
            { "void_pendant",      Tier4Cost },

            { "mythic_crown",         Tier5Cost },
            { "beastslayer_plate",    Tier5Cost },
            { "colossus_legguards",   Tier5Cost },
            { "abyss_gauntlets",      Tier5Cost },
            { "skystrider_boots",     Tier5Cost },
            { "worldbreaker_blade",   Tier5Cost },
            { "colossus_shield",      Tier5Cost },
            { "raven_king_band",      Tier5Cost },
            { "pendant_of_the_wild",  Tier5Cost },

            { "malchor_grips",          DungeonBossGearCost },
            { "fanculo_helm",           DungeonBossGearCost },
            { "hrothgar_ring",          DungeonBossGearCost },
            { "oathcrush_legguards",    DungeonBossGearCost },
            { "taterous_battleaxe",     DungeonBossGearCost },
            { "luminara_amulet",        DungeonBossGearCost },
            { "skarr_sawbladeshield",   DungeonBossGearCost },
            { "shadowsaphire_signet",   DungeonBossGearCost },
            { "thorkell_boots",         DungeonBossGearCost },
            { "gritch_warplate",        DungeonBossGearCost },

            { "aurelius_sword",     GlobalBossGearCost },
            { "xerathul_armor",     GlobalBossGearCost },
            { "gravelmaw_shield",   GlobalBossGearCost },
            { "serpent_gloves",     GlobalBossGearCost },
            { "tyr_helm",           GlobalBossGearCost },
            { "thorlak_leggings",   GlobalBossGearCost },
            { "punisher_ring",      GlobalBossGearCost },
            { "gullveig_amulet",    GlobalBossGearCost },
        };

        public static readonly Dictionary<EquipmentSlot, string> T1BaselineBySlot = new()
        {
            { EquipmentSlot.Helmet,   "bone_helm" },
            { EquipmentSlot.Body,     "leather_vest" },
            { EquipmentSlot.Legs,     "leather_leggings" },
            { EquipmentSlot.Gloves,   "fur_gloves" },
            { EquipmentSlot.Boots,    "hide_boots" },
            { EquipmentSlot.MainHand, "claw_dagger" },
            { EquipmentSlot.OffHand,  "bone_buckler" },
            { EquipmentSlot.Amulet,   "feather_band" },
            { EquipmentSlot.Ring,     "raven_charm" },
        };
    }
}