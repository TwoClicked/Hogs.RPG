using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.DungeonBosses;

namespace Hogs.RPG.Core.GameData.Dungeons
{
    public static class AllDungeons // Rename when restarting, Will remain level 10
    {
        public static readonly DungeonDefinition CryptOfFanculo = new()
        {
            Id = "crypt_fanculo",
            Name = "Crypt of the Wandering Viking",

            RequiredLevel = 10,
            Floors = 5,

            BaseEnemyHealth = 100,
            EnemyHealthScaling = 40,

            BaseEnemyAttack = 45,
            EnemyAttackScaling = 15,

            Boss = DungeonBosses.DungeonBosses.Fanculo
        };

        // =========================
        // LEVEL 15 DUNGEON
        // =========================
        public static readonly DungeonDefinition ForsakenCatacombs = new()
        {
            Id = "forsaken_catacombs",
            Name = "Forsaken Catacombs",

            RequiredLevel = 15,
            Floors = 8,

            BaseEnemyHealth = 160,
            EnemyHealthScaling = 55,

            BaseEnemyAttack = 65,
            EnemyAttackScaling = 20,

            Boss = DungeonBosses.DungeonBosses.Hrothgar
        };

        // =========================
        // LEVEL 20 DUNGEON
        // =========================
        public static readonly DungeonDefinition SpiritForest = new()
        {
            Id = "spirit_forest",
            Name = "Spirit Forest",

            RequiredLevel = 20,
            Floors = 10,

            BaseEnemyHealth = 240,
            EnemyHealthScaling = 70,

            BaseEnemyAttack = 150,
            EnemyAttackScaling = 25,

            Boss = DungeonBosses.DungeonBosses.Luminara
        };

        // =========================
        // LEVEL 25 DUNGEON
        // =========================
        public static readonly DungeonDefinition TempleOfRuin = new()
        {
            Id = "temple_of_ruin",
            Name = "Temple of Ruin",

            RequiredLevel = 25,
            Floors = 12,

            BaseEnemyHealth = 350,
            EnemyHealthScaling = 90,

            BaseEnemyAttack = 200,
            EnemyAttackScaling = 30,

            Boss = DungeonBosses.DungeonBosses.ThorkellSonOfTyr
        };
    }
}