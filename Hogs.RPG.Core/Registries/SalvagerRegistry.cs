using Hogs.RPG.Core.GameData.Equipment;
using System.Collections.Generic;

namespace Hogs.RPG.Core.GameData.Salvage
{
    // =========================
    // 🔨 SALVAGE REGISTRY
    // Lets players break down spare Global/Dungeon Boss Gear into
    // Blackstones. Only the two boss-gear sets are eligible — Tier 1-5
    // smithing gear already has its own economy (smelt/craft/sell), and
    // Hunter/Tracker gear is earned via Trail tokens, so neither is
    // included here.
    //
    // Values are intentionally NOT tied to enhancement level: enhancement
    // progress lives on the Player entity per-slot, not on the physical
    // item, so every copy of a given piece is worth the same regardless
    // of how enhanced the equipped one is.
    // =========================
    public static class SalvageRegistry
    {
        public const int GlobalBossGearValue = 250;
        public const int DungeonBossGearValue = 50;

        public static readonly Dictionary<string, int> BlackstoneValue = new()
        {
            // ===== Global Boss Gear =====
            { GlobalBossGear.AureliusSword.Id,    GlobalBossGearValue },
            { GlobalBossGear.XerathulArmor.Id,    GlobalBossGearValue },
            { GlobalBossGear.GravelmawShield.Id,  GlobalBossGearValue },
            { GlobalBossGear.SerpentGloves.Id,    GlobalBossGearValue },
            { GlobalBossGear.TyrHelm.Id,          GlobalBossGearValue },
            { GlobalBossGear.ThrolakLeggings.Id,  GlobalBossGearValue },
            { GlobalBossGear.PunisherRing.Id,     GlobalBossGearValue },
            { GlobalBossGear.GullveigAmulet.Id,   GlobalBossGearValue },
            { GlobalBossGear.SirRachaBoots.Id,    GlobalBossGearValue },

            // ===== Dungeon Boss Gear =====
            { DungeonBossGear.MalchorGrips.Id,        DungeonBossGearValue },
            { DungeonBossGear.FanculoHelm.Id,         DungeonBossGearValue },
            { DungeonBossGear.HrothgarRing.Id,        DungeonBossGearValue },
            { DungeonBossGear.OathcrushLegguards.Id,  DungeonBossGearValue },
            { DungeonBossGear.TaterousBattleaxe.Id,   DungeonBossGearValue },
            { DungeonBossGear.LuminaraAmulet.Id,      DungeonBossGearValue },
            { DungeonBossGear.SkarrSawbladeshield.Id, DungeonBossGearValue },
            { DungeonBossGear.ShadowsaphireSignet.Id, DungeonBossGearValue },
            { DungeonBossGear.ThorkellBoots.Id,       DungeonBossGearValue },
            { DungeonBossGear.GritchWarplate.Id,      DungeonBossGearValue },
        };
    }
}