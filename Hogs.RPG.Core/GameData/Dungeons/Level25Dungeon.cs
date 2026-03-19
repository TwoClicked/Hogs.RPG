using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.DungeonBosses;

namespace Hogs.RPG.Core.GameData.Dungeons
{
    public static class Level25Dungeon
    {
        public static readonly DungeonDefinition CryptOfFanculo = new()
        {
            Id = "crypt_fanculo",
            Name = "Crypt of the Wandering Viking",

            RequiredLevel = 25,
            Floors = 5,

            BaseEnemyHealth = 100,
            EnemyHealthScaling = 25,

            BaseEnemyAttack = 25,
            EnemyAttackScaling = 5,

            Boss = FanculoBoss.Fanculo
        };

        //public static readonly DungeonDefinition TyrDungeon = new()
        //{
        //    Id = "tyr_fallen_asgard",
        //    Name = "Fallen Asgard",

        //    RequiredLevel = 35,
        //    Floors = 6,

        //    BaseEnemyHealth = 160,
        //    EnemyHealthScaling = 35,

        //    BaseEnemyAttack = 40,
        //    EnemyAttackScaling = 8,

        //    Boss = TyrBoss.Tyr
        //};
    }
}