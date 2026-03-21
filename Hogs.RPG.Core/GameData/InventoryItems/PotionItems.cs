using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public static class PotionItems
    {
        public static readonly ItemDefinition XpPotion = new()
        {
            Id = "xp_potion",
            Name = "XP Potion",
            Icon = "<:xp_potion:1484990673164832768>",
            Type = "Potion",
            Description = "Grants double experience on the next hunt."
        };

        public static readonly ItemDefinition HealthPotion = new()
        {
            Id = "health_potion",
            Name = "Health Potion",
            Icon = "<:health_potion:1484990983845183702>",
            Type = "Potion",
            Description = "Fully restores your health during encounters."
        };
    }
}