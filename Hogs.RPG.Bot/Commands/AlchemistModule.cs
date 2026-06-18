using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AlchemyServices;
using Hogs.RPG.Services.InventoryServices;
using System.Text;

namespace Hogs.RPG.Bot.Commands
{
    [Group("alchemist", "Alchemist commands — brew and drink potions")]
    [BossLock]
    [GearSwapLock]
    [TradeLock]
    public class AlchemistModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AlchemyBrewService _brewService;
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;

        public AlchemistModule(
            AlchemyBrewService brewService,
            PlayerRepository playerRepository,
            InventoryService inventoryService)
        {
            _brewService = brewService;
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
        }

        // =========================
        // /alchemist brew
        // =========================
        [SlashCommand("brew", "Brew a potion using ingredients")]
        public async Task Brew(
            [Autocomplete(typeof(AlchemyBrewAutocompleteHandler))] string potion,
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

            var result = await _brewService.BrewAsync(Context.User.Id, potion.Trim().ToLower(), qty);
            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // /alchemist drink
        // =========================
        [SlashCommand("drink", "Drink a potion from your inventory")]
        public async Task Drink(
            [Autocomplete(typeof(AlchemyDrinkAutocompleteHandler))] string potion)
        {
            await DeferAsync(ephemeral: true);

            var result = await _brewService.DrinkAsync(Context.User.Id, potion.Trim().ToLower());
            await FollowupAsync(result, ephemeral: true);
        }


        // =========================
        // /alchemist recipes
        // =========================
        [SlashCommand("recipes", "View all potion recipes available at your Alchemist level")]
        public async Task Recipes()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("❌ You need to start your adventure first.", ephemeral: true);
                return;
            }

