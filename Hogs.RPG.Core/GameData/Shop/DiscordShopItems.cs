using Hogs.RPG.Core.Entities.JobObjects;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Shop
{
    public static class DiscordShopItems
    {
        // Placeholder role IDs — replace with real Discord role IDs once created
        private const ulong TitleRole = 1111111111111111111;
        private const ulong NameColorRole = 2222222222222222222;

        public static readonly ShopItemDefinition CustomTitle = new()
        {
            Id = "discord_title",
            Name = "Custom Title",
            Description = "Receive a custom title displayed on your RPG profile.",
            Icon = "🏷️",
            Category = ShopCategory.DiscordRewards,
            Type = ShopItemType.FixedPrice,
            Price = 10_000,
            RewardRoleId = TitleRole
        };

        public static readonly ShopItemDefinition NameColor = new()
        {
            Id = "discord_color",
            Name = "Custom Name Color",
            Description = "Get a unique name color role in the server.",
            Icon = "🎨",
            Category = ShopCategory.DiscordRewards,
            Type = ShopItemType.FixedPrice,
            Price = 15_000,
            RewardRoleId = NameColorRole
        };

    }
}