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
            Price = 7_000
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
            Price = 8_000
        };

        public static readonly ShopItemDefinition StaminaReset = new()
        {
            Id = "rpg_stamina_reset",
            Name = "Stamina Reset",
            Description = "Instantly restore your hunt stamina to full.",
            Icon = "🏹",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 1_500
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

        public static readonly ShopItemDefinition EnergyRefill = new()
        {
            Id = "rpg_energy_refill",
            Name = "Energy Refill",
            Description = "Instantly restore your gathering energy to full.",
            Icon = "⚗️",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 1_000
        };

        public static readonly ShopItemDefinition PetSnackSmall = new()
        {
            Id = "rpg_pet_snack_small",
            Name = "Pet Snack — Small",
            Description = "Give your equipped pet a small snack for +50 XP.",
            Icon = "🍖",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 500
        };

        public static readonly ShopItemDefinition PetSnackMedium = new()
        {
            Id = "rpg_pet_snack_medium",
            Name = "Pet Snack — Medium",
            Description = "Give your equipped pet a hearty snack for +150 XP.",
            Icon = "🍗",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 1_000
        };

        public static readonly ShopItemDefinition PetSnackLarge = new()
        {
            Id = "rpg_pet_snack_large",
            Name = "Pet Snack — Large",
            Description = "Give your equipped pet a feast for +300 XP.",
            Icon = "🍖",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 1_500
        };

        public static readonly ShopItemDefinition DungeonReset = new()
        {
            Id = "rpg_dungeon_reset",
            Name = "Dungeon Reset",
            Description = "Clear your dungeon cooldown and enter again immediately.",
            Icon = "🏰",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 3_000
        };

        public static readonly ShopItemDefinition PetDungeonReset = new()
        {
            Id = "rpg_pet_dungeon_reset",
            Name = "Pet Dungeon Reset",
            Description = "Clear your pet dungeon cooldown and enter again immediately.",
            Icon = "🐾",
            Category = ShopCategory.RpgPerks,
            Type = ShopItemType.FixedPrice,
            Price = 3_000
        };
    }
}
