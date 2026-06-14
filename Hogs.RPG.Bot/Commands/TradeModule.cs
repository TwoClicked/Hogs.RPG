using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.Entities.TradeObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.RelicServices;
using Hogs.RPG.Services.TradeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    [BossLock]
    [GearSwapLock]
    public class TradeModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TradeService _tradeService;
        private readonly InventoryService _inventoryService;
        private readonly PlayerService _playerService;
        private readonly RelicService _relicService;

        private const ulong TRADE_CHANNEL_ID = 1489405758603923477;

        public TradeModule(
            TradeService tradeService,
            InventoryService inventoryService,
            PlayerService playerService,
            RelicService relicService)
        {
            _tradeService = tradeService;
            _inventoryService = inventoryService;
            _playerService = playerService;
            _relicService = relicService;
        }

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

        private Embed BuildTradeEmbed(TradeSession trade)
        {
            string Format(Dictionary<string, int> offer, int gold, List<int> pets, List<int> relics)
            {
                var lines = new List<string>();

                if (gold > 0)
                    lines.Add($"💰 Gold: {gold}");

                if (pets.Any())
                    lines.AddRange(pets.Select(p => $"🐾 Pet #{p}"));

                if (relics.Any())
                    lines.AddRange(relics.Select(r => $"💎 Relic #{r}"));

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
                    Format(trade.Player1Offer, trade.Player1Gold, trade.Player1Pets, trade.Player1Relics),
                    true)
                .AddField(
                    $"👤 <@{trade.Player2Id}> {(trade.Player2Confirmed ? "🔒" : "🔓")}",
                    Format(trade.Player2Offer, trade.Player2Gold, trade.Player2Pets, trade.Player2Relics),
                    true)
                .WithFooter("🔒 = Confirmed | 🔓 = Not Confirmed")
                .Build();
        }

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
                $"Use `/tradeadd`, `/tradeaddpet`, `/tradeaddrelic`, or `/tradeaddgold`.");
        }

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

            if (trade.State == TradeState.Confirming)
                trade.State = TradeState.Active;

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

            if (player.Gold < amount)
            {
                await RespondAsync($"You only have **{player.Gold} gold**.", ephemeral: true);
                return;
            }

            if (trade.State == TradeState.Confirming)
                trade.State = TradeState.Active;

            trade.LastUpdatedAt = DateTime.UtcNow;
            trade.WarningSent = false;

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

        [SlashCommand("tradeaddpet", "Add pet to trade")]
        public async Task TradeAddPet(int petId)
        {
            if (!await EnsureTradeChannel()) return;

            var trade = _tradeService.GetTrade(Context.User.Id);

            if (trade == null || trade.State != TradeState.Active)
            {
                await RespondAsync("No active trade.", ephemeral: true);
                return;
            }

            var pets = await _playerService.GetPlayerPets(Context.User.Id);
            var pet = pets.FirstOrDefault(p => p.Id == petId);

            if (pet == null)
            {
                await RespondAsync("You do not own this pet.", ephemeral: true);
                return;
            }

            if (pet.IsEquipped)
            {
                await RespondAsync("You cannot trade an equipped pet.", ephemeral: true);
                return;
            }

            if (trade.State == TradeState.Confirming)
                trade.State = TradeState.Active;

            trade.LastUpdatedAt = DateTime.UtcNow;
            trade.WarningSent = false;

            var list = Context.User.Id == trade.Player1Id
                ? trade.Player1Pets
                : trade.Player2Pets;

            if (list.Contains(petId))
            {
                await RespondAsync("Pet already added.", ephemeral: true);
                return;
            }

            list.Add(petId);

            trade.Player1Confirmed = false;
            trade.Player2Confirmed = false;

            await RespondAsync(
                $"🐾 <@{Context.User.Id}> added pet **#{petId}**",
                embed: BuildTradeEmbed(trade));
        }

        // =========================
        // /tradeaddrelic
        // =========================
        [SlashCommand("tradeaddrelic", "Add a relic to the trade")]
        public async Task TradeAddRelic(
            [Summary("relic_id", "The relic to add"), Autocomplete(typeof(RelicAutocompleteHandler))] int relicId)
        {
            if (!await EnsureTradeChannel()) return;

            var trade = _tradeService.GetTrade(Context.User.Id);

            if (trade == null || trade.State != TradeState.Active)
            {
                await RespondAsync("No active trade.", ephemeral: true);
                return;
            }

            var relic = await _relicService.GetRelicByIdAsync(relicId);

            if (relic == null || relic.DiscordId != Context.User.Id)
            {
                await RespondAsync("❌ You do not own this relic.", ephemeral: true);
                return;
            }

            if (relic.IsEquipped)
            {
                await RespondAsync("❌ Unequip the relic before adding it to a trade.", ephemeral: true);
                return;
            }

            if (trade.State == TradeState.Confirming)
                trade.State = TradeState.Active;

            trade.LastUpdatedAt = DateTime.UtcNow;
            trade.WarningSent = false;

            var list = Context.User.Id == trade.Player1Id
                ? trade.Player1Relics
                : trade.Player2Relics;

            if (list.Contains(relicId))
            {
                await RespondAsync("❌ Relic already added to trade.", ephemeral: true);
                return;
            }

            list.Add(relicId);

            trade.Player1Confirmed = false;
            trade.Player2Confirmed = false;

            var def = RelicRegistry.Get(relic.RelicId);

            await RespondAsync(
                $"💎 <@{Context.User.Id}> added **{def.Name}** (Rank {relic.Rank})",
                embed: BuildTradeEmbed(trade));
        }

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

            if (!trade.Player1Offer.Any() && !trade.Player2Offer.Any()
                && !trade.Player1Pets.Any() && !trade.Player2Pets.Any()
                && !trade.Player1Relics.Any() && !trade.Player2Relics.Any()
                && trade.Player1Gold == 0 && trade.Player2Gold == 0)
            {
                await RespondAsync("You cannot confirm an empty trade.", ephemeral: true);
                return;
            }

            if (Context.User.Id == trade.Player1Id)
                trade.Player1Confirmed = true;
            else
                trade.Player2Confirmed = true;

            if (trade.State == TradeState.Active)
            {
                trade.State = TradeState.Confirming;

                await RespondAsync(embed: BuildTradeEmbed(trade));
                await FollowupAsync("⚠️ Trade locked in. Confirm again to finalize.");
                return;
            }

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

            await RespondAsync(
                $"❌ Trade between <@{trade.Player1Id}> and <@{trade.Player2Id}> has been cancelled.");

            _tradeService.RemoveTrade(trade);
        }

        private async Task ExecuteTrade(TradeSession trade)
        {
            // ✅ VALIDATE ITEMS
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

            // ✅ VALIDATE GOLD
            var p1 = await _playerService.GetPlayerAsync(trade.Player1Id);
            var p2 = await _playerService.GetPlayerAsync(trade.Player2Id);

            if (p1.Gold < trade.Player1Gold)
                throw new Exception("Player 1 no longer has enough gold.");

            if (p2.Gold < trade.Player2Gold)
                throw new Exception("Player 2 no longer has enough gold.");

            // ✅ VALIDATE PETS
            var p1Pets = await _playerService.GetPlayerPets(trade.Player1Id);
            var p2Pets = await _playerService.GetPlayerPets(trade.Player2Id);

            foreach (var petId in trade.Player1Pets)
            {
                var pet = p1Pets.FirstOrDefault(p => p.Id == petId);
                if (pet == null)
                    throw new Exception($"Player 1 no longer owns pet #{petId}.");
                if (pet.IsEquipped)
                    throw new Exception($"Player 1 has pet #{petId} equipped.");
            }

            foreach (var petId in trade.Player2Pets)
            {
                var pet = p2Pets.FirstOrDefault(p => p.Id == petId);
                if (pet == null)
                    throw new Exception($"Player 2 no longer owns pet #{petId}.");
                if (pet.IsEquipped)
                    throw new Exception($"Player 2 has pet #{petId} equipped.");
            }

            // ✅ VALIDATE RELICS
            foreach (var relicId in trade.Player1Relics)
            {
                var relic = await _relicService.GetRelicByIdAsync(relicId);
                if (relic == null || relic.DiscordId != trade.Player1Id)
                    throw new Exception($"Player 1 no longer owns relic #{relicId}.");
                if (relic.IsEquipped)
                    throw new Exception($"Player 1 has relic #{relicId} equipped. Unequip it first.");
            }

            foreach (var relicId in trade.Player2Relics)
            {
                var relic = await _relicService.GetRelicByIdAsync(relicId);
                if (relic == null || relic.DiscordId != trade.Player2Id)
                    throw new Exception($"Player 2 no longer owns relic #{relicId}.");
                if (relic.IsEquipped)
                    throw new Exception($"Player 2 has relic #{relicId} equipped. Unequip it first.");
            }

            // ✅ TRANSFER GOLD
            p1.Gold -= trade.Player1Gold;
            p2.Gold += trade.Player1Gold;
            p2.Gold -= trade.Player2Gold;
            p1.Gold += trade.Player2Gold;

            await _playerService.UpdatePlayerAsync(p1);
            await _playerService.UpdatePlayerAsync(p2);

            // ✅ TRANSFER ITEMS
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

            // ✅ TRANSFER PETS
            foreach (var petId in trade.Player1Pets)
                await _playerService.TransferPet(petId, trade.Player1Id, trade.Player2Id);

            foreach (var petId in trade.Player2Pets)
                await _playerService.TransferPet(petId, trade.Player2Id, trade.Player1Id);

            // ✅ TRANSFER RELICS
            foreach (var relicId in trade.Player1Relics)
            {
                var relic = await _relicService.GetRelicByIdAsync(relicId);
                if (relic != null)
                {
                    relic.DiscordId = trade.Player2Id;
                    await _relicService.SaveRelicAsync(relic);
                }
            }

            foreach (var relicId in trade.Player2Relics)
            {
                var relic = await _relicService.GetRelicByIdAsync(relicId);
                if (relic != null)
                {
                    relic.DiscordId = trade.Player1Id;
                    await _relicService.SaveRelicAsync(relic);
                }
            }
        }
    }
}