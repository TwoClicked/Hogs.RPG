using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.SmithingServices;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    [Group("blacksmith", "Blacksmith commands — smelt bars and forge weapons")]
    [BossLock]
    public class BlacksmithModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly SmithingService _smithingService;
        private readonly SmithingShopRepository _shopRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;

        public BlacksmithModule(
            SmithingService smithingService,
            SmithingShopRepository shopRepository,
            PlayerRepository playerRepository,
            InventoryService inventoryService)
        {
            _smithingService = smithingService;
            _shopRepository = shopRepository;
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
        }

        // =========================
        // /blacksmith smelt
        // =========================
        [SlashCommand("smelt", "Smelt ore into bars")]
        public async Task Smelt(
            [Autocomplete(typeof(SmeltBarAutocompleteHandler))] string bar,
            [Autocomplete(typeof(SmithQuantityAutocompleteHandler))] string quantity = "1")
        {
            await DeferAsync(ephemeral: true);

            int qty;

            if (quantity.ToLower() == "max")
                qty = -1;
            else if (!int.TryParse(quantity, out qty) || qty <= 0)
            {
                await FollowupAsync("❌ Invalid quantity.", ephemeral: true);
                return;
            }

            var result = await _smithingService.SmeltAsync(Context.User.Id, bar.Trim().ToLower(), qty);
            await FollowupAsync(result, ephemeral: true);
        }

        [SlashCommand("craft", "Forge bars into weapons — auto-lists in your NPC shop")]
        public async Task Craft(
            [Autocomplete(typeof(SmithCraftAutocompleteHandler))] string item,
            [Autocomplete(typeof(SmithQuantityAutocompleteHandler))] string quantity = "1")
        {
            await DeferAsync(ephemeral: true);

            int qty;

            if (quantity.ToLower() == "max")
                qty = -1;
            else if (!int.TryParse(quantity, out qty) || qty <= 0)
            {
                await FollowupAsync("❌ Invalid quantity.", ephemeral: true);
                return;
            }

            var result = await _smithingService.CraftAsync(Context.User.Id, item.Trim().ToLower(), qty);
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // /blacksmith ore-bag
        // =========================
        [SlashCommand("ore-bag", "View all your ores and bars")]
        public async Task OreBag()
        {
            await DeferAsync(ephemeral: true);

            var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);
            var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            var oreIds = new[]
            {
        "bronze_ore", "iron_ore", "coal",
        "mithril_ore", "adamantite_ore", "runite_ore", "dragon_crystal"
    };

            var barIds = new[]
            {
        "bronze_bar", "iron_bar", "steel_bar",
        "mithril_bar", "adamant_bar", "runite_bar"
    };

            var oreSb = new StringBuilder();
            foreach (var id in oreIds)
            {
                invLookup.TryGetValue(id, out int qty);
                if (!InventoryItemDefinitions.All.TryGetValue(id, out var def)) continue;
                oreSb.AppendLine($"{def.Icon} **{def.Name}** — {qty:N0}");
            }

            var barSb = new StringBuilder();
            foreach (var id in barIds)
            {
                invLookup.TryGetValue(id, out int qty);
                if (!InventoryItemDefinitions.All.TryGetValue(id, out var def)) continue;
                barSb.AppendLine($"{def.Icon} **{def.Name}** — {qty:N0}");
            }

            var embed = new EmbedBuilder()
                .WithTitle("⛏️ Ore Bag")
                .WithColor(new Color(0xB87333))
                .AddField("🪨 Ores", oreSb.Length > 0 ? oreSb.ToString().Trim() : "*None*", true)
                .AddField("🔩 Bars", barSb.Length > 0 ? barSb.ToString().Trim() : "*None*", true)
                .WithFooter("Use /blacksmith smelt to turn ore into bars")
                .Build();

            await FollowupAsync(embed: embed, ephemeral: true);
        }

        // =========================
        // /blacksmith shop
        // =========================
        [SlashCommand("shop", "View your NPC shop listings and today's earnings")]
        public async Task Shop()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("❌ You need to start your adventure first.", ephemeral: true);
                return;
            }

            var listings = await _shopRepository.GetListingsAsync(Context.User.Id);

            var embed = new EmbedBuilder()
                .WithTitle("🛒 Your NPC Shop")
                .WithColor(new Color(0xB87333))
                .WithFooter("NPCs visit daily at 12:00 UTC and buy what's listed");

            if (listings.Count == 0)
            {
                embed.WithDescription("Your shop is empty.\nForge items with `/blacksmith craft` to stock it.");
            }
            else
            {
                var sb = new StringBuilder();

                foreach (var listing in listings.OrderBy(l => l.ItemId))
                {
                    if (!SmithingItemRegistry.All.TryGetValue(listing.ItemId, out var itemDef))
                        continue;

                    sb.AppendLine($"{itemDef.Icon} **{itemDef.Name}** × {listing.Quantity}");
                    sb.AppendLine($"   {itemDef.NpcGoldPrice}g each · NPC buys up to {itemDef.MaxNpcBuysPerDay}/day");
                }

                embed.WithDescription(sb.ToString().Trim());
            }

            // Daily earnings display
            var todayKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
            int earnedToday = player.SmithingLastReset == todayKey ? player.SmithingEarnedToday : 0;

            embed.AddField(
                "💰 Today's Earnings",
                $"**{earnedToday:N0}g** / 5,000g daily cap",
                inline: false);

            embed.AddField(
                "⚒️ Smithing Level",
                $"Level **{player.SmithingLevel}** — {player.SmithingXP:N0} XP",
                inline: false);

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        // =========================
        // /blacksmith recipes
        // =========================
        [SlashCommand("recipes", "View all smithing recipes available at your level")]
        public async Task Recipes()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("❌ You need to start your adventure first.", ephemeral: true);
                return;
            }

            var unlocked = SmithingItemRegistry.All.Values
                .Where(i => player.SmithingLevel >= i.RequiredSmithingLevel)
                .OrderBy(i => i.RequiredSmithingLevel)
                .ToList();

            var locked = SmithingItemRegistry.All.Values
                .Where(i => player.SmithingLevel < i.RequiredSmithingLevel)
                .OrderBy(i => i.RequiredSmithingLevel)
                .Take(3)
                .ToList();

            var embed = new EmbedBuilder()
                .WithTitle("⚒️ Blacksmith Recipes")
                .WithColor(new Color(0xB87333))
                .WithFooter($"Smithing Level {player.SmithingLevel} · Showing unlocked + next 3 locked");

            if (unlocked.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var item in unlocked)
                {
                    var mats = string.Join(" + ", item.BarRequirements
                        .Select(r => $"{r.Value}x {r.Key.Replace("_", " ")}"));
                    sb.AppendLine($"{item.Icon} **{item.Name}** — {mats} → {item.NpcGoldPrice}g · {item.SmithingXpReward} XP");
                }
                embed.AddField("✅ Unlocked", sb.ToString().Trim(), inline: false);
            }

            if (locked.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var item in locked)
                {
                    var mats = string.Join(" + ", item.BarRequirements
                        .Select(r => $"{r.Value}x {r.Key.Replace("_", " ")}"));
                    sb.AppendLine($"🔒 **{item.Name}** (Lv {item.RequiredSmithingLevel}) — {mats} → {item.NpcGoldPrice}g");
                }
                embed.AddField("🔒 Coming Up", sb.ToString().Trim(), inline: false);
            }

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}