using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Shop;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.DungeonServices;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.PlayerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Hogs.RPG.Services.ShopServices
{
    public class ShopService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;
        private readonly DungeonService _dungeonService;
        private readonly PetDungeonService _petService;

        private readonly ulong _feedChannelId = 1485357755433750549;
        private readonly ulong _adminRoleId = 1483528182106685691;

        public ShopService(IServiceScopeFactory scopeFactory, DiscordSocketClient client, DungeonService dungeonService, PetDungeonService petService)
        {
            _scopeFactory = scopeFactory;
            _client = client;
            _dungeonService = dungeonService;
            _petService = petService;
        }

        public async Task<(bool success, string message)> BuyAsync(ulong userId, string itemId, SocketGuild guild)
        {
            if (!ShopRegistry.All.TryGetValue(itemId, out var item))
                return (false, "Item not found.");

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

            player.Gold -= item.Price;
            await playerRepo.UpdatePlayerAsync(player);

            // Declare before use
            bool isInstant = item.Category == Hogs.RPG.Core.Enums.ShopCategory.RpgPerks;

            // Log purchase — mark as fulfilled immediately for instant perks
            await shopRepo.AddPurchaseAsync(new ShopPurchase
            {
                BuyerDiscordId = userId,
                ItemId = item.Id,
                ItemName = item.Name,
                GoldPaid = item.Price,
                IsFulfilled = isInstant,
                FulfilledAt = isInstant ? DateTime.UtcNow : null
            });

            if (isInstant)
                await ApplyRpgPerkAsync(userId, itemId, playerRepo);

            await PostPurchaseFeedAsync(userId, item, isInstant);

            string confirmation = isInstant
                ? $"✅ You purchased **{item.Icon} {item.Name}** for **{item.Price:N0} gold**!\nYour perk has been applied."
                : $"✅ You purchased **{item.Icon} {item.Name}** for **{item.Price:N0} gold**!\nAn admin will fulfil your order soon.";

            return (true, confirmation);
        }

        // =========================
        // BUY AND RENAME PET
        // =========================
        public async Task<(bool success, string message)> BuyAndRenamePetAsync(
            ulong userId, string itemId, SocketGuild guild, string newName)
        {
            if (!ShopRegistry.All.TryGetValue(itemId, out var item))
                return (false, "Item not found.");

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();
            var petRepo = scope.ServiceProvider.GetRequiredService<PetRepository>();

            var player = await playerRepo.GetByDiscordIdAsync(userId);

            if (player == null)
                return (false, "You need to start your adventure first.");

            if (player.Gold < item.Price)
                return (false, $"❌ You need **{item.Price:N0} gold** but only have **{player.Gold:N0}**.");

            var pet = await petRepo.GetEquippedPetAsync(userId);

            if (pet == null)
                return (false, "❌ You don't have a pet equipped. Equip a pet first then try again.");

            player.Gold -= item.Price;
            await playerRepo.UpdatePlayerAsync(player);

            pet.CustomName = newName;
            await petRepo.SaveAsync();

            await shopRepo.AddPurchaseAsync(new ShopPurchase
            {
                BuyerDiscordId = userId,
                ItemId = item.Id,
                ItemName = item.Name,
                GoldPaid = item.Price,
                IsFulfilled = true,
                FulfilledAt = DateTime.UtcNow
            });

            await PostPurchaseFeedAsync(userId, item, true);

            return (true, $"✅ Your pet has been renamed to **{newName}** for **{item.Price:N0} gold**!");
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

            if (item.RequiredRoleId.HasValue)
            {
                var member = guild.GetUser(userId);
                if (member == null || !member.Roles.Any(r => r.Id == item.RequiredRoleId.Value))
                    return (false, "❌ You do not have the required role to start this auction.", null);
            }

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();

            var existing = await shopRepo.GetActiveAuctionByItemAsync(itemId);
            if (existing != null)
                return (false, $"❌ There is already an active auction for **{item.Name}**.", null);

            var player = await playerRepo.GetByDiscordIdAsync(userId);

            if (player == null)
                return (false, "You need to start your adventure first.", null);

            if (player.Gold < item.StartingBid)
                return (false, $"❌ You need **{item.StartingBid:N0} gold** to start this auction but only have **{player.Gold:N0}**.", null);

            player.Gold -= item.StartingBid;
            await playerRepo.UpdatePlayerAsync(player);

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

            bidder.Gold -= bidAmount;
            await playerRepo.UpdatePlayerAsync(bidder);

            auction.CurrentBid = bidAmount;
            auction.CurrentBidderDiscordId = userId;
            await shopRepo.UpdateAuctionAsync(auction);

            await PostBidFeedAsync(userId, auction, item, bidAmount);

            return (true, $"✅ You placed a bid of **{bidAmount:N0} gold** on **{item.Icon} {item.Name}**!");
        }

        // =========================
        // END AUCTION (ADMIN ONLY)
        // =========================
        public async Task<(bool success, string message)> EndAuctionAsync(
            ulong adminUserId, int auctionId, SocketGuild guild)
        {
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

            await shopRepo.AddPurchaseAsync(new ShopPurchase
            {
                BuyerDiscordId = auction.CurrentBidderDiscordId!.Value,
                ItemId = item.Id,
                ItemName = item.Name,
                GoldPaid = auction.CurrentBid
            });

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
        public async Task<(bool success, string message)> FulfilPurchaseAsync(
            ulong adminUserId, int purchaseId, SocketGuild guild)
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
        // SET AUCTION MESSAGE ID
        // =========================
        public async Task SetAuctionMessageAsync(int auctionId, ulong messageId)
        {
            using var scope = _scopeFactory.CreateScope();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();
            await shopRepo.SetAuctionMessageIdAsync(auctionId, messageId);
        }

        // =========================
        // GET AUCTION BY ID
        // =========================
        public async Task<ActiveAuction?> GetAuctionByIdAsync(int auctionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var shopRepo = scope.ServiceProvider.GetRequiredService<ShopRepository>();
            return await shopRepo.GetAuctionByIdAsync(auctionId);
        }

        // =========================
        // FEED: PURCHASE
        // =========================
        private async Task PostPurchaseFeedAsync(ulong userId, ShopItemDefinition item, bool isInstant)
        {
            var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;
            if (channel == null) return;

            var footer = isInstant ? "✅ Applied instantly" : "⏳ Pending admin fulfilment";

            var embed = new EmbedBuilder()
                .WithTitle("🛒 Shop Purchase")
                .WithDescription($"<@{userId}> purchased **{item.Icon} {item.Name}** for **{item.Price:N0} gold**.")
                .WithColor(Color.Gold)
                .WithFooter(footer)
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
        // RPG PERK INSTANT DELIVERY
        // =========================
        private async Task ApplyRpgPerkAsync(ulong userId, string itemId, PlayerRepository playerRepo)
        {
            var player = await playerRepo.GetByDiscordIdAsync(userId);
            if (player == null) return;

            switch (itemId)
            {
                // =========================
                // 🏹 STAMINA RESET
                // =========================
                case "rpg_stamina_reset":
                    int resetMax = player.StaminaBoostExpiry.HasValue &&
                                   player.StaminaBoostExpiry.Value > DateTime.UtcNow ? 150 : 100;
                    player.HunterStamina = resetMax;
                    player.LastHunterStaminaUpdate = DateTimeOffset.UtcNow.ToString("o");
                    await playerRepo.UpdatePlayerAsync(player);
                    break;

                // =========================
                // ✨ DOUBLE XP — 24 hours
                // =========================
                case "rpg_double_xp":
                    player.XpBoostExpiry = DateTime.UtcNow.AddHours(24);
                    await playerRepo.UpdatePlayerAsync(player);
                    break;

                // =========================
                // ⚡ STAMINA BOOST — 7 days, cap raised to 150
                // =========================
                case "rpg_stamina_boost":
                    player.StaminaBoostExpiry = DateTime.UtcNow.AddDays(7);
                    player.HunterStamina = Math.Min(150, player.HunterStamina + 50);
                    await playerRepo.UpdatePlayerAsync(player);
                    break;

                // =========================
                // 📦 LOOT CRATE — 20 random rare items
                // =========================
                case "rpg_loot_crate":
                    var rareItems = new[]
                    {
                        "wolf_trophy",
                        "alpha_fang",
                        "storm_talon",
                        "saber_relic",
                        "griffin_core",
                        "storm_relic",
                        "ancient_core",
                        "mythic_heart",
                        "sky_relic"
                    };

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var inventoryService = scope.ServiceProvider
                            .GetRequiredService<Hogs.RPG.Services.InventoryServices.InventoryService>();

                        var random = new Random();
                        var dropped = new Dictionary<string, int>();

                        for (int i = 0; i < 20; i++)
                        {
                            var pick = rareItems[random.Next(rareItems.Length)];
                            if (!dropped.ContainsKey(pick)) dropped[pick] = 0;
                            dropped[pick]++;
                        }

                        foreach (var drop in dropped)
                            await inventoryService.GiveItemAsync(userId, drop.Key, drop.Value);
                    }
                    break;

                // =========================
                // ⚗️ ENERGY REFILL
                // =========================
                case "rpg_energy_refill":
                    player.Energy = 100;
                    player.LastEnergyUpdate = DateTimeOffset.UtcNow.ToString("o");
                    await playerRepo.UpdatePlayerAsync(player);
                    break;

                // =========================
                // 🍖 PET SNACK SMALL — +50 XP
                // =========================
                case "rpg_pet_snack_small":
                    await ApplyPetSnackAsync(userId, 50);
                    break;

                // =========================
                // 🍗 PET SNACK MEDIUM — +150 XP
                // =========================
                case "rpg_pet_snack_medium":
                    await ApplyPetSnackAsync(userId, 150);
                    break;

                // =========================
                // 🍖 PET SNACK LARGE — +300 XP
                // =========================
                case "rpg_pet_snack_large":
                    await ApplyPetSnackAsync(userId, 300);
                    break;

                // =========================
                // 🏰 DUNGEON COOLDOWN RESET
                // =========================
                case "rpg_dungeon_reset":
                    _dungeonService.ResetDungeonCooldown(userId);
                    break;

                // =========================
                // 🏰 PET DUNGEON COOLDOWN RESET
                // =========================
                case "rpg_pet_dungeon_reset":
                    _petService.ResetPetDungeonCooldown(userId);
                    break;
            }
        }

        // =========================
        // PET SNACK HELPER
        // =========================
        private async Task ApplyPetSnackAsync(ulong userId, int xpAmount)
        {
            using var scope = _scopeFactory.CreateScope();
            var petService = scope.ServiceProvider
                .GetRequiredService<Hogs.RPG.Services.PetServices.PetService>();

            var (leveledUp, newLevel) = await petService.AddXPAsync(userId, xpAmount);

            if (leveledUp)
            {
                var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;
                if (channel != null)
                {
                    await channel.SendMessageAsync(
                        $"🐾 <@{userId}>'s pet reached **Level {newLevel}** from a snack! 🎉");
                }
            }
        }
    }
}