using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public static class PotionItems
    {
        public static readonly ItemDefinition XpPotion = new()
        {
            Id = "xp_potion",
            Name = "XP Potion",
            Icon = "🧪",
            Type = "Potion",
            Description = "Grants double experience on the next hunt."
        };

        public static readonly ItemDefinition HealthPotion = new()
        {
            Id = "health_potion",
            Name = "Health Potion",
            Icon = "❤️",
            Type = "Potion",
            Description = "Fully restores your health during encounters."
        };
    }
}