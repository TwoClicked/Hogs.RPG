using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.Entities.EnhancementObjects;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.GameData.Enhancement;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.EnhancementServices;
using Hogs.RPG.Services.InventoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    [Group("enhance", "Enhance your Global Boss Gear")]
    [BossLock]
    [GearSwapLock]
    [TradeLock]
    public class EnhanceModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly EnhancementService _enhancementService;
        private readonly InventoryService _inventoryService;
        private readonly PlayerRepository _playerRepository;

        public EnhanceModule(
            EnhancementService enhancementService,
            InventoryService inventoryService,
            PlayerRepository playerRepository)
        {
            _enhancementService = enhancementService;
            _inventoryService = inventoryService;
            _playerRepository = playerRepository;
        }

        // Shared slot -> display name/icon, matching EquipSlotAutocompleteHandler's set
        private static readonly Dictionary<EquipmentSlot, (string Icon, string Name)> SlotDisplay = new()
        {
            { EquipmentSlot.MainHand, ("🗡", "Main Hand") },
            { EquipmentSlot.OffHand, ("🏹", "Off Hand") },
            { EquipmentSlot.Helmet, ("🪖", "Helmet") },
            { EquipmentSlot.Body, ("🛡", "Body") },
            { EquipmentSlot.Legs, ("👖", "Legs") },
            { EquipmentSlot.Gloves, ("🧤", "Gloves") },
            { EquipmentSlot.Boots, ("🥾", "Boots") },
            { EquipmentSlot.Ring, ("💍", "Ring") },
            { EquipmentSlot.Amulet, ("📿", "Amulet") }
        };

        private static string SlotLabel(EquipmentSlot slot) =>
            $"{SlotDisplay[slot].Icon} {SlotDisplay[slot].Name}";

        // =========================
        // ATTEMPT
        // =========================
        [SlashCommand("attempt", "Attempt to enhance a Global Boss Gear slot")]
        public async Task Attempt(
            [Autocomplete(typeof(EquipSlotAutocompleteHandler))] string slot,
            [Summary("cronstones", "Cron Stones to spend boosting this attempt (optional)")] int cronstones = 0)
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("⚠️ You need to start your adventure first with `/startadventure`.", ephemeral: true);
                return;
            }

            if (!Enum.TryParse<EquipmentSlot>(slot, out var equipSlot))
            {
                await FollowupAsync("⚠️ Unknown slot — please pick one from the list.", ephemeral: true);
                return;
            }

            var preview = await _enhancementService.GetAttemptPreviewAsync(Context.User.Id, equipSlot, cronstones);

            string currentLabel = EnhancementLevelLabels.GetLabel(preview.CurrentLevel);
            string targetLabel = EnhancementLevelLabels.GetLabel(preview.TargetLevel);

            var embed = new EmbedBuilder()
                .WithTitle($"🔨 Enhance — {SlotLabel(equipSlot)}")
                .WithColor(preview.CanAttempt ? Color.Gold : Color.DarkRed);

            if (preview.IsMaxLevel)
            {
                embed.WithDescription($"This slot is already at **PEN** — there's nowhere further to go.");
                await FollowupAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            embed.AddField("Progress",
                $"{(string.IsNullOrEmpty(currentLabel) ? "Base" : currentLabel)} → **{targetLabel}**", false);

            embed.AddField("<:BlackStone:1541556030855577650> Blackstones",
                $"{preview.BlackstoneCost} needed — you have {preview.BlackstonesOwned}", true);

            if (preview.RequiresConcentratedBlackstone)
            {
                embed.AddField("⬛ Concentrated Blackstone",
                    preview.HasConcentratedBlackstone ? "✅ Ready" : "❌ Missing — craft with `/enhance craft`", true);
            }

            embed.AddField("<:Cronstone:1541556705052074074> Cron Stones applied",
                $"{preview.CronStonesToUse} (+{preview.BonusSuccessPercent:0.##}%) — you have {preview.CronStonesOwned}", true);

            embed.AddField("🎯 Success Chance",
                $"**{preview.EffectiveSuccessPercent:0.##}%**", false);

            if (!preview.CanAttempt)
            {
                embed.AddField("⚠️ Cannot attempt", preview.BlockedReason ?? "Requirements not met.", false);
                await FollowupAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            var components = new ComponentBuilder()
                .WithButton("✅ Confirm", $"enhance_confirm:{slot}:{preview.CronStonesToUse}", ButtonStyle.Success)
                .WithButton("❌ Cancel", "enhance_cancel", ButtonStyle.Secondary);

            await FollowupAsync(embed: embed.Build(), components: components.Build(), ephemeral: true);
        }

        // =========================
        // BAG
        // =========================
        [SlashCommand("bag", "View your enhancement materials")]
        public async Task Bag()
        {
            await DeferAsync(ephemeral: true);

            var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);
            var qty = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            int Get(string id) => qty.TryGetValue(id, out var amount) ? amount : 0;

            var embed = new EmbedBuilder()
                .WithTitle("🎒 Enhancement Bag")
                .WithColor(Color.DarkPurple);

            embed.AddField("🪨 Currency",
                $"{EnhancementItems.Blackstone.Icon} Blackstone x{Get(EnhancementItems.Blackstone.Id)}\n" +
                $"{EnhancementItems.CronStone.Icon} Cron Stone x{Get(EnhancementItems.CronStone.Id)}",
                false);

            embed.AddField("🔮 Components",
                $"{EnhancementItems.InfuseCrystal.Icon} Infuse Crystal x{Get(EnhancementItems.InfuseCrystal.Id)}",
                false);

            var upgradeLines = Enum.GetValues<EquipmentSlot>()
                .Select(s => (Slot: s, Amount: Get(EnhancementSlotMap.GetUpgradePieceItemId(s))))
                .Where(x => x.Amount > 0)
                .Select(x => $"🧩 {SlotLabel(x.Slot)} x{x.Amount}")
                .ToList();

            embed.AddField("Upgrade Pieces",
                upgradeLines.Count > 0 ? string.Join("\n", upgradeLines) : "None yet.", false);

            var concentratedLines = Enum.GetValues<EquipmentSlot>()
                .Select(s => (Slot: s, Amount: Get(EnhancementSlotMap.GetConcentratedBlackstoneItemId(s))))
                .Where(x => x.Amount > 0)
                .Select(x => $"⬛ {SlotLabel(x.Slot)} x{x.Amount}")
                .ToList();

            embed.AddField("Concentrated Blackstones",
                concentratedLines.Count > 0 ? string.Join("\n", concentratedLines) : "None yet.", false);

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        // =========================
        // CRAFT
        // =========================
        [SlashCommand("craft", "Combine an Upgrade Piece + Infuse Crystal into a Concentrated Blackstone")]
        public async Task Craft(
            [Autocomplete(typeof(EquipSlotAutocompleteHandler))] string slot)
        {
            await DeferAsync(ephemeral: true);

            if (!Enum.TryParse<EquipmentSlot>(slot, out var equipSlot))
            {
                await FollowupAsync("⚠️ Unknown slot — please pick one from the list.", ephemeral: true);
                return;
            }

            string upgradePieceId = EnhancementSlotMap.GetUpgradePieceItemId(equipSlot);
            int upgradePieces = await _inventoryService.GetItemAmountAsync(Context.User.Id, upgradePieceId);
            int infuseCrystals = await _inventoryService.GetItemAmountAsync(Context.User.Id, InventoryItemDefinitions.All[EnhancementItems.InfuseCrystal.Id].Id);

            var embed = new EmbedBuilder()
                .WithTitle($"🔮 Craft — Concentrated Blackstone ({SlotDisplay[equipSlot].Name})")
                .AddField("🧩 Upgrade Piece", $"{upgradePieces} owned", true)
                .AddField("🔮 Infuse Crystal", $"{infuseCrystals} owned", true);

            if (upgradePieces < 1 || infuseCrystals < 1)
            {
                embed.WithColor(Color.DarkRed);
                embed.AddField("⚠️ Cannot craft", "You need at least 1 of each.", false);
                await FollowupAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            embed.WithColor(Color.Purple);

            var components = new ComponentBuilder()
                .WithButton("✅ Confirm", $"enhance_craft_confirm:{slot}", ButtonStyle.Success)
                .WithButton("❌ Cancel", "enhance_cancel", ButtonStyle.Secondary);

            await FollowupAsync(embed: embed.Build(), components: components.Build(), ephemeral: true);
        }
    }

    // =========================
    // ENHANCE INTERACTION MODULE
    // Component interactions must NOT be in a [Group] module — this is
    // why Confirm/Cancel buttons weren't firing. Same split TrailModule
    // already uses (TrailModule + TrailInteractionModule).
    // =========================
    public class EnhanceInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly EnhancementService _enhancementService;

        public EnhanceInteractionModule(EnhancementService enhancementService)
        {
            _enhancementService = enhancementService;
        }

        private static readonly Dictionary<EquipmentSlot, (string Icon, string Name)> SlotDisplay = new()
        {
            { EquipmentSlot.MainHand, ("🗡", "Main Hand") },
            { EquipmentSlot.OffHand, ("🏹", "Off Hand") },
            { EquipmentSlot.Helmet, ("🪖", "Helmet") },
            { EquipmentSlot.Body, ("🛡", "Body") },
            { EquipmentSlot.Legs, ("👖", "Legs") },
            { EquipmentSlot.Gloves, ("🧤", "Gloves") },
            { EquipmentSlot.Boots, ("🥾", "Boots") },
            { EquipmentSlot.Ring, ("💍", "Ring") },
            { EquipmentSlot.Amulet, ("📿", "Amulet") }
        };

        private static string SlotLabel(EquipmentSlot slot) =>
            $"{SlotDisplay[slot].Icon} {SlotDisplay[slot].Name}";

        [ComponentInteraction("enhance_confirm:*:*")]
        public async Task ConfirmEnhance(string slot, string cronstonesStr)
        {
            if (Context.Interaction is not SocketMessageComponent component)
                return;

            // Strip the buttons immediately so a second click has nothing to fire —
            // same anti-double-click pattern used by equip_confirm.
            await component.UpdateAsync(msg =>
            {
                msg.Content = "⏳ Rolling...";
                msg.Embed = null;
                msg.Components = new ComponentBuilder().Build();
            });

            if (!Enum.TryParse<EquipmentSlot>(slot, out var equipSlot) || !int.TryParse(cronstonesStr, out int cronstones))
            {
                await component.ModifyOriginalResponseAsync(msg => msg.Content = "⚠️ Something went wrong reading that attempt.");
                return;
            }

            var result = await _enhancementService.AttemptEnhanceAsync(Context.User.Id, equipSlot, cronstones);

            if (!result.Success)
            {
                await component.ModifyOriginalResponseAsync(msg => msg.Content = $"⚠️ {result.FailureReason}");
                return;
            }

            string previousLabel = EnhancementLevelLabels.GetLabel(result.PreviousLevel);
            string newLabel = EnhancementLevelLabels.GetLabel(result.NewLevel);

            string resultText;
            if (result.RollSucceeded)
            {
                var (atk, def, hp) = EnhancementStatGains.GetGainForLevel(result.NewLevel);
                resultText =
                    $"✅ **Success!** {SlotLabel(equipSlot)} is now **{newLabel}**!\n" +
                    $"+{atk} ATK / +{def} DEF / +{hp} HP\n\n" +
                    $"Spent {result.BlackstonesSpent} Blackstones" +
                    (result.CronStonesSpent > 0 ? $", {result.CronStonesSpent} Cron Stones" : "") +
                    (result.ConcentratedBlackstoneConsumed ? ", 1 Concentrated Blackstone" : "") + ".";
            }
            else
            {
                resultText =
                    $"❌ **Failed.** {SlotLabel(equipSlot)} remains at {(string.IsNullOrEmpty(previousLabel) ? "Base" : previousLabel)}.\n\n" +
                    $"Spent {result.BlackstonesSpent} Blackstones" +
                    (result.CronStonesSpent > 0 ? $", {result.CronStonesSpent} Cron Stones" : "") +
                    (result.ConcentratedBlackstoneConsumed ? ", 1 Concentrated Blackstone" : "") + "." +
                    (result.UpgradePieceRefunded ? "\n🧩 Your Upgrade Piece has been refunded." : "");
            }

            await component.ModifyOriginalResponseAsync(msg => msg.Content = resultText);
        }

        [ComponentInteraction("enhance_cancel")]
        public async Task CancelEnhance()
        {
            if (Context.Interaction is SocketMessageComponent component)
            {
                await component.UpdateAsync(msg =>
                {
                    msg.Content = "❌ Enhancement cancelled.";
                    msg.Embed = null;
                    msg.Components = new ComponentBuilder().Build();
                });
            }
        }

        [ComponentInteraction("enhance_craft_confirm:*")]
        public async Task ConfirmCraft(string slot)
        {
            if (Context.Interaction is not SocketMessageComponent component)
                return;

            await component.UpdateAsync(msg =>
            {
                msg.Content = "⏳ Crafting...";
                msg.Embed = null;
                msg.Components = new ComponentBuilder().Build();
            });

            if (!Enum.TryParse<EquipmentSlot>(slot, out var equipSlot))
            {
                await component.ModifyOriginalResponseAsync(msg => msg.Content = "⚠️ Something went wrong reading that craft.");
                return;
            }

            var (success, failureReason) = await _enhancementService.CraftConcentratedBlackstoneAsync(Context.User.Id, equipSlot);

            string resultText = success
                ? $"✅ Crafted a Concentrated Blackstone ({SlotDisplay[equipSlot].Name})!"
                : $"⚠️ {failureReason}";

            await component.ModifyOriginalResponseAsync(msg => msg.Content = resultText);
        }
    }
}