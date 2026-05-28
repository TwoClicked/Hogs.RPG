using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Services.AuctionServices;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    [Group("market", "Player-to-player marketplace — list, bid and trade")]
    public class MarketModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PlayerAuctionService _marketService;
        private readonly DiscordSocketClient _client;

        // ⚠️ Replace with your actual market channel ID
        private const ulong MarketChannelId = 0000000000000000000;

        public MarketModule(PlayerAuctionService marketService, DiscordSocketClient client)
        {
            _marketService = marketService;
            _client = client;
        }

        // =========================
        // /market list-item
        // =========================
        [SlashCommand("list-item", "List an inventory item for sale")]
        public async Task ListItem(
            [Summary("item_id", "Item ID (check /inventory for IDs)")] string itemId,
            [Summary("base_price", "Minimum bid price in gold")] int basePrice,
            [Summary("buyout_price", "Optional instant-buy price (leave 0 for none)")] int buyoutPrice = 0)
        {
            await DeferAsync(ephemeral: true);

            int? buyout = buyoutPrice > 0 ? buyoutPrice : null;

            var (success, message, listing) = await _marketService.ListItemAsync(
                Context.User.Id, itemId.Trim().ToLower(), basePrice, buyout);

            if (!success)
            {
                await FollowupAsync(message, ephemeral: true);
                return;
            }

            await PostListingToChannelAsync(listing!);
            await FollowupAsync(
                $"✅ **{listing!.ListingName}** listed on the market!\n" +
                $"Listing ID: `#{listing.Id}` · Base bid: **{listing.BasePrice:N0} gold**" +
                (listing.BuyoutPrice.HasValue ? $" · Buyout: **{listing.BuyoutPrice.Value:N0} gold**" : "") +
                $"\n\nOthers can use `/market bid {listing.Id} <amount>` to place bids.\n" +
                $"When you're ready to close, use `/market end {listing.Id}`.",
                ephemeral: true);
        }

        // =========================
        // /market list-pet
        // =========================
        [SlashCommand("list-pet", "List a pet for sale")]
        public async Task ListPet(
            [Summary("pet_id", "Pet DB ID (check /pets for IDs)")] int petId,
            [Summary("base_price", "Minimum bid price in gold")] int basePrice,
            [Summary("buyout_price", "Optional instant-buy price (leave 0 for none)")] int buyoutPrice = 0)
        {
            await DeferAsync(ephemeral: true);

            int? buyout = buyoutPrice > 0 ? buyoutPrice : null;

            var (success, message, listing) = await _marketService.ListPetAsync(
                Context.User.Id, petId, basePrice, buyout);

            if (!success)
            {
                await FollowupAsync(message, ephemeral: true);
                return;
            }

            await PostListingToChannelAsync(listing!);
            await FollowupAsync(
                $"✅ **{listing!.ListingName}** listed on the market!\n" +
                $"Listing ID: `#{listing.Id}` · Base bid: **{listing.BasePrice:N0} gold**" +
                (listing.BuyoutPrice.HasValue ? $" · Buyout: **{listing.BuyoutPrice.Value:N0} gold**" : "") +
                $"\n\nOthers can use `/market bid {listing.Id} <amount>` to place bids.\n" +
                $"When you're ready to close, use `/market end {listing.Id}`.",
                ephemeral: true);
        }

        // =========================
        // /market list-relic
        // =========================
        [SlashCommand("list-relic", "List a relic for sale")]
        public async Task ListRelic(
            [Summary("relic_id", "Relic DB ID (check /relics for IDs)")] int relicId,
            [Summary("base_price", "Minimum bid price in gold")] int basePrice,
            [Summary("buyout_price", "Optional instant-buy price (leave 0 for none)")] int buyoutPrice = 0)
        {
            await DeferAsync(ephemeral: true);

            int? buyout = buyoutPrice > 0 ? buyoutPrice : null;

            var (success, message, listing) = await _marketService.ListRelicAsync(
                Context.User.Id, relicId, basePrice, buyout);

            if (!success)
            {
                await FollowupAsync(message, ephemeral: true);
                return;
            }

            await PostListingToChannelAsync(listing!);
            await FollowupAsync(
                $"✅ **{listing!.ListingName}** listed on the market!\n" +
                $"Listing ID: `#{listing.Id}` · Base bid: **{listing.BasePrice:N0} gold**" +
                (listing.BuyoutPrice.HasValue ? $" · Buyout: **{listing.BuyoutPrice.Value:N0} gold**" : "") +
                $"\n\nOthers can use `/market bid {listing.Id} <amount>` to place bids.\n" +
                $"When you're ready to close, use `/market end {listing.Id}`.",
                ephemeral: true);
        }

        // =========================
        // /market bid
        // =========================
        [SlashCommand("bid", "Place a bid on a market listing")]
        public async Task Bid(
            [Summary("listing_id", "Listing ID (from /market view)")] int listingId,
            [Summary("amount", "Bid amount in gold")] int amount)
        {
            await DeferAsync(ephemeral: true);

            var (success, message) = await _marketService.PlaceBidAsync(
                Context.User.Id, listingId, amount);

            await FollowupAsync(message, ephemeral: true);
        }

        // =========================
        // /market buyout
        // =========================
        [SlashCommand("buyout", "Instantly purchase a listing at the buyout price")]
        public async Task Buyout(
            [Summary("listing_id", "Listing ID (from /market view)")] int listingId)
        {
            await DeferAsync(ephemeral: true);

            var (success, message) = await _marketService.BuyoutAsync(Context.User.Id, listingId);
            await FollowupAsync(message, ephemeral: !success);
        }

        // =========================
        // /market end
        // =========================
        [SlashCommand("end", "Close your listing and award the item to the highest bidder")]
        public async Task EndListing(
            [Summary("listing_id", "Listing ID to close")] int listingId)
        {
            await DeferAsync(ephemeral: true);

            var (success, message) = await _marketService.EndListingAsync(Context.User.Id, listingId);
            await FollowupAsync(message, ephemeral: true);
        }

        // =========================
        // /market cancel
        // =========================
        [SlashCommand("cancel", "Cancel a listing with no bids (item returned to you)")]
        public async Task CancelListing(
            [Summary("listing_id", "Listing ID to cancel")] int listingId)
        {
            await DeferAsync(ephemeral: true);

            var (success, message) = await _marketService.CancelListingAsync(Context.User.Id, listingId);
            await FollowupAsync(message, ephemeral: true);
        }

        // =========================
        // /market view
        // =========================
        [SlashCommand("view", "Browse all active market listings")]
        public async Task ViewListings()
        {
            await DeferAsync(ephemeral: true);

            var listings = await _marketService.GetActiveListingsAsync();

            if (listings.Count == 0)
            {
                await FollowupAsync("🏪 The market is empty. Be the first to list something!", ephemeral: true);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"🏪 **Active Listings** ({listings.Count} total)\n");

            foreach (var l in listings)
            {
                string typeIcon = l.Type switch
                {
                    Core.Enums.ListingType.Pet => "🐾",
                    Core.Enums.ListingType.Relic => "🔮",
                    _ => "📦"
                };

                bool hasBids = l.CurrentBidderDiscordId.HasValue &&
                               l.CurrentBidderDiscordId.Value != l.SellerDiscordId;

                string bidLine = hasBids
                    ? $"**{l.CurrentBid:N0}g** bid by <@{l.CurrentBidderDiscordId}>"
                    : $"No bids yet — base: **{l.BasePrice:N0}g**";

                string buyoutLine = l.BuyoutPrice.HasValue
                    ? $" · Buyout: **{l.BuyoutPrice.Value:N0}g**"
                    : "";

                sb.AppendLine($"**#{l.Id}** {typeIcon} **{l.ListingName}**");
                sb.AppendLine($"  {bidLine}{buyoutLine}");
                sb.AppendLine($"  Seller: <@{l.SellerDiscordId}> · Listed {l.ListedAt:dd MMM}");
                sb.AppendLine();
            }

            sb.AppendLine("*Use `/market bid <id> <amount>` to place a bid.*");

            var embed = new EmbedBuilder()
                .WithTitle("🏪 Player Market")
                .WithColor(new Color(0xE67E22))
                .WithDescription(sb.ToString().Trim())
                .Build();

            await FollowupAsync(embed: embed, ephemeral: true);
        }

        // =========================
        // HELPER — post listing to market channel
        // =========================
        private async Task PostListingToChannelAsync(Core.Entities.PlayerAuctionListing listing)
        {
            var channel = _client.GetChannel(MarketChannelId) as ITextChannel;
            if (channel == null) return;

            var embed = _marketService.BuildListingEmbed(listing);
            var msg = await channel.SendMessageAsync(embed: embed);

            await _marketService.SetListingMessageAsync(listing.Id, msg.Id);
        }
    }
}
