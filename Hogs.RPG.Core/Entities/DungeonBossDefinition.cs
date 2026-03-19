namespace Hogs.RPG.Core.Entities
{
    public class DungeonBossDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }

        public string AbilitiesText { get; set; }

        // Drop system
        public List<BossLoot> LootTable { get; set; } = new();
    }
}