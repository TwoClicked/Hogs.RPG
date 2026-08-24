namespace Hogs.RPG.Core.Entities.DungeonObjects
{
    public class DungeonDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int RequiredLevel { get; set; }

        public int Floors { get; set; }

        public DungeonBossDefinition Boss { get; set; }

        public int BaseEnemyHealth { get; set; }

        public int EnemyHealthScaling { get; set; }

        public int BaseEnemyAttack { get; set; }

        public int EnemyAttackScaling { get; set; }

        // ✅ Separates pet dungeons from gear dungeons in autocomplete
        public bool IsPetDungeon { get; set; } = false;

        // 🔨 Multiplies gold/XP/pet XP on completion. Default 1.0 (unchanged
        // behavior for every existing dungeon). The lvl 36/38/40 enhancement
        // gate dungeons use 2.0.
        public double RewardMultiplier { get; set; } = 1.0;
    }
}