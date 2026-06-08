using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.GameLoopObjects;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.GameData.Hunts
{
    public static class AlchemyTargets
    {
        // =========================
        // Lv 1+ — Common swamp enemy
        // Drops: Serpent Venom
        // =========================
        public static readonly HuntTarget SwampSerpent = new()
        {
            Id = "swamp_serpent",
            Name = "Swamp Serpent",
            Icon = "🐍",
            DropItem = "serpent_venom",
            MinXP = 5,
            MaxXP = 10,
            MinDrop = 1,
            MaxDrop = 2,
            RequiredLevel = 1,
            Category = HuntCategory.Alchemy,
            AlchemyXpReward = 8
        };

        // =========================
        // Lv 10+ — Uncommon golem
        // Drops: Alchemical Core
        // =========================
        public static readonly HuntTarget CorruptedGolem = new()
        {
            Id = "corrupted_golem",
            Name = "Corrupted Golem",
            Icon = "🪨",
            DropItem = "alchemical_core",
            MinXP = 7,
            MaxXP = 12,
            MinDrop = 1,
            MaxDrop = 2,
            RequiredLevel = 10,
            Category = HuntCategory.Alchemy,
            AlchemyXpReward = 15
        };

        // =========================
        // Lv 20+ — Rare wraith
        // Drops: Ethereal Dust
        // =========================
        public static readonly HuntTarget ShadowWraith = new()
        {
            Id = "shadow_wraith",
            Name = "Shadow Wraith",
            Icon = "👻",
            DropItem = "ethereal_dust",
            MinXP = 12,
            MaxXP = 18,
            MinDrop = 1,
            MaxDrop = 2,
            RequiredLevel = 20,
            Category = HuntCategory.Alchemy,
            AlchemyXpReward = 25
        };

        // =========================
        // Lv 30+ — Very rare boss-tier
        // Drops: Philosopher's Stone
        // =========================
        public static readonly HuntTarget ElderAlchemist = new()
        {
            Id = "elder_alchemist",
            Name = "Elder Alchemist",
            Icon = "🧙",
            DropItem = "philosophers_stone",
            MinXP = 18,
            MaxXP = 25,
            MinDrop = 1,
            MaxDrop = 1,
            RequiredLevel = 30,
            Category = HuntCategory.Alchemy,
            AlchemyXpReward = 40
        };
    }
}