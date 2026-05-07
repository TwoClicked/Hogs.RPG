using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Shop
{
    public static class VikingRiseShopItems
    {
        private const ulong VikingRiseRole = 1222668156271591485;

        // =========================
        // Resources
        // =========================

        public static readonly ShopItemDefinition Food = new()
        {
            Id = "vr_food",
            Name = "100M Food",
            Description = "100 Million Food delivered to your Viking Rise city.",
            Icon = "🌾",
            Category = ShopCategory.VikingRiseResources,
            Type = ShopItemType.FixedPrice,
            Price = 25_000,
            RequiredRoleId = VikingRiseRole
        };

        public static readonly ShopItemDefinition Wood = new()
        {
            Id = "vr_wood",
            Name = "100M Wood",
            Description = "100 Million Wood delivered to your Viking Rise city.",
            Icon = "🪵",
            Category = ShopCategory.VikingRiseResources,
            Type = ShopItemType.FixedPrice,
            Price = 25_000,
            RequiredRoleId = VikingRiseRole
        };

        public static readonly ShopItemDefinition Stone = new()
        {
            Id = "vr_stone",
            Name = "100M Stone",
            Description = "100 Million Stone delivered to your Viking Rise city.",
            Icon = "🪨",
            Category = ShopCategory.VikingRiseResources,
            Type = ShopItemType.FixedPrice,
            Price = 30_000,
            RequiredRoleId = VikingRiseRole
        };

        public static readonly ShopItemDefinition Gold = new()
        {
            Id = "vr_gold",
            Name = "100M Gold",
            Description = "100 Million Gold delivered to your Viking Rise city.",
            Icon = "💰",
            Category = ShopCategory.VikingRiseResources,
            Type = ShopItemType.FixedPrice,
            Price = 40_000,
            RequiredRoleId = VikingRiseRole
        };

        public static readonly ShopItemDefinition SkipBank = new()
        {
            Id = "vr_skip_bank",
            Name = "Skip Bank Payment",
            Description = "Skip your bank payment this week.",
            Icon = "🏦",
            Category = ShopCategory.VikingRiseResources,
            Type = ShopItemType.FixedPrice,
            Price = 25_000,
            RequiredRoleId = VikingRiseRole
        };

        public static readonly ShopItemDefinition ClearFines = new()
        {
            Id = "vr_clear_fines",
            Name = "Clear Fines",
            Description = "Clear all your active fines.(DOES NOT APPLY TO VIKING REIGN FINES)",
            Icon = "📜",
            Category = ShopCategory.VikingRiseResources,
            Type = ShopItemType.FixedPrice,
            Price = 50_000,
            RequiredRoleId = VikingRiseRole
        };

        public static readonly ShopItemDefinition SkipVd = new()
        {
            Id = "vr_skip_vd",
            Name = "Skip VD Bracelets",
            Description = "Skip Vengeful Determination bracelets this cycle.",
            Icon = "📿",
            Category = ShopCategory.VikingRiseResources,
            Type = ShopItemType.FixedPrice,
            Price = 50_000,
            RequiredRoleId = VikingRiseRole
        };

        // =========================
        // Rank Spots (Auction)
        // =========================

        public static readonly ShopItemDefinition Rank11To20 = new()
        {
            Id = "vr_rank_11_20",
            Name = "Rank Spot 11–20",
            Description = "Auction for a Viking Rise rank spot between 11 and 20.",
            Icon = "🥉",
            Category = ShopCategory.VikingRiseRanks,
            Type = ShopItemType.Auction,
            StartingBid = 100_000,
            RequiredRoleId = VikingRiseRole
        };

        public static readonly ShopItemDefinition Rank6To10 = new()
        {
            Id = "vr_rank_6_10",
            Name = "Rank Spot 6–10",
            Description = "Auction for a Viking Rise rank spot between 6 and 10.",
            Icon = "🥈",
            Category = ShopCategory.VikingRiseRanks,
            Type = ShopItemType.Auction,
            StartingBid = 150_000,
            RequiredRoleId = VikingRiseRole
        };

        public static readonly ShopItemDefinition Rank4To5 = new()
        {
            Id = "vr_rank_4_5",
            Name = "Rank Spot 4–5",
            Description = "Auction for a Viking Rise rank spot between 4 and 5.",
            Icon = "🥇",
            Category = ShopCategory.VikingRiseRanks,
            Type = ShopItemType.Auction,
            StartingBid = 200_000,
            RequiredRoleId = VikingRiseRole
        };
    }
}