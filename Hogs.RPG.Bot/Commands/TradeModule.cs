using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.TradeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    public class TradeModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TradeService _tradeService;
        private readonly InventoryService _inventoryService;
        private readonly PlayerService _playerService;

        private const ulong TRADE_CHANNEL_ID = 1489405758603923477;

        public TradeModule(TradeService tradeService, InventoryService inventoryService, PlayerService playerService)
        {
            _tradeService = tradeService;
            _inventoryService = inventoryService;
            _playerService = playerService;

        }

        /// <summary>
        /// Ensures trade commands are only used in the designated trade channel
        /// </summary>
        private async Task<bool> EnsureTradeChannel()
        {
            if (Context.Channel.Id != TRADE_CHANNEL_ID)
            {
                await RespondAsync(
                    $"❌ Trading can only be done in <#{TRADE_CHANNEL_ID}>.",
                    ephemeral: true);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Builds the trade embed UI
        /// </summary>
        private Embed BuildTradeEmbed(TradeSession trade)
        {
            string Format(Dictionary<string, int> offer, int gold)
            {
                var lines = new List<string>();

                // 💰 GOLD FIRST
                if (gold > 0)
                    lines.Add($"💰 Gold: {gold}");

                // 📦 ITEMS
                if (offer.Any())
                {
                    lines.AddRange(offer.Select(x =>
                    {
                        var def = InventoryItemDefinitions.All.TryGetValue(x.Key, out var d) ? d : null;
                        var name = def?.Name ?? x.Key;
                        var icon = def?.Icon ?? "📦";

                        return $"{icon} {name} x{x.Value}";
                    }));
                }

                if (!lines.Any())
                    return "Nothing";

                return string.Join("\n", lines);
            }

            return new EmbedBuilder()
                .WithTitle("📦 Trade Session")
                .WithColor(Color.DarkBlue)
                .AddField(
                    $"👤 <@{trade.Player1Id}> {(trade.Player1Confirmed ? "🔒" : "🔓")}",
                    Format(trade.Player1Offer, trade.Player1Gold),
                    true)
                .AddField(
                    $"👤 <@{trade.Player2Id}> {(trade.Player2Confirmed ? "🔒" : "🔓")}",
                    Format(trade.Player2Offer, trade.Player2Gold),
                    true)
                .WithFooter("🔒 = Confirmed | 🔓 = Not Confirmed")
                .Build();
        }

        /// <summary>
        /// Starts a trade request with another player (Pending state)
        /// </summary>
        [SlashCommand("trade", "Request a trade with another player")]
        public async Task Trade(SocketGuildUser target)
        {
            if (!await EnsureTradeChannel()) return;

            if (target.Id == Context.User.Id)
            {
                await RespondAsync("You cannot trade with yourself.", ephemeral: true);
                return;
            }

            if (_tradeService.HasActiveTrade(Context.User.Id))
            {
                await RespondAsync("You are already in a trade.", ephemeral: true);
                return;
            }

            if (_tradeService.HasActiveTrade(target.Id))
            {
                await RespondAsync("That player is already in a trade.", ephemeral: true);
                return;
            }

            _tradeService.CreateTrade(Context.User.Id, target.Id);

            await RespondAsync(
                $"📦 {target.Mention}, {Context.User.Mention} wants to trade.\n" +
                $"Use `/tradeaccept` to accept.");
        }


        /// <summary>
        /// Adds gold to the current trade
        /// </summary>
        [SlashCommand("tradeaddgold", "Add gold to trade")]
        public async Task TradeAddGold(int amount)
        {
            if (!await EnsureTradeChannel()) return;

            var trade = _tradeService.GetTrade(Context.User.Id);

            if (trade == null || trade.State != TradeState.Active)
            {
                await RespondAsync("No active trade.", ephemeral: true);
                return;
            }

            if (amount <= 0)
            {
                await RespondAsync("Invalid amount.", ephemeral: true);
                return;
            }

            var player = await _playerService.GetPlayerAsync(Context.User.Id);
            trade.WarningSent = false;
            trade.LastUpdatedAt = DateTime.UtcNow;

            if (player.Gold < amount)
            {
                await RespondAsync($"You only have **{player.Gold} gold**.", ephemeral: true);
                return;
            }

            // Reset confirm state if needed
            if (trade.State == TradeState.Confirming)
                trade.State = TradeState.Active;

            if (Context.User.Id == trade.Player1Id)
                trade.Player1Gold += amount;
            else
                trade.Player2Gold += amount;

            trade.Player1Confirmed = false;
            trade.Player2Confirmed = false;

            await RespondAsync(
                $"💰 <@{Context.User.Id}> added **{amount} gold**",
                embed: BuildTradeEmbed(trade));
        }

        /// <summary>
        /// Cancels the current trade session for both players
        /// </summary>
        [SlashCommand("tradecancel", "Cancel the current trade")]
        public async Task TradeCancel()
        {
            if (!await EnsureTradeChannel()) return;

            var trade = _tradeService.GetTrade(Context.User.Id);

            if (trade == null)
            {
                await RespondAsync("You are not in a trade.", ephemeral: true);
                return;
            }

            trade.LastUpdatedAt = DateTime.UtcNow;
            trade.WarningSent = false;

            // Notify both players
            await RespondAsync(
                $"❌ Trade between <@{trade.Player1Id}> and <@{trade.Player2Id}> has been cancelled.");

            // Remove trade session
            _tradeService.RemoveTrade(trade);
        }


        /// <summary>
        /// Accepts a pending trade request and activates the trade
        /// </summary>
        [SlashCommand("tradeaccept", "Accept a trade request")]
        public async Task TradeAccept()
        {
            if (!await EnsureTradeChannel()) return;

            var trade = _tradeService.GetTrade(Context.User.Id);

            if (trade == null)
            {
                await RespondAsync("No trade request found.", ephemeral: true);
                return;
            }

            if (trade.State != TradeState.Pending)
            {
                await RespondAsync("This trade is already active.", ephemeral: true);
                return;
            }

            if (Context.User.Id != trade.Player2Id)
            {
                await RespondAsync("You are not the one being invited to this trade.", ephemeral: true);
                return;
            }

            trade.LastUpdatedAt = DateTime.UtcNow;
            trade.WarningSent = false;
            trade.State = TradeState.Active;

            await RespondAsync(
                $"✅ Trade started between <@{trade.Player1Id}> and <@{trade.Player2Id}>!\n" +
                $"Use `/tradeadd` to add items.");
        }

        /// <summary>
        /// Adds an item to the current trade offer
        /// </summary>
        [SlashCommand("tradeadd", "Add item to trade")]
        public async Task TradeAdd(
            [Autocomplete(typeof(TradeItemAutocompleteHandler))] string itemId,
            int amount)
        {
            if (!await EnsureTradeChannel()) return;

            var trade = _tradeService.GetTrade(Context.User.Id);

            if (trade == null || trade.State != TradeState.Active)
            {
                await RespondAsync("No active trade.", ephemeral: true);
                return;
            }

            var owned = await _inventoryService.GetItemAmountAsync(Context.User.Id, itemId);

            if (owned <= 0)
            {
                await RespondAsync("You do not own this item.", ephemeral: true);
                return;
            }

            if (amount <= 0 || amount > owned)
            {
                await RespondAsync($"Invalid amount. You own **{owned}**.", ephemeral: true);
                return;
            }

            // If modifying after confirm → reset state
            if (trade.State == TradeState.Confirming)
            {
                trade.State = TradeState.Active;
            }
            trade.LastUpdatedAt = DateTime.UtcNow;
            trade.WarningSent = false;
            var isPlayer1 = trade.Player1Id == Context.User.Id;
            var offer = isPlayer1 ? trade.Player1Offer : trade.Player2Offer;

            offer[itemId] = offer.ContainsKey(itemId)
                ? offer[itemId] + amount
                : amount;

            trade.Player1Confirmed = false;
            trade.Player2Confirmed = false;

            var name = InventoryItemDefinitions.All.TryGetValue(itemId, out var def)
                ? def.Name
                : itemId;

            await RespondAsync(
                $"➕ <@{Context.User.Id}> added **{amount}x {name}**",
                embed: BuildTradeEmbed(trade));
        }

        /// <summary>
        /// Displays both players' trade offers
        /// </summary>
        [SlashCommand("tradeview", "View current trade")]
        public async Task TradeView()
        {
            if (!await EnsureTradeChannel()) return;

            var trade = _tradeService.GetTrade(Context.User.Id);

            if (trade == null)
            {
                await RespondAsync("No active trade.", ephemeral: true);
                return;
            }

            await RespondAsync(embed: BuildTradeEmbed(trade));
        }

        /// <summary>
        /// Handles double confirmation logic and completes the trade
        /// </summary>
        [SlashCommand("tradeconfirm", "Confirm the trade")]
        public async Task TradeConfirm()
        {
            if (!await EnsureTradeChannel()) return;

            var trade = _tradeService.GetTrade(Context.User.Id);

            if (trade == null || !(trade.State == TradeState.Active || trade.State == TradeState.Confirming))
            {
                await RespondAsync("No active trade.", ephemeral: true);
                return;
            }

            // Prevent empty trades
            if (!trade.Player1Offer.Any() && !trade.Player2Offer.Any())
            {
                await RespondAsync("You cannot confirm an empty trade.", ephemeral: true);
                return;
            }

            // Mark confirmation
            if (Context.User.Id == trade.Player1Id)
                trade.Player1Confirmed = true;
            else
                trade.Player2Confirmed = true;

            // FIRST CONFIRM
            if (trade.State == TradeState.Active)
            {
                trade.State = TradeState.Confirming;

                await RespondAsync(embed: BuildTradeEmbed(trade));
                await FollowupAsync("⚠️ Trade locked in. Confirm again to finalize.");

                return;
            }

            // SECOND CONFIRM
            if (trade.Player1Confirmed && trade.Player2Confirmed)
            {
                try
                {
                    await ExecuteTrade(trade);

                    _tradeService.RemoveTrade(trade);

                    await RespondAsync("🎉 Trade completed successfully!");
                }
                catch (Exception ex)
                {
                    await RespondAsync($"❌ Trade failed: {ex.Message}");
                }
            }
            else
            {
                await RespondAsync("🔒 You confirmed. Waiting for the other player...");
            }
        }

        /// <summary>
        /// Executes the trade safely with full validation (anti-duplication)
        /// </summary>
        private async Task ExecuteTrade(TradeSession trade)
        {
            foreach (var item in trade.Player1Offer)
            {
                var owned = await _inventoryService.GetItemAmountAsync(trade.Player1Id, item.Key);
                if (owned < item.Value)
                    throw new Exception("Player 1 no longer has required items.");
            }

            foreach (var item in trade.Player2Offer)
            {
                var owned = await _inventoryService.GetItemAmountAsync(trade.Player2Id, item.Key);
                if (owned < item.Value)
                    throw new Exception("Player 2 no longer has required items.");
            }

            foreach (var item in trade.Player1Offer)
            {
                await _inventoryService.TakeItemAsync(trade.Player1Id, item.Key, item.Value);
                await _inventoryService.GiveItemAsync(trade.Player2Id, item.Key, item.Value);
            }

            foreach (var item in trade.Player2Offer)
            {
                await _inventoryService.TakeItemAsync(trade.Player2Id, item.Key, item.Value);
                await _inventoryService.GiveItemAsync(trade.Player1Id, item.Key, item.Value);
            }
        }
    }
}