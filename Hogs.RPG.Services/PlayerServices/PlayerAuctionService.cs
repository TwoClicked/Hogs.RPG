using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Hogs.RPG.Services.AuctionServices
{
    public class PlayerAuctionService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;

        // ⚠️ Replace with your actual market/auction channel ID
        private const ulong MarketChannelId = 1491919891090112633;

        // Feed channel for announcements (same as used elsewhere)
        private const ulong FeedChannelId = 1485357755433750549;

        public PlayerAuctionService(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;
        }

        // =========================
        // LIST ITEM
        // Removes 1x from inventory and creates a listing.
        // =========================
        public async Task<(bool success, string message, PlayerAuctionListing? listing)> ListItemAsync(
            ulong sellerId, string itemId, int basePrice, int? buyoutPrice)
        {
            if (basePrice <= 0)
                return (false, "❌ Base price must be greater than 0.", null);

            if (buyoutPrice.HasValue && buyoutPrice.Value <= basePrice)
                return (false, "❌ Buyout price must be higher than the base price.", null);

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var inventoryRepo = scope.ServiceProvider.GetRequiredService<InventoryRepository>();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();

            var player = await playerRepo.GetByDiscordIdAsync(sellerId);
            if (player == null) return (false, "❌ Player not found.", null);

            var inventory = await inventoryRepo.GetInventoryAsync(sellerId);
            var invItem = inventory.FirstOrDefault(i => i.ItemId == itemId);

            if (invItem == null || invItem.Quantity <= 0)
                return (false, "❌ You don't have that item in your inventory.", null);

            // Resolve display name
            string displayName = itemId;
            if (InventoryItemDefinitions.All.TryGetValue(itemId, out var matDef))
                displayName = matDef.Name;
            else if (EquipmentRegistry.All.TryGetValue(itemId, out var gearDef))
                displayName = gearDef.Name;

            // Deduct from inventory immediately (item held in listing escrow)
            await inventoryRepo.RemoveItemAsync(sellerId, itemId, 1);

            var listing = new PlayerAuctionListing
            {
                SellerDiscordId = sellerId,
                Type = ListingType.Item,
                ItemId = itemId,
                ListingName = displayName,
                BasePrice = basePrice,
                BuyoutPrice = buyoutPrice,
                CurrentBid = basePrice,
                ChannelId = MarketChannelId
            };

            await auctionRepo.AddListingAsync(listing);

            await PostListingFeedAsync(sellerId, listing);

            return (true, null, listing);
        }

        // =========================
        // LIST PET
        // Marks the pet as listed and creates a listing.
        // =========================
        public async Task<(bool success, string message, PlayerAuctionListing? listing)> ListPetAsync(
            ulong sellerId, int petDbId, int basePrice, int? buyoutPrice)
        {
            if (basePrice <= 0)
                return (false, "❌ Base price must be greater than 0.", null);

            if (buyoutPrice.HasValue && buyoutPrice.Value <= basePrice)
                return (false, "❌ Buyout price must be higher than the base price.", null);

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var petRepo = scope.ServiceProvider.GetRequiredService<PetRepository>();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();

            var pet = await petRepo.GetByIdAsync(petDbId);
            if (pet == null || pet.DiscordId != sellerId)
                return (false, "❌ Pet not found or doesn't belong to you.", null);

            if (pet.IsEquipped)
                return (false, "❌ Unequip the pet before listing it on the market.", null);

            if (pet.IsListed)
                return (false, "❌ This pet is already listed on the market.", null);

            // Resolve display name
            string petName = pet.CustomName ?? (PetRegistry.All.TryGetValue(pet.PetId, out var def) ? def.Name : pet.PetId);

            // Mark as listed — prevents equipping/evolving/trading during listing
            pet.IsListed = true;
            await petRepo.SaveAsync();

            var listing = new PlayerAuctionListing
            {
                SellerDiscordId = sellerId,
                Type = ListingType.Pet,
                PetId = petDbId,
                ListingName = $"{petName} (Lv.{pet.Level})",
                BasePrice = basePrice,
                BuyoutPrice = buyoutPrice,
                CurrentBid = basePrice,
                ChannelId = MarketChannelId
            };

            await auctionRepo.AddListingAsync(listing);

            await PostListingFeedAsync(sellerId, listing);

            return (true, null, listing);
        }

        // =========================
        // LIST RELIC
        // Marks the relic as listed and creates a listing.
        // =========================
        public async Task<(bool success, string message, PlayerAuctionListing? listing)> ListRelicAsync(
            ulong sellerId, int relicDbId, int basePrice, int? buyoutPrice)
        {
            if (basePrice <= 0)
                return (false, "❌ Base price must be greater than 0.", null);

            if (buyoutPrice.HasValue && buyoutPrice.Value <= basePrice)
                return (false, "❌ Buyout price must be higher than the base price.", null);

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var relicRepo = scope.ServiceProvider.GetRequiredService<RelicRepository>();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();

            var relic = await relicRepo.GetByIdAsync(relicDbId);
            if (relic == null || relic.DiscordId != sellerId)
                return (false, "❌ Relic not found or doesn't belong to you.", null);

            if (relic.IsEquipped)
                return (false, "❌ Unequip the relic before listing it on the market.", null);

            if (relic.IsListed)
                return (false, "❌ This relic is already listed on the market.", null);

            string relicName = RelicRegistry.All.TryGetValue(relic.RelicId, out var def)
                ? $"{def.Name} (Rank {relic.Rank})"
                : relic.RelicId;

            // Mark as listed
            relic.IsListed = true;
            await relicRepo.SaveAsync();

            var listing = new PlayerAuctionListing
            {
                SellerDiscordId = sellerId,
                Type = ListingType.Relic,
                RelicId = relicDbId,
                ListingName = relicName,
                BasePrice = basePrice,
                BuyoutPrice = buyoutPrice,
                CurrentBid = basePrice,
                ChannelId = MarketChannelId
            };

            await auctionRepo.AddListingAsync(listing);

            await PostListingFeedAsync(sellerId, listing);

            return (true, null, listing);
        }

        // =========================
        // PLACE BID
        // =========================
        public async Task<(bool success, string message)> PlaceBidAsync(
            ulong bidderId, int listingId, int bidAmount)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();

            var listing = await auctionRepo.GetByIdAsync(listingId);

            if (listing == null || listing.IsEnded || listing.IsCancelled)
                return (false, "❌ This listing is no longer active.");

            if (bidderId == listing.SellerDiscordId)
                return (false, "❌ You cannot bid on your own listing.");

            if (bidderId == listing.CurrentBidderDiscordId)
                return (false, "❌ You are already the highest bidder.");

            if (bidAmount <= listing.CurrentBid)
                return (false, $"❌ Your bid must be higher than the current bid of **{listing.CurrentBid:N0} gold**.");

            if (listing.BuyoutPrice.HasValue && bidAmount >= listing.BuyoutPrice.Value)
                return (false, $"❌ That bid matches or exceeds the buyout price. Use `/market-buyout {listingId}` to buy instantly.");

            var bidder = await playerRepo.GetByDiscordIdAsync(bidderId);
            if (bidder == null) return (false, "❌ You need to start your adventure first.");

            if (bidder.Gold < bidAmount)
                return (false, $"❌ You need **{bidAmount:N0} gold** but only have **{bidder.Gold:N0}**.");

            // Refund the previous bidder (if there is one and they're not the seller)
            if (listing.CurrentBidderDiscordId.HasValue &&
                listing.CurrentBidderDiscordId.Value != listing.SellerDiscordId)
            {
                var prevBidder = await playerRepo.GetByDiscordIdAsync(listing.CurrentBidderDiscordId.Value);
                if (prevBidder != null)
                {
                    prevBidder.Gold += listing.CurrentBid;
                    await playerRepo.UpdatePlayerAsync(prevBidder);
                }
            }

            // Deduct new bidder's gold
            bidder.Gold -= bidAmount;
            await playerRepo.UpdatePlayerAsync(bidder);

            listing.CurrentBid = bidAmount;
            listing.CurrentBidderDiscordId = bidderId;
            await auctionRepo.UpdateListingAsync(listing);

            // Update the public listing embed in the market channel
            await RefreshListingEmbedAsync(listing);

            await PostBidFeedAsync(bidderId, listing, bidAmount);

            return (true, $"✅ Bid of **{bidAmount:N0} gold** placed on **{listing.ListingName}**!");
        }

        // =========================
        // BUYOUT
        // Instant purchase at the buyout price.
        // =========================
        public async Task<(bool success, string message)> BuyoutAsync(
            ulong buyerId, int listingId)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();

            var listing = await auctionRepo.GetByIdAsync(listingId);

            if (listing == null || listing.IsEnded || listing.IsCancelled)
                return (false, "❌ This listing is no longer active.");

            if (!listing.BuyoutPrice.HasValue)
                return (false, "❌ This listing does not have a buyout price.");

            if (buyerId == listing.SellerDiscordId)
                return (false, "❌ You cannot buy out your own listing.");

            var buyer = await playerRepo.GetByDiscordIdAsync(buyerId);
            if (buyer == null) return (false, "❌ You need to start your adventure first.");

            if (buyer.Gold < listing.BuyoutPrice.Value)
                return (false, $"❌ You need **{listing.BuyoutPrice.Value:N0} gold** but only have **{buyer.Gold:N0}**.");

            // Refund the previous bidder if they're not the seller
            if (listing.CurrentBidderDiscordId.HasValue &&
                listing.CurrentBidderDiscordId.Value != listing.SellerDiscordId &&
                listing.CurrentBidderDiscordId.Value != buyerId)
            {
                var prevBidder = await playerRepo.GetByDiscordIdAsync(listing.CurrentBidderDiscordId.Value);
                if (prevBidder != null)
                {
                    prevBidder.Gold += listing.CurrentBid;
                    await playerRepo.UpdatePlayerAsync(prevBidder);
                }
            }

            // Execute the transfer
            listing.CurrentBid = listing.BuyoutPrice.Value;
            listing.CurrentBidderDiscordId = buyerId;
            buyer.Gold -= listing.BuyoutPrice.Value;
            await playerRepo.UpdatePlayerAsync(buyer);

            await ExecuteTransferAsync(scope, listing, buyerId);

            // Pay seller
            var seller = await playerRepo.GetByDiscordIdAsync(listing.SellerDiscordId);
            if (seller != null)
            {
                seller.Gold += listing.BuyoutPrice.Value;
                await playerRepo.UpdatePlayerAsync(seller);
            }

            listing.IsEnded = true;
            listing.EndedAt = DateTime.UtcNow;
            await auctionRepo.UpdateListingAsync(listing);

            await MarkListingEndedInChannelAsync(listing, buyerId, listing.BuyoutPrice.Value, isBuyout: true);
            await PostEndFeedAsync(listing, buyerId, listing.BuyoutPrice.Value);

            return (true, $"✅ You bought **{listing.ListingName}** for **{listing.BuyoutPrice.Value:N0} gold**!");
        }

        // =========================
        // END LISTING (seller only)
        // Completes the auction and transfers item to highest bidder.
        // If no bids, returns item to seller.
        // =========================
        public async Task<(bool success, string message)> EndListingAsync(
            ulong callerId, int listingId)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();

            var listing = await auctionRepo.GetByIdAsync(listingId);

            if (listing == null || listing.IsEnded || listing.IsCancelled)
                return (false, "❌ This listing is no longer active.");

            if (listing.SellerDiscordId != callerId)
                return (false, "❌ Only the seller can end this listing.");

            bool hasBidder = listing.CurrentBidderDiscordId.HasValue &&
                             listing.CurrentBidderDiscordId.Value != listing.SellerDiscordId;

            if (hasBidder)
            {
                // Winner takes the item — seller gets the gold
                await ExecuteTransferAsync(scope, listing, listing.CurrentBidderDiscordId!.Value);

                var seller = await playerRepo.GetByDiscordIdAsync(listing.SellerDiscordId);
                if (seller != null)
                {
                    seller.Gold += listing.CurrentBid;
                    await playerRepo.UpdatePlayerAsync(seller);
                }

                listing.IsEnded = true;
                listing.EndedAt = DateTime.UtcNow;
                await auctionRepo.UpdateListingAsync(listing);

                await MarkListingEndedInChannelAsync(listing, listing.CurrentBidderDiscordId!.Value, listing.CurrentBid, isBuyout: false);
                await PostEndFeedAsync(listing, listing.CurrentBidderDiscordId!.Value, listing.CurrentBid);

                return (true,
                    $"✅ Auction ended! **{listing.ListingName}** sold to <@{listing.CurrentBidderDiscordId}> for **{listing.CurrentBid:N0} gold**.\n" +
                    $"💰 **+{listing.CurrentBid:N0} gold** added to your balance.");
            }
            else
            {
                // No bids — return item to seller
                await ReturnListingToSellerAsync(scope, listing);

                listing.IsEnded = true;
                listing.EndedAt = DateTime.UtcNow;
                await auctionRepo.UpdateListingAsync(listing);

                await MarkListingCancelledInChannelAsync(listing, reason: "No bids — listing ended by seller.");

                return (true, $"✅ Listing ended with no bids. Your **{listing.ListingName}** has been returned.");
            }
        }

        // =========================
        // CANCEL LISTING (seller only, no bids)
        // =========================
        public async Task<(bool success, string message)> CancelListingAsync(
            ulong callerId, int listingId)
        {
            using var scope = _scopeFactory.CreateScope();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();

            var listing = await auctionRepo.GetByIdAsync(listingId);

            if (listing == null || listing.IsEnded || listing.IsCancelled)
                return (false, "❌ This listing is no longer active.");

            if (listing.SellerDiscordId != callerId)
                return (false, "❌ Only the seller can cancel this listing.");

            bool hasBidder = listing.CurrentBidderDiscordId.HasValue &&
                             listing.CurrentBidderDiscordId.Value != listing.SellerDiscordId;

            if (hasBidder)
                return (false, "❌ Cannot cancel a listing that already has bids. Use `/market-end` to close it instead.");

            // Return item to seller
            await ReturnListingToSellerAsync(scope, listing);

            listing.IsCancelled = true;
            listing.EndedAt = DateTime.UtcNow;
            await auctionRepo.UpdateListingAsync(listing);

            await MarkListingCancelledInChannelAsync(listing, reason: "Listing cancelled by seller.");

            return (true, $"✅ Listing cancelled. Your **{listing.ListingName}** has been returned.");
        }

        // =========================
        // GET ACTIVE LISTINGS
        // =========================
        public async Task<List<PlayerAuctionListing>> GetActiveListingsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();
            return await auctionRepo.GetActiveListingsAsync();
        }

        public async Task<PlayerAuctionListing?> GetListingByIdAsync(int listingId)
        {
            using var scope = _scopeFactory.CreateScope();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();
            return await auctionRepo.GetByIdAsync(listingId);
        }

        public async Task SetListingMessageAsync(int listingId, ulong messageId)
        {
            using var scope = _scopeFactory.CreateScope();
            var auctionRepo = scope.ServiceProvider.GetRequiredService<PlayerAuctionRepository>();
            await auctionRepo.SetMessageIdAsync(listingId, messageId);
        }

        // =========================
        // BUILD LISTING EMBED (shared)
        // =========================
        public Embed BuildListingEmbed(PlayerAuctionListing listing)
        {
            string typeIcon = listing.Type switch
            {
                ListingType.Pet => "🐾",
                ListingType.Relic => "🔮",
                _ => "📦"
            };

            var eb = new EmbedBuilder()
                .WithTitle($"{typeIcon} {listing.ListingName}")
                .WithColor(new Color(0xE67E22))
                .AddField("💰 Current Bid", $"**{listing.CurrentBid:N0} gold**", true)
                .AddField("🏁 Base Price", $"{listing.BasePrice:N0} gold", true);

            if (listing.BuyoutPrice.HasValue)
                eb.AddField("⚡ Buyout", $"{listing.BuyoutPrice.Value:N0} gold", true);

            if (listing.CurrentBidderDiscordId.HasValue &&
                listing.CurrentBidderDiscordId.Value != listing.SellerDiscordId)
                eb.AddField("👤 Highest Bidder", $"<@{listing.CurrentBidderDiscordId}>", true);
            else
                eb.AddField("👤 Highest Bidder", "*No bids yet*", true);

            eb.AddField("📋 How to bid",
                $"`/market-bid {listing.Id} <amount>` — place a bid\n" +
                (listing.BuyoutPrice.HasValue ? $"`/market-buyout {listing.Id}` — instant buyout\n" : "") +
                $"*Outbid players are refunded instantly.*",
                inline: false);

            eb.WithFooter($"Listing #{listing.Id} · Listed by <@{listing.SellerDiscordId}> · " +
                          $"{listing.ListedAt:dd MMM yyyy}");

            return eb.Build();
        }

        // =========================
        // PRIVATE HELPERS
        // =========================

        // Transfer the listed asset to the winner.
        private async Task ExecuteTransferAsync(IServiceScope scope, PlayerAuctionListing listing, ulong winnerId)
        {
            switch (listing.Type)
            {
                case ListingType.Item:
                    {
                        var inventoryRepo = scope.ServiceProvider.GetRequiredService<InventoryRepository>();
                        await inventoryRepo.AddItemAsync(winnerId, listing.ItemId!, 1);
                        break;
                    }

                case ListingType.Pet:
                    {
                        var petRepo = scope.ServiceProvider.GetRequiredService<PetRepository>();
                        var pet = await petRepo.GetByIdAsync(listing.PetId!.Value);
                        if (pet != null)
                        {
                            pet.DiscordId = winnerId;
                            pet.IsListed = false;
                            pet.IsEquipped = false;
                            await petRepo.SaveAsync();
                        }
                        break;
                    }

                case ListingType.Relic:
                    {
                        var relicRepo = scope.ServiceProvider.GetRequiredService<RelicRepository>();
                        var relic = await relicRepo.GetByIdAsync(listing.RelicId!.Value);
                        if (relic != null)
                        {
                            relic.DiscordId = winnerId;
                            relic.IsListed = false;
                            relic.IsEquipped = false;
                            relic.SlotIndex = 0;
                            await relicRepo.SaveAsync();
                        }
                        break;
                    }
            }
        }

        // Return the listed asset back to the seller (no sale).
        private async Task ReturnListingToSellerAsync(IServiceScope scope, PlayerAuctionListing listing)
        {
            switch (listing.Type)
            {
                case ListingType.Item:
                    {
                        var inventoryRepo = scope.ServiceProvider.GetRequiredService<InventoryRepository>();
                        await inventoryRepo.AddItemAsync(listing.SellerDiscordId, listing.ItemId!, 1);
                        break;
                    }

                case ListingType.Pet:
                    {
                        var petRepo = scope.ServiceProvider.GetRequiredService<PetRepository>();
                        var pet = await petRepo.GetByIdAsync(listing.PetId!.Value);
                        if (pet != null)
                        {
                            pet.IsListed = false;
                            await petRepo.SaveAsync();
                        }
                        break;
                    }

                case ListingType.Relic:
                    {
                        var relicRepo = scope.ServiceProvider.GetRequiredService<RelicRepository>();
                        var relic = await relicRepo.GetByIdAsync(listing.RelicId!.Value);
                        if (relic != null)
                        {
                            relic.IsListed = false;
                            await relicRepo.SaveAsync();
                        }
                        break;
                    }
            }
        }

        private async Task RefreshListingEmbedAsync(PlayerAuctionListing listing)
        {
            if (listing.MessageId == 0) return;
            var channel = _client.GetChannel(listing.ChannelId) as ITextChannel;
            if (channel == null) return;

            var msg = await channel.GetMessageAsync(listing.MessageId) as IUserMessage;
            if (msg == null) return;

            await msg.ModifyAsync(m => m.Embed = BuildListingEmbed(listing));
        }

        private async Task MarkListingEndedInChannelAsync(
            PlayerAuctionListing listing, ulong winnerId, int finalPrice, bool isBuyout)
        {
            if (listing.MessageId == 0) return;
            var channel = _client.GetChannel(listing.ChannelId) as ITextChannel;
            if (channel == null) return;

            var msg = await channel.GetMessageAsync(listing.MessageId) as IUserMessage;
            if (msg == null) return;

            string label = isBuyout ? "🏷️ Bought Out" : "🏆 Sold";
            var endEmbed = new EmbedBuilder()
                .WithTitle($"{label} — {listing.ListingName}")
                .WithDescription(
                    $"**Winner:** <@{winnerId}>\n" +
                    $"**Final Price:** {finalPrice:N0} gold\n\n" +
                    $"*This listing has been closed.*")
                .WithColor(Color.Gold)
                .Build();

            await msg.ModifyAsync(m =>
            {
                m.Embed = endEmbed;
                m.Components = new ComponentBuilder().Build();
            });
        }

        private async Task MarkListingCancelledInChannelAsync(PlayerAuctionListing listing, string reason)
        {
            if (listing.MessageId == 0) return;
            var channel = _client.GetChannel(listing.ChannelId) as ITextChannel;
            if (channel == null) return;

            var msg = await channel.GetMessageAsync(listing.MessageId) as IUserMessage;
            if (msg == null) return;

            var cancelEmbed = new EmbedBuilder()
                .WithTitle($"❌ Listing Cancelled — {listing.ListingName}")
                .WithDescription(reason)
                .WithColor(Color.Red)
                .Build();

            await msg.ModifyAsync(m =>
            {
                m.Embed = cancelEmbed;
                m.Components = new ComponentBuilder().Build();
            });
        }

        private async Task PostListingFeedAsync(ulong sellerId, PlayerAuctionListing listing)
        {
            var channel = _client.GetChannel(FeedChannelId) as IMessageChannel;
            if (channel == null) return;

            string typeLabel = listing.Type switch
            {
                ListingType.Pet => "a pet",
                ListingType.Relic => "a relic",
                _ => "an item"
            };

            var embed = new EmbedBuilder()
                .WithTitle("🏪 New Market Listing")
                .WithDescription(
                    $"<@{sellerId}> listed **{listing.ListingName}** for sale!\n\n" +
                    $"💰 Starting bid: **{listing.BasePrice:N0} gold**" +
                    (listing.BuyoutPrice.HasValue ? $"\n⚡ Buyout: **{listing.BuyoutPrice.Value:N0} gold**" : ""))
                .WithColor(new Color(0xE67E22))
                .WithFooter($"Listing #{listing.Id} · Use /market-bid {listing.Id} to bid")
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        private async Task PostBidFeedAsync(ulong bidderId, PlayerAuctionListing listing, int bidAmount)
        {
            var channel = _client.GetChannel(FeedChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🔨 New Bid")
                .WithDescription(
                    $"<@{bidderId}> bid **{bidAmount:N0} gold** on **{listing.ListingName}**!\n" +
                    $"*Listing #{listing.Id}*")
                .WithColor(Color.Blue)
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        private async Task PostEndFeedAsync(PlayerAuctionListing listing, ulong winnerId, int finalPrice)
        {
            var channel = _client.GetChannel(FeedChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🏆 Listing Sold!")
                .WithDescription(
                    $"**{listing.ListingName}** sold to <@{winnerId}> for **{finalPrice:N0} gold**!\n" +
                    $"*Seller: <@{listing.SellerDiscordId}>*")
                .WithColor(Color.Gold)
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
    }
}
