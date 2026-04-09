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
            Description = "Open a crate for 20 rare material drops",
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

        public static readonly ShopItemDefinition PetRename = new()
        {
            Id = "discord_pet_rename",
            Name = "Pet Rename",
            Description = "Rename your currently equipped pet to anything you like.",
            Icon = "🐾",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 5_000
        };
    }
}