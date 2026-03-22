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

            Boss = FanculoBoss.Fanculo,

            Drops = new List<DungeonDrop>
{
            new DungeonDrop
            {
                ItemId = "fanculo_helm",
                ChancePercent = 1
            }
        }
        };

    }
}