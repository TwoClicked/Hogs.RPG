using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.DungeonBosses;

namespace Hogs.RPG.Core.GameData.Dungeons
{
    public static class AllDungeons
    {
        // =========================
        // LEVEL 5 DUNGEON (NEW)
        // =========================
        public static readonly DungeonDefinition RotwoodHollow = new()
        {
            Id = "rotwood_hollow",
            Name = "Rotwood Hollow",
            RequiredLevel = 5,
            Floors = 4,
            BaseEnemyHealth = 70,
            EnemyHealthScaling = 30,
            BaseEnemyAttack = 35,
            EnemyAttackScaling = 10,
            Boss = DungeonBosses.DungeonBosses.RotFatherMalchor
        };

        // =========================
        // LEVEL 10 DUNGEON
        // =========================
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
            Floors = 7,
            BaseEnemyHealth = 160,
            EnemyHealthScaling = 55,
            BaseEnemyAttack = 65,
            EnemyAttackScaling = 20,
            Boss = DungeonBosses.DungeonBosses.Hrothgar
        };

        // =========================
        // LEVEL 17 DUNGEON
        // =========================
        public static readonly DungeonDefinition SunkenCathedral = new()
        {
            Id = "sunken_cathedral",
            Name = "Sunken Cathedral",
            RequiredLevel = 17,
            Floors = 8,
            BaseEnemyHealth = 190,
            EnemyHealthScaling = 62,
            BaseEnemyAttack = 120,
            EnemyAttackScaling = 22,
            Boss = DungeonBosses.DungeonBosses.AurelionTheOathbreakerPaladin
        };

        // =========================
        // LEVEL 18 DUNGEON
        // =========================
        public static readonly DungeonDefinition StarchyWastes = new()
        {
            Id = "starchy_wastes",
            Name = "The Starchy Wastes",
            RequiredLevel = 18,
            Floors = 8,
            BaseEnemyHealth = 210,
            EnemyHealthScaling = 65,
            BaseEnemyAttack = 130,
            EnemyAttackScaling = 23,
            Boss = DungeonBosses.DungeonBosses.AmphivosTaterous
        };

        // =========================
        // LEVEL 20 DUNGEON
        // =========================
        public static readonly DungeonDefinition SpiritForest = new()
        {
            Id = "spirit_forest",
            Name = "Spirit Forest",
            RequiredLevel = 20,
            Floors = 9,
            BaseEnemyHealth = 240,
            EnemyHealthScaling = 70,
            BaseEnemyAttack = 150,
            EnemyAttackScaling = 25,
            Boss = DungeonBosses.DungeonBosses.Luminara
        };

        // =========================
        // LEVEL 22 DUNGEON
        // =========================
        public static readonly DungeonDefinition CarnivalOfCarnage = new()
        {
            Id = "carnival_of_carnage",
            Name = "Carnival of Carnage",
            RequiredLevel = 22,
            Floors = 10,
            BaseEnemyHealth = 280,
            EnemyHealthScaling = 78,
            BaseEnemyAttack = 170,
            EnemyAttackScaling = 27,
            Boss = DungeonBosses.DungeonBosses.SkarrTheClownOfCarnage
        };

        // =========================
        // LEVEL 23 DUNGEON
        // =========================
        public static readonly DungeonDefinition GildedVault = new()
        {
            Id = "gilded_vault",
            Name = "The Gilded Vault",
            RequiredLevel = 23,
            Floors = 10,
            BaseEnemyHealth = 300,
            EnemyHealthScaling = 82,
            BaseEnemyAttack = 180,
            EnemyAttackScaling = 28,
            Boss = DungeonBosses.DungeonBosses.GrimblexTheGildedTyrant
        };

        // =========================
        // LEVEL 25 DUNGEON
        // =========================
        public static readonly DungeonDefinition TempleOfRuin = new()
        {
            Id = "temple_of_ruin",
            Name = "Temple of Ruin",
            RequiredLevel = 25,
            Floors = 11,
            BaseEnemyHealth = 350,
            EnemyHealthScaling = 90,
            BaseEnemyAttack = 200,
            EnemyAttackScaling = 30,
            Boss = DungeonBosses.DungeonBosses.ThorkellSonOfTyr
        };

        // =========================
        // LEVEL 27 DUNGEON
        // =========================
        public static readonly DungeonDefinition SparkiteMines = new()
        {
            Id = "sparkite_mines",
            Name = "Sparkite Mines",
            RequiredLevel = 27,
            Floors = 12,
            BaseEnemyHealth = 430,
            EnemyHealthScaling = 105,
            BaseEnemyAttack = 260,
            EnemyAttackScaling = 37,
            Boss = DungeonBosses.DungeonBosses.Gritch
        };
    }
}
