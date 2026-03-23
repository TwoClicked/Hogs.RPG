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

            RequiredLevel = 10,
            Floors = 5,

            BaseEnemyHealth = 100,
            EnemyHealthScaling = 40,

            BaseEnemyAttack = 45,
            EnemyAttackScaling = 15,

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