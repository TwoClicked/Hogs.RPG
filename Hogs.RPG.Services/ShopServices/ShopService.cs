using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Shop;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.PlayerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Hogs.RPG.Services.ShopServices
{
    public class ShopService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;

        private readonly ulong _feedChannelId = 1485357755433750549;
        private readonly ulong _adminRoleId = 1483528182106685691;

        public ShopService(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;
        }

        // =========================
        // BUY (FIXED PRICE)
        // =========================
        public async Task<(bool success, string message)> BuyAsync(ulong userId, string itemId, SocketGuild guild)
        {
            if (!ShopRegistry.All.TryGetValue(itemId, out var item))
                return (false, "Item not found.");

            // Role check
            if (item.RequiredRoleId.HasValue)
            {
                var member = guild.GetUser(userId);
                if (member == null || !member.Roles.Any(r => r.Id == item.RequiredRoleId.Value))
                    return (false, "❌ You do not have the required role to purchase this item.");
            }

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();

            var player = await playerRepo.GetByDiscordIdAsync(userId);

            if (player == null)
                return (false, "You need to start your adventure first.");

            if (player.Gold < item.Price)
                return (false, $"❌ You need **{item.Price:N0} gold** but only have **{player.Gold:N0}**.");

            // Deduct gold
            player.Gold -= item.Price;
            await playerRepo.UpdatePlayerAsync(player);

            // Log purchase
            await shopRepo.AddPurchaseAsync(new ShopPurchase
            {
                BuyerDiscordId = userId,
                ItemId = item.Id,
                ItemName = item.Name,
                GoldPaid = item.Price
            });

            // Post to feed
            await PostPurchaseFeedAsync(userId, item);

            return (true, $"✅ You purchased **{item.Icon} {item.Name}** for **{item.Price:N0} gold**!\nAn admin will fulfil your order soon.");
        }

        // =========================
        // START AUCTION
        // =========================
        public async Task<(bool success, string message, ActiveAuction auction)> StartAuctionAsync(
            ulong userId, string itemId, SocketGuild guild, ulong channelId)
        {
            if (!ShopRegistry.All.TryGetValue(itemId, out var item))
                return (false, "Item not found.", null);

            if (item.Type != Hogs.RPG.Core.Enums.ShopItemType.Auction)
                return (false, "This item is not an auction item.", null);

            // Role check
            if (item.RequiredRoleId.HasValue)
            {
                var member = guild.GetUser(userId);
                if (member == null || !member.Roles.Any(r => r.Id == item.RequiredRoleId.Value))
                    return (false, "❌ You do not have the required role to start this auction.", null);
            }

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();

            // Check no active auction for same item
            var existing = await shopRepo.GetActiveAuctionByItemAsync(itemId);
            if (existing != null)
                return (false, $"❌ There is already an active auction for **{item.Name}**.", null);

            var player = await playerRepo.GetByDiscordIdAsync(userId);

            if (player == null)
                return (false, "You need to start your adventure first.", null);

            if (player.Gold < item.StartingBid)
                return (false, $"❌ You need **{item.StartingBid:N0} gold** to start this auction but only have **{player.Gold:N0}**.", null);

            // Deduct starting bid (held in escrow)
            player.Gold -= item.StartingBid;
            await playerRepo.UpdatePlayerAsync(player);

            // Create auction in DB
            var auction = new ActiveAuction
            {
                ItemId = item.Id,
                StartedByDiscordId = userId,
                StartingBid = item.StartingBid,
                CurrentBid = item.StartingBid,
                CurrentBidderDiscordId = userId,
                ChannelId = channelId
            };

            await shopRepo.AddAuctionAsync(auction);

            return (true, null, auction);
        }

        // =========================
        // PLACE BID
        // =========================
        public async Task<(bool success, string message)> PlaceBidAsync(
            ulong userId, int auctionId, int bidAmount, SocketGuild guild)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();

            var auction = await shopRepo.GetAuctionByIdAsync(auctionId);

            if (auction == null || auction.IsEnded)
                return (false, "❌ This auction is no longer active.");

            if (!ShopRegistry.All.TryGetValue(auction.ItemId, out var item))
                return (false, "Item not found.");

            // Role check
            if (item.RequiredRoleId.HasValue)
            {
                var member = guild.GetUser(userId);
                if (member == null || !member.Roles.Any(r => r.Id == item.RequiredRoleId.Value))
                    return (false, "❌ You do not have the required role to bid on this item.");
            }

            if (userId == auction.CurrentBidderDiscordId)
                return (false, "❌ You are already the highest bidder.");

            if (bidAmount <= auction.CurrentBid)
                return (false, $"❌ Your bid must be higher than the current bid of **{auction.CurrentBid:N0} gold**.");

            var bidder = await playerRepo.GetByDiscordIdAsync(userId);

            if (bidder == null)
                return (false, "You need to start your adventure first.");

            if (bidder.Gold < bidAmount)
                return (false, $"❌ You need **{bidAmount:N0} gold** but only have **{bidder.Gold:N0}**.");

            // Refund previous bidder instantly
            if (auction.CurrentBidderDiscordId.HasValue &&
                auction.CurrentBidderDiscordId.Value != auction.StartedByDiscordId)
            {
                var previousBidder = await playerRepo.GetByDiscordIdAsync(auction.CurrentBidderDiscordId.Value);
                if (previousBidder != null)
                {
                    previousBidder.Gold += auction.CurrentBid;
                    await playerRepo.UpdatePlayerAsync(previousBidder);
                }
            }

            // Deduct new bid
            bidder.Gold -= bidAmount;
            await playerRepo.UpdatePlayerAsync(bidder);

            // Update auction
            auction.CurrentBid = bidAmount;
            auction.CurrentBidderDiscordId = userId;
            await shopRepo.UpdateAuctionAsync(auction);

            // Post to feed
            await PostBidFeedAsync(userId, auction, item, bidAmount);

            return (true, $"✅ You placed a bid of **{bidAmount:N0} gold** on **{item.Icon} {item.Name}**!");
        }

        // =========================
        // END AUCTION (ADMIN ONLY)
        // =========================
        public async Task<(bool success, string message)> EndAuctionAsync(
            ulong adminUserId, int auctionId, SocketGuild guild)
        {
            // Verify admin role
            var admin = guild.GetUser(adminUserId);
            if (admin == null || !admin.Roles.Any(r => r.Id == _adminRoleId))
                return (false, "❌ Only admins can end auctions.");

            using var scope = _scopeFactory.CreateScope();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();

            var auction = await shopRepo.GetAuctionByIdAsync(auctionId);

            if (auction == null || auction.IsEnded)
                return (false, "❌ Auction not found or already ended.");

            if (!ShopRegistry.All.TryGetValue(auction.ItemId, out var item))
                return (false, "Item not found.");

            auction.IsEnded = true;
            auction.EndedAt = DateTime.UtcNow;
            await shopRepo.UpdateAuctionAsync(auction);

            // Log as a purchase for admin fulfillment tracking
            await shopRepo.AddPurchaseAsync(new ShopPurchase
            {
                BuyerDiscordId = auction.CurrentBidderDiscordId!.Value,
                ItemId = item.Id,
                ItemName = item.Name,
                GoldPaid = auction.CurrentBid
            });

            // Announce winner to feed
            await PostAuctionEndFeedAsync(auction, item);

            return (true, $"✅ Auction ended. **{item.Icon} {item.Name}** won by <@{auction.CurrentBidderDiscordId}> for **{auction.CurrentBid:N0} gold**.");
        }

        // =========================
        // GET ACTIVE AUCTIONS
        // =========================
        public async Task<List<ActiveAuction>> GetActiveAuctionsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();
            return await shopRepo.GetActiveAuctionsAsync();
        }

        // =========================
        // GET PENDING PURCHASES (ADMIN)
        // =========================
        public async Task<List<ShopPurchase>> GetPendingPurchasesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();
            return await shopRepo.GetPendingAsync();
        }

        // =========================
        // FULFIL PURCHASE (ADMIN)
        // =========================
        public async Task<(bool success, string message)> FulfilPurchaseAsync(ulong adminUserId, int purchaseId, SocketGuild guild)
        {
            var admin = guild.GetUser(adminUserId);
            if (admin == null || !admin.Roles.Any(r => r.Id == _adminRoleId))
                return (false, "❌ Only admins can fulfil purchases.");

            using var scope = _scopeFactory.CreateScope();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();

            var success = await shopRepo.FulfillAsync(purchaseId);

            return success
                ? (true, $"✅ Purchase #{purchaseId} marked as fulfilled.")
                : (false, $"❌ Purchase #{purchaseId} not found or already fulfilled.");
        }

        // =========================
        // FEED: PURCHASE
        // =========================
        private async Task PostPurchaseFeedAsync(ulong userId, ShopItemDefinition item)
        {
            var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🛒 Shop Purchase")
                .WithDescription($"<@{userId}> purchased **{item.Icon} {item.Name}** for **{item.Price:N0} gold**.")
                .WithColor(Color.Gold)
                .WithFooter("⏳ Pending admin fulfilment")
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        // =========================
        // FEED: BID
        // =========================
        private async Task PostBidFeedAsync(ulong userId, ActiveAuction auction, ShopItemDefinition item, int bid)
        {
            var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle($"🔨 Auction — {item.Icon} {item.Name}")
                .WithDescription($"<@{userId}> placed a bid of **{bid:N0} gold**!")
                .WithColor(Color.Orange)
                .WithFooter($"Auction ID: {auction.Id} | Use /shopbid to outbid")
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        // =========================
        // FEED: AUCTION END
        // =========================
        private async Task PostAuctionEndFeedAsync(ActiveAuction auction, ShopItemDefinition item)
        {
            var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle($"🏆 Auction Ended — {item.Icon} {item.Name}")
                .WithDescription(
                    $"**Winner:** <@{auction.CurrentBidderDiscordId}>\n" +
                    $"**Winning Bid:** {auction.CurrentBid:N0} gold\n\n" +
                    $"An admin will be in touch shortly to deliver your reward.")
                .WithColor(Color.Gold)
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        // =========================
        // FEED: AUCTION MESSAGE
        // =========================
        public async Task SetAuctionMessageAsync(int auctionId, ulong messageId)
        {
            using var scope = _scopeFactory.CreateScope();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();
            await shopRepo.SetAuctionMessageIdAsync(auctionId, messageId);
        }

        // =========================
        // Get auction by ID
        // =========================
        public async Task<ActiveAuction?> GetAuctionByIdAsync(int auctionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();
            return await shopRepo.GetAuctionByIdAsync(auctionId);
        }
    }
}