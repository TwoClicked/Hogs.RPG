using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Shop;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Services.ShopServices;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    // =========================
    // PET RENAME MODAL
    // =========================
    public class PetRenameModal : IModal
    {
        public string Title => "🐾 Rename Your Pet";

        [InputLabel("New Pet Name")]
        [ModalTextInput("pet_name", placeholder: "Enter a name...", minLength: 1, maxLength: 32)]
        public string PetName { get; set; }
    }

    public class ShopModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ShopService _shopService;

        private const ulong AdminRoleId = 1483528182106685691;
        private const ulong AuctionChannelId = 1491919891090112633;

        public ShopModule(ShopService shopService)
        {
            _shopService = shopService;
        }

        // =========================
        // /shop — ENTRY POINT
        // =========================
        [SlashCommand("shop", "Browse the Gold Shop")]
        public async Task Shop()
        {
            await DeferAsync(ephemeral: true);
            await ShowCategory(ShopCategory.VikingRiseResources);
        }

        // =========================
        // CATEGORY TAB BUTTONS
        // =========================
        [ComponentInteraction("shop_tab_*")]
        public async Task SwitchTab(string category)
        {
            await DeferAsync();
            await ShowCategory(Enum.Parse<ShopCategory>(category));
        }

        // =========================
        // CONFIRM PURCHASE BUTTON
        // =========================
        [ComponentInteraction("shop_confirm_*")]
        public async Task ConfirmPurchase(string itemId)
        {
            // Pet rename gets a modal instead of a confirm screen
            if (itemId == "discord_pet_rename")
            {
                var modal = new ModalBuilder()
                    .WithTitle("🐾 Rename Your Pet")
                    .WithCustomId("shop_petrename_modal")
                    .AddTextInput("New Pet Name", "pet_name",
                        placeholder: "Enter a name...",
                        minLength: 1,
                        maxLength: 32,
                        required: true)
                    .Build();

                await Context.Interaction.RespondWithModalAsync(modal);
                return;
            }

            await DeferAsync();

            if (!ShopRegistry.All.TryGetValue(itemId, out var item))
            {
                await ModifyOriginalResponseAsync(m => m.Content = "❌ Item not found.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("🛒 Confirm Purchase")
                .WithDescription(
                    $"{item.Icon} **{item.Name}**\n\n" +
                    $"{item.Description}\n\n" +
                    $"💰 Cost: **{item.Price:N0} gold**\n\n" +
                    $"Are you sure?")
                .WithColor(Color.Gold)
                .Build();

            var components = new ComponentBuilder()
                .WithButton("✅ Confirm", $"shop_buy_{itemId}", ButtonStyle.Success)
                .WithButton("❌ Cancel", "shop_cancel", ButtonStyle.Secondary)
                .Build();

            await ModifyOriginalResponseAsync(m =>
            {
                m.Embed = embed;
                m.Components = components;
            });
        }

        // =========================
        // PET RENAME MODAL SUBMIT
        // =========================
        [ModalInteraction("shop_petrename_modal")]
        public async Task PetRenameModalSubmit(PetRenameModal modal)
        {
            await DeferAsync(ephemeral: true);

            var newName = modal.PetName?.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                await FollowupAsync("❌ Name cannot be empty.", ephemeral: true);
                return;
            }

            var guild = (Context.Channel as SocketGuildChannel)?.Guild;
            if (guild == null)
            {
                await FollowupAsync("❌ This command must be used in a server.", ephemeral: true);
                return;
            }

            var (success, message) = await _shopService.BuyAndRenamePetAsync(
                Context.User.Id, "discord_pet_rename", guild, newName);

            await FollowupAsync(message, ephemeral: true);
        }

        // =========================
        // EXECUTE PURCHASE
        // =========================
        [ComponentInteraction("shop_buy_*")]
        public async Task ExecutePurchase(string itemId)
        {
            await DeferAsync();

            var guild = (Context.Channel as SocketGuildChannel)?.Guild;
            if (guild == null)
            {
                await ModifyOriginalResponseAsync(m => m.Content = "❌ This command must be used in a server.");
                return;
            }

            var (success, message) = await _shopService.BuyAsync(Context.User.Id, itemId, guild);

            var embed = new EmbedBuilder()
                .WithDescription(message)
                .WithColor(success ? Color.Green : Color.Red)
                .Build();

            var components = new ComponentBuilder()
                .WithButton("🔙 Back to Shop", "shop_tab_VikingRiseResources", ButtonStyle.Secondary)
                .Build();

            await ModifyOriginalResponseAsync(m =>
            {
                m.Embed = embed;
                m.Components = components;
            });
        }

        // =========================
        // CANCEL
        // =========================
        [ComponentInteraction("shop_cancel")]
        public async Task Cancel()
        {
            await DeferAsync();
            await ShowCategory(ShopCategory.VikingRiseResources);
        }

        // =========================
        // CONFIRM START AUCTION
        // =========================
        [ComponentInteraction("shop_auctionconfirm_*")]
        public async Task ConfirmAuction(string itemId)
        {
            await DeferAsync();

            if (!ShopRegistry.All.TryGetValue(itemId, out var item))
            {
                await ModifyOriginalResponseAsync(m => m.Content = "❌ Item not found.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("🔨 Start Auction")
                .WithDescription(
                    $"{item.Icon} **{item.Name}**\n\n" +
                    $"{item.Description}\n\n" +
                    $"💰 Starting Bid: **{item.StartingBid:N0} gold**\n\n" +
                    $"This gold will be held until the auction ends.\n" +
                    $"The auction will be posted publicly in <#{AuctionChannelId}>.\n" +
                    $"An admin will close the auction when ready.\n\n" +
                    $"Are you sure you want to start this auction?")
                .WithColor(Color.Orange)
                .Build();

            var components = new ComponentBuilder()
                .WithButton("✅ Start Auction", $"shop_startauction_{itemId}", ButtonStyle.Success)
                .WithButton("❌ Cancel", "shop_cancel", ButtonStyle.Secondary)
                .Build();

            await ModifyOriginalResponseAsync(m =>
            {
                m.Embed = embed;
                m.Components = components;
            });
        }

        // =========================
        // EXECUTE START AUCTION
        // =========================
        [ComponentInteraction("shop_startauction_*")]
        public async Task ExecuteStartAuction(string itemId)
        {
            await DeferAsync();

            var guild = (Context.Channel as SocketGuildChannel)?.Guild;
            if (guild == null)
            {
                await ModifyOriginalResponseAsync(m => m.Content = "❌ This command must be used in a server.");
                return;
            }

            var (success, message, auction) = await _shopService.StartAuctionAsync(
                Context.User.Id, itemId, guild, AuctionChannelId);

            if (!success)
            {
                await ModifyOriginalResponseAsync(m =>
                {
                    m.Embed = new EmbedBuilder()
                        .WithDescription(message)
                        .WithColor(Color.Red)
                        .Build();
                    m.Components = new ComponentBuilder()
                        .WithButton("🔙 Back to Shop", "shop_tab_VikingRiseRanks", ButtonStyle.Secondary)
                        .Build();
                });
                return;
            }

            if (!ShopRegistry.All.TryGetValue(itemId, out var item))
                return;

            var auctionChannel = guild.GetTextChannel(AuctionChannelId);
            if (auctionChannel != null)
            {
                var publicEmbed = BuildAuctionEmbed(auction, item);
                var publicMsg = await auctionChannel.SendMessageAsync(embed: publicEmbed);
                await _shopService.SetAuctionMessageAsync(auction.Id, publicMsg.Id);
            }

            await ModifyOriginalResponseAsync(m =>
            {
                m.Embed = new EmbedBuilder()
                    .WithTitle("✅ Auction Started!")
                    .WithDescription(
                        $"{item.Icon} **{item.Name}** is now live in <#{AuctionChannelId}>!\n\n" +
                        $"**Auction ID:** `{auction.Id}`\n" +
                        $"**Starting Bid:** {auction.CurrentBid:N0} gold\n\n" +
                        $"Others can use `/shopbid {auction.Id} <amount>` to outbid you.")
                    .WithColor(Color.Green)
                    .Build();
                m.Components = new ComponentBuilder()
                    .WithButton("🔙 Back to Shop", "shop_tab_VikingRiseRanks", ButtonStyle.Secondary)
                    .Build();
            });
        }

        // =========================
        // /shopbid — PLACE A BID
        // =========================
        [SlashCommand("shopbid", "Place a bid on an active auction")]
        public async Task ShopBid(int auctionId, int amount)
        {
            await DeferAsync(ephemeral: true);

            var guild = (Context.Channel as SocketGuildChannel)?.Guild;
            if (guild == null)
            {
                await FollowupAsync("❌ This command must be used in a server.", ephemeral: true);
                return;
            }

            var (success, message) = await _shopService.PlaceBidAsync(
                Context.User.Id, auctionId, amount, guild);

            if (success)
            {
                var auction = await _shopService.GetAuctionByIdAsync(auctionId);
                if (auction != null && ShopRegistry.All.TryGetValue(auction.ItemId, out var item))
                {
                    var auctionChannel = guild.GetTextChannel(AuctionChannelId);
                    if (auctionChannel != null && auction.MessageId != 0)
                    {
                        var msg = await auctionChannel.GetMessageAsync(auction.MessageId) as IUserMessage;
                        if (msg != null)
                            await msg.ModifyAsync(m => m.Embed = BuildAuctionEmbed(auction, item));
                    }
                }
            }

            await FollowupAsync(message, ephemeral: true);
        }

        // =========================
        // /shopendauction — ADMIN
        // =========================
        [SlashCommand("shopendauction", "End an active auction (Admin only)")]
        public async Task ShopEndAuction(int auctionId)
        {
            await DeferAsync(ephemeral: true);

            var guild = (Context.Channel as SocketGuildChannel)?.Guild;
            if (guild == null)
            {
                await FollowupAsync("❌ This command must be used in a server.", ephemeral: true);
                return;
            }

            var auction = await _shopService.GetAuctionByIdAsync(auctionId);

            var (success, message) = await _shopService.EndAuctionAsync(
                Context.User.Id, auctionId, guild);

            if (success && auction != null && ShopRegistry.All.TryGetValue(auction.ItemId, out var item))
            {
                var auctionChannel = guild.GetTextChannel(AuctionChannelId);
                if (auctionChannel != null && auction.MessageId != 0)
                {
                    var msg = await auctionChannel.GetMessageAsync(auction.MessageId) as IUserMessage;
                    if (msg != null)
                    {
                        var endedEmbed = new EmbedBuilder()
                            .WithTitle($"🏆 Auction Ended — {item.Icon} {item.Name}")
                            .WithDescription(
                                $"**Winner:** <@{auction.CurrentBidderDiscordId}>\n" +
                                $"**Winning Bid:** {auction.CurrentBid:N0} gold\n\n" +
                                $"An admin will be in touch shortly to deliver your reward.")
                            .WithColor(Color.Gold)
                            .Build();

                        await msg.ModifyAsync(m =>
                        {
                            m.Embed = endedEmbed;
                            m.Components = new ComponentBuilder().Build();
                        });
                    }
                }
            }

            await FollowupAsync(message, ephemeral: true);
        }

        // =========================
        // /shopfulfil — ADMIN
        // =========================
        [SlashCommand("shopfulfil", "Mark a purchase as fulfilled (Admin only)")]
        public async Task ShopFulfil(int purchaseId)
        {
            await DeferAsync(ephemeral: true);

            var guild = (Context.Channel as SocketGuildChannel)?.Guild;
            if (guild == null)
            {
                await FollowupAsync("❌ This command must be used in a server.", ephemeral: true);
                return;
            }

            var (success, message) = await _shopService.FulfilPurchaseAsync(
                Context.User.Id, purchaseId, guild);

            await FollowupAsync(message, ephemeral: true);
        }

        // =========================
        // /shoppending — ADMIN
        // =========================
        [SlashCommand("shoppending", "View pending shop purchases (Admin only)")]
        public async Task ShopPending()
        {
            await DeferAsync(ephemeral: true);

            var guild = (Context.Channel as SocketGuildChannel)?.Guild;
            var caller = guild?.GetUser(Context.User.Id);

            if (caller == null || !caller.Roles.Any(r => r.Id == AdminRoleId))
            {
                await FollowupAsync("❌ Admins only.", ephemeral: true);
                return;
            }

            var purchases = await _shopService.GetPendingPurchasesAsync();

            if (!purchases.Any())
            {
                await FollowupAsync("✅ No pending purchases.", ephemeral: true);
                return;
            }

            // Split into pages of 10 to stay under embed limits
            const int pageSize = 10;
            var pages = purchases.Chunk(pageSize).ToList();

            foreach (var page in pages)
            {
                var sb = new StringBuilder();

                foreach (var p in page)
                {
                    sb.AppendLine(
                        $"**#{p.Id}** — <@{p.BuyerDiscordId}>\n" +
                        $"  {p.ItemName} — **{p.GoldPaid:N0} gold**\n" +
                        $"  {p.PurchasedAt:dd MMM yyyy HH:mm} UTC — `/shopfulfil {p.Id}`\n");
                }

                var embed = new EmbedBuilder()
                    .WithTitle($"📋 Pending Purchases ({purchases.Count} total)")
                    .WithDescription(sb.ToString().Trim())
                    .WithColor(Color.Orange)
                    .Build();

                await FollowupAsync(embed: embed, ephemeral: true);
            }
        }

        // =========================
        // /shopauctions — VIEW ACTIVE
        // =========================
        [SlashCommand("shopauctions", "View all active auctions")]
        public async Task ShopAuctions()
        {
            await DeferAsync(ephemeral: true);

            var auctions = await _shopService.GetActiveAuctionsAsync();

            if (!auctions.Any())
            {
                await FollowupAsync("There are no active auctions right now.", ephemeral: true);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("🔨 **Active Auctions**\n");

            foreach (var a in auctions)
            {
                if (!ShopRegistry.All.TryGetValue(a.ItemId, out var item)) continue;

                sb.AppendLine(
                    $"**#{a.Id}** — {item.Icon} {item.Name}\n" +
                    $"  Current Bid: **{a.CurrentBid:N0} gold** by <@{a.CurrentBidderDiscordId}>\n" +
                    $"  Started: {a.StartedAt:dd MMM yyyy HH:mm} UTC\n" +
                    $"  Use `/shopbid {a.Id} <amount>` to outbid\n");
            }

            await FollowupAsync(sb.ToString(), ephemeral: true);
        }

        // =========================
        // MAIN RENDER
        // =========================
        private async Task ShowCategory(ShopCategory category)
        {
            var guild = (Context.Channel as SocketGuildChannel)?.Guild;
            var member = guild?.GetUser(Context.User.Id);

            var items = ShopRegistry.GetByCategory(category);

            var sb = new StringBuilder();

            foreach (var item in items)
            {
                bool locked = item.RequiredRoleId.HasValue &&
                              (member == null || !member.Roles.Any(r => r.Id == item.RequiredRoleId.Value));

                if (item.Type == ShopItemType.Auction)
                    sb.AppendLine($"{item.Icon} **{item.Name}** — Starting bid: **{item.StartingBid:N0} gold**{(locked ? " 🔒" : "")}");
                else
                    sb.AppendLine($"{item.Icon} **{item.Name}** — **{item.Price:N0} gold**{(locked ? " 🔒" : "")}");

                sb.AppendLine($"*{item.Description}*\n");
            }

            var embed = new EmbedBuilder()
                .WithTitle("🛒 Gold Shop")
                .WithDescription(sb.ToString())
                .WithColor(Color.Gold)
                .WithFooter("🔒 = Requires Viking Rise guild role")
                .Build();

            var builder = new ComponentBuilder()
                .WithButton("⚔️ VR Resources", "shop_tab_VikingRiseResources",
                    category == ShopCategory.VikingRiseResources ? ButtonStyle.Primary : ButtonStyle.Secondary, row: 0)
                .WithButton("🏆 VR Ranks", "shop_tab_VikingRiseRanks",
                    category == ShopCategory.VikingRiseRanks ? ButtonStyle.Primary : ButtonStyle.Secondary, row: 0)
                .WithButton("🎭 Discord", "shop_tab_DiscordRewards",
                    category == ShopCategory.DiscordRewards ? ButtonStyle.Primary : ButtonStyle.Secondary, row: 0)
                .WithButton("🎮 RPG Perks", "shop_tab_RpgPerks",
                    category == ShopCategory.RpgPerks ? ButtonStyle.Primary : ButtonStyle.Secondary, row: 0);

            int row = 1;
            int count = 0;

            foreach (var item in items)
            {
                bool locked = item.RequiredRoleId.HasValue &&
                              (member == null || !member.Roles.Any(r => r.Id == item.RequiredRoleId.Value));

                if (count > 0 && count % 5 == 0)
                    row++;

                if (row > 3) break;

                if (item.Type == ShopItemType.Auction)
                {
                    builder.WithButton(
                        $"{item.Icon} {item.Name}",
                        $"shop_auctionconfirm_{item.Id}",
                        ButtonStyle.Primary,
                        disabled: locked,
                        row: row);
                }
                else
                {
                    builder.WithButton(
                        $"{item.Icon} {item.Name}",
                        $"shop_confirm_{item.Id}",
                        ButtonStyle.Success,
                        disabled: locked,
                        row: row);
                }

                count++;
            }

            if (Context.Interaction is SocketMessageComponent)
            {
                await ModifyOriginalResponseAsync(m =>
                {
                    m.Embed = embed;
                    m.Components = builder.Build();
                });
            }
            else
            {
                await FollowupAsync(embed: embed, components: builder.Build(), ephemeral: true);
            }
        }

        // =========================
        // AUCTION EMBED HELPER
        // =========================
        private Embed BuildAuctionEmbed(ActiveAuction auction, ShopItemDefinition item)
        {
            return new EmbedBuilder()
                .WithTitle($"🔨 {item.Icon} {item.Name} — Live Auction")
                .WithDescription(item.Description)
                .AddField("💰 Current Bid", $"**{auction.CurrentBid:N0} gold**", true)
                .AddField("👤 Highest Bidder", $"<@{auction.CurrentBidderDiscordId}>", true)
                .AddField("🏁 Starting Bid", $"{auction.StartingBid:N0} gold", false)
                .AddField("📋 How to Bid",
                    $"Use `/shopbid {auction.Id} <amount>` to place a bid.\n" +
                    $"You must bid higher than the current amount.\n" +
                    $"Outbid players receive their gold back instantly.", false)
                .WithColor(Color.Orange)
                .WithFooter($"Auction ID: {auction.Id} | Started by {auction.StartedByDiscordId}")
                .WithTimestamp(auction.StartedAt)
                .Build();
        }
    }
}