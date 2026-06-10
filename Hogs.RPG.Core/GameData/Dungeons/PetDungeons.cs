using Hogs.RPG.Core.Entities.DungeonObjects;
using Hogs.RPG.Core.GameData.DungeonBosses;

namespace Hogs.RPG.Core.GameData.Dungeons
{
    public static class PetDungeons
    {
        // =========================
        // ⚔️ ATTACK PET DUNGEON — Lv 15
        // Drops: Blazefang Raptor (3%)
        // =========================
        public static readonly DungeonDefinition BlazewingsGorge = new()
        {
            Id = "blazewings_gorge",
            Name = "Blazewing's Gorge",

            RequiredLevel = 15,
            Floors = 7,

            BaseEnemyHealth = 160,
            EnemyHealthScaling = 55,

            BaseEnemyAttack = 80,
            EnemyAttackScaling = 20,

            IsPetDungeon = true,

            Boss = PetDungeonBosses.ArmoredCapybara
        };

        // =========================
        // 🛡️ DEFENSE PET DUNGEON — Lv 20
        // Drops: Ironshell Boar (3%)
        // =========================
        public static readonly DungeonDefinition StonehallDepths = new()
        {
            Id = "stonehall_depths",
            Name = "Stonehall Depths",

            RequiredLevel = 20,
            Floors = 9,

            BaseEnemyHealth = 250,
            EnemyHealthScaling = 70,

            BaseEnemyAttack = 160,
            EnemyAttackScaling = 25,

            IsPetDungeon = true,

            Boss = PetDungeonBosses.ElTataDeFrog
        };

        // =========================
        // ❤️ HEALTH PET DUNGEON — Lv 25
        // Drops: Tidecall Serpent (3%)
        // =========================
        public static readonly DungeonDefinition DrownedArchives = new()
        {
            Id = "drowned_archives",
            Name = "Drowned Archives",

            RequiredLevel = 25,
            Floors = 11,

            BaseEnemyHealth = 360,
            EnemyHealthScaling = 90,

            BaseEnemyAttack = 210,
            EnemyAttackScaling = 30,

            IsPetDungeon = true,

            Boss = PetDungeonBosses.IceWolf
        };

        // =========================
        // 🧪 ALCHEMIST COMPANION DUNGEON — Lv 27
        // =========================
        public static readonly DungeonDefinition AbandonedAcademy = new()
        {
            Id = "abandoned_academy",
            Name = "The Abandoned Academy",
            RequiredLevel = 27,
            Floors = 12,
            BaseEnemyHealth = 420,
            EnemyHealthScaling = 100,
            BaseEnemyAttack = 240,
            EnemyAttackScaling = 33,
            IsPetDungeon = true,
            Boss = PetDungeonBosses.Bandit
        };

        // =========================
        // 🌿 GATHER COMPANION DUNGEON — Lv 29
        // =========================
        public static readonly DungeonDefinition AshenHollow = new()
        {
            Id = "ashen_hollow",
            Name = "The Ashen Hollow",
            RequiredLevel = 29,
            Floors = 13,
            BaseEnemyHealth = 490,
            EnemyHealthScaling = 110,
            BaseEnemyAttack = 270,
            EnemyAttackScaling = 36,
            IsPetDungeon = true,
            Boss = PetDungeonBosses.RavensOfOdin
        };

        // =========================
        // 🔨 BLACKSMITH COMPANION DUNGEON — Lv 31
        // =========================
        public static readonly DungeonDefinition EmberClankaVille = new()
        {
            Id = "ember_clankavile",
            Name = "Ember ClankaVille",
            RequiredLevel = 31,
            Floors = 14,
            BaseEnemyHealth = 560,
            EnemyHealthScaling = 120,
            BaseEnemyAttack = 300,
            EnemyAttackScaling = 39,
            IsPetDungeon = true,
            Boss = PetDungeonBosses.FurnyDaClanka
        };
    }
}