            var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);
            var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            var unlocked = AlchemyPotionRegistry.All.Values
                .Where(p => player.AlchemistLevel >= p.RequiredAlchemistLevel)
                .OrderBy(p => p.RequiredAlchemistLevel)
                .ToList();

            var locked = AlchemyPotionRegistry.All.Values
                .Where(p => player.AlchemistLevel < p.RequiredAlchemistLevel)
                .OrderBy(p => p.RequiredAlchemistLevel)
                .Take(3)
                .ToList();

            var embed = new EmbedBuilder()
                .WithTitle("🧪 Alchemist Recipes")
                .WithColor(new Color(0x9B59B6))
                .WithFooter($"Alchemist Level {player.AlchemistLevel} · Showing unlocked + next 3 locked");

            if (unlocked.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var p in unlocked)
                {
                    // Calculate how many can be brewed
                    int canBrew = int.MaxValue;
                    foreach (var (ingId, needed) in p.IngredientRequirements)
                    {
                        invLookup.TryGetValue(ingId, out int owned);
                        canBrew = Math.Min(canBrew, owned / needed);
                    }
                    if (canBrew == int.MaxValue) canBrew = 0;

                    var ings = string.Join(", ", p.IngredientRequirements
                        .Select(r =>
                        {
                            var name = InventoryItemDefinitions.All.TryGetValue(r.Key, out var def)
                                ? def.Name : r.Key;
                            return $"{r.Value}x {name}";
                        }));

                    string brewStatus = canBrew > 0 ? $"✅ can brew {canBrew}" : "❌ not enough ingredients";
                    sb.AppendLine($"{p.Icon} **{p.Name}** — {ings}");
                    sb.AppendLine($"  *{p.Description}* — {brewStatus}");
                }
                embed.AddField("✅ Unlocked", sb.ToString().Trim(), inline: false);
            }

            if (locked.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var p in locked)
                {
                    var ings = string.Join(", ", p.IngredientRequirements
                        .Select(r =>
                        {
                            var name = InventoryItemDefinitions.All.TryGetValue(r.Key, out var def)
                                ? def.Name : r.Key;
                            return $"{r.Value}x {name}";
                        }));
                    sb.AppendLine($"🔒 **{p.Name}** *(Lv {p.RequiredAlchemistLevel})* — {ings}");
                }
                embed.AddField("🔒 Coming Up", sb.ToString().Trim(), inline: false);
            }

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        // =========================
        // /alchemist potions
        // =========================
        [SlashCommand("potions", "View your potion inventory")]
        public async Task Potions()
        {
            await DeferAsync(ephemeral: true);

            var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);
            var potions = inventory
                .Where(i => AlchemyPotionRegistry.All.ContainsKey(i.ItemId) && i.Quantity > 0)
                .ToList();

            var embed = new EmbedBuilder()
                .WithTitle("🧪 Potion Inventory")
                .WithColor(new Color(0x9B59B6))
                .WithFooter("Use /alchemist drink to consume a potion");

            if (potions.Count == 0)
            {
                embed.WithDescription("You have no potions.\nBrew some with `/alchemist brew`.");
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var inv in potions.OrderBy(i => i.ItemId))
                {
                    if (!AlchemyPotionRegistry.All.TryGetValue(inv.ItemId, out var def)) continue;
                    sb.AppendLine($"{def.Icon} **{def.Name}** × {inv.Quantity}");
                    sb.AppendLine($"  *{def.Description}*");
                }
                embed.WithDescription(sb.ToString().Trim());
            }

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            int usedToday = player?.LastPotionDrankDate == today ? player.PotionsDrankToday : 0;

            embed.AddField("📊 Daily Usage", $"**{usedToday}/5** potions used today", inline: false);

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        // =========================
        // /alchemist buffs
        // =========================
        [SlashCommand("buffs", "View your currently active potion buffs")]
        public async Task Buffs()
        {
            await DeferAsync(ephemeral: true);

            var player = await _playerRepository.GetByDiscordIdAsync(Context.User.Id);
            if (player == null)
            {
                await FollowupAsync("❌ You need to start your adventure first.", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("✨ Active Potion Buffs")
                .WithColor(new Color(0x9B59B6));

            var now = DateTime.UtcNow;

            // Stat buff slot
            if (player.ActiveStatBuffId != null &&
                player.ActiveStatBuffExpiry.HasValue &&
                player.ActiveStatBuffExpiry.Value > now)
            {
                AlchemyPotionRegistry.All.TryGetValue(player.ActiveStatBuffId, out var statDef);
                var remaining = player.ActiveStatBuffExpiry.Value - now;
                embed.AddField(
                    "⚔️ Stat Buff",
                    $"{statDef?.Icon ?? "🧪"} **{statDef?.Name ?? player.ActiveStatBuffId}**\n" +
                    $"{statDef?.Description}\n" +
                    $"⏳ Expires in **{remaining.Hours}h {remaining.Minutes}m**",
                    inline: false);
            }
            else
            {
                embed.AddField("⚔️ Stat Buff", "*No active stat buff*", inline: false);
            }

            // Utility buff slot
            if (player.ActiveUtilityBuffId != null &&
                player.ActiveUtilityBuffExpiry.HasValue &&
                player.ActiveUtilityBuffExpiry.Value > now)
            {
                AlchemyPotionRegistry.All.TryGetValue(player.ActiveUtilityBuffId, out var utilDef);
                var remaining = player.ActiveUtilityBuffExpiry.Value - now;
                embed.AddField(
                    "🔮 Utility Buff",
                    $"{utilDef?.Icon ?? "🧪"} **{utilDef?.Name ?? player.ActiveUtilityBuffId}**\n" +
                    $"{utilDef?.Description}\n" +
                    $"⏳ Expires in **{remaining.Hours}h {remaining.Minutes}m**",
                    inline: false);
            }
            else
            {
                embed.AddField("🔮 Utility Buff", "*No active utility buff*", inline: false);
            }

            // Blacksmith's Elixir
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (player.BlacksmithElixirActiveDate == today)
            {
                embed.AddField(
                    "⚒️ Blacksmith's Elixir",
                    "Active — NPCs will buy max stock at tomorrow's 12 UTC reset.",
                    inline: false);
            }

            var today2 = DateTime.UtcNow.ToString("yyyy-MM-dd");
            int usedToday = player.LastPotionDrankDate == today2 ? player.PotionsDrankToday : 0;
            embed.WithFooter($"Potions used today: {usedToday}/5");

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}