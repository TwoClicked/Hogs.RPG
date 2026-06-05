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
    }
}