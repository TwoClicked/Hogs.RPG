using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Shop
{
    public static class DiscordShopItems
    {
        // Placeholder role IDs — replace with real Discord role IDs once created
        private const ulong TitleRole = 1111111111111111111;
        private const ulong NameColorRole = 2222222222222222222;
        private const ulong VipLoungeRole = 3333333333333333333;

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

        public static readonly ShopItemDefinition VipLounge = new()
        {
            Id = "discord_vip",
            Name = "VIP Lounge Access",
            Description = "Unlock access to the exclusive VIP channel.",
            Icon = "👑",
            Category = ShopCategory.DiscordRewards,
            Type = ShopItemType.FixedPrice,
            Price = 20_000,
            RewardRoleId = VipLoungeRole
        };

        public static readonly ShopItemDefinition Shoutout = new()
        {
            Id = "discord_shoutout",
            Name = "Server Shoutout",
            Description = "Get a bot announcement shoutout in the server.",
            Icon = "📣",
            Category = ShopCategory.DiscordRewards,
            Type = ShopItemType.FixedPrice,
            Price = 8_000
        };
    }
}