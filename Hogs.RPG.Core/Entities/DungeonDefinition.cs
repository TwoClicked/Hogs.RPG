namespace Hogs.RPG.Core.Entities
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
    }
}