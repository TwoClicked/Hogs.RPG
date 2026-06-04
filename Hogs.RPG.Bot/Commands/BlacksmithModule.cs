using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
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

        public BlacksmithModule(
            SmithingService smithingService,
            SmithingShopRepository shopRepository,
            PlayerRepository playerRepository)
        {
            _smithingService = smithingService;
            _shopRepository = shopRepository;
            _playerRepository = playerRepository;
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

            if (!int.TryParse(quantity, out int qty) || qty <= 0)
            {
                await FollowupAsync("❌ Invalid quantity.", ephemeral: true);
                return;
            }

            var result = await _smithingService.SmeltAsync(Context.User.Id, bar.Trim().ToLower(), qty);
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // /blacksmith craft
        // =========================
        [SlashCommand("craft", "Forge bars into weapons — auto-lists in your NPC shop")]
        public async Task Craft(
            [Autocomplete(typeof(SmithCraftAutocompleteHandler))] string item,
            [Autocomplete(typeof(SmithQuantityAutocompleteHandler))] string quantity = "1")
        {
            await DeferAsync(ephemeral: true);

            if (!int.TryParse(quantity, out int qty) || qty <= 0)
            {
                await FollowupAsync("❌ Invalid quantity.", ephemeral: true);
                return;
            }

            var result = await _smithingService.CraftAsync(Context.User.Id, item.Trim().ToLower(), qty);
            await FollowupAsync(result, ephemeral: true);
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