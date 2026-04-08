using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Shop
{
    public static class RpgPerkShopItems
    {
        public static readonly ShopItemDefinition DoubleXp = new()
        {
            Id = "rpg_double_xp",
            Name = "Double XP — 24h",
            Description = "Earn double XP from all hunts for 24 hours.",
            Icon = "✨",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 8_000
        };

        public static readonly ShopItemDefinition StaminaBoost = new()
        {
            Id = "rpg_stamina_boost",
            Name = "+50 Max Stamina — 7 days",
            Description = "Your maximum stamina is increased by 50 for 7 days.",
            Icon = "⚡",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 12_000
        };

        public static readonly ShopItemDefinition LootCrate = new()
        {
            Id = "rpg_loot_crate",
            Name = "Loot Crate",
            Description = "Open a crate for a random rare material drop.",
            Icon = "📦",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 6_000
        };

        public static readonly ShopItemDefinition StaminaReset = new()
        {
            Id = "rpg_stamina_reset",
            Name = "Stamina Reset",
            Description = "Instantly restore your hunt stamina to full.",
            Icon = "🏹",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 3_000
        };
    }
}