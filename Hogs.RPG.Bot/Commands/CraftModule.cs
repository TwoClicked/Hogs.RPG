using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.Equipment;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    [BossLock]
    [GearSwapLock]
    public class CraftModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CraftingService _craftingService;
        private readonly InventoryService _inventoryService;

        private readonly Dictionary<string, string> _slotIcons = new()
        {
            { "MainHand", "🗡" },
            { "OffHand", "🏹" },
            { "Helmet", "🪖" },
            { "Body", "🛡" },
            { "Legs", "👖" },
            { "Gloves", "🧤" },
            { "Boots", "🥾" },
            { "Ring", "💍" },
            { "Amulet", "📿" }
        };

        public CraftModule(CraftingService craftingService, InventoryService inventoryService)
        {
            _craftingService = craftingService;
            _inventoryService = inventoryService;
        }

        // =========================
        // HELPERS
        // =========================
        private static string GetStars(int? tier) =>
            tier.HasValue ? string.Concat(Enumerable.Repeat("⭐", tier.Value)) : "";

        private static int? GetItemTier(string itemId) =>
            InventoryItemDefinitions.All.TryGetValue(itemId, out var def) ? def.Tier : null;

        // =========================
        // CRAFT
        // =========================
        [SlashCommand("craft", "Craft an item")]
        public async Task Craft(
            [Autocomplete(typeof(CraftAutocompleteHandler))]
            string recipe)
        {
            await DeferAsync(ephemeral: true);

            var result = await _craftingService.CraftAsync(Context.User.Id, recipe);

            await FollowupAsync(result, ephemeral: true);
        }

        // =========================
        // RECIPES
        // =========================
        [SlashCommand("recipes", "View crafting recipes")]
        public async Task Recipes(
            [Autocomplete(typeof(RecipeSlotAutocompleteHandler))]
             string slot = null)
        {
            await DeferAsync(ephemeral: true);

            var recipes = _craftingService.GetAllRecipes();
            var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);

            // Fast lookup
            var invLookup = inventory.ToDictionary(i => i.ItemId, i => i.Quantity);

            // =========================
            // CATEGORY VIEW
            // =========================
            if (string.IsNullOrWhiteSpace(slot))
            {
                var grouped = recipes
                    .Where(r => EquipmentRegistry.All.ContainsKey(r.ResultItem))
                    .GroupBy(r =>
                    {
                        var item = EquipmentRegistry.All[r.ResultItem];
                        return item.Slot.ToString();
                    });

                var embed = new EmbedBuilder()
                    .WithTitle("⚒ Crafting Categories")
                    .WithColor(Color.DarkOrange)
                    .WithDescription("Use `/recipes <slot>` to view recipes.");

                foreach (var group in grouped)
                {
                    var icon = _slotIcons.ContainsKey(group.Key) ? _slotIcons[group.Key] : "⚒";
                    embed.AddField($"{icon} {group.Key}", $"{group.Count()} recipes", true);
                }

                await FollowupAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            // =========================
            // SLOT VIEW
            // =========================
            var slotKey = slot.Trim().ToLower();

            var slotRecipes = recipes
                .Where(r =>
                    EquipmentRegistry.All.ContainsKey(r.ResultItem) &&
                    EquipmentRegistry.All[r.ResultItem].Slot.ToString().ToLower() == slotKey
                )
                .ToList();

            if (slotRecipes.Count == 0)
            {
                await FollowupAsync("No recipes found for that slot.", ephemeral: true);
                return;
            }

            var builder = new StringBuilder();

            foreach (var recipe in slotRecipes)
            {
                // =========================
                // CAN CRAFT CHECK
                // =========================
                bool canCraft = true;

                foreach (var mat in recipe.Materials)
                {
                    if (!invLookup.TryGetValue(mat.Key, out var qty) || qty < mat.Value)
                    {
                        canCraft = false;
                        break;
                    }
                }

                var craftIcon = canCraft ? "✅" : "❌";
                var item = EquipmentRegistry.All[recipe.ResultItem];

                // =========================
                // ITEM TIER (from highest tier material)
                // =========================
                int? recipeTier = recipe.Materials.Keys
                    .Select(GetItemTier)
                    .Where(t => t.HasValue)
                    .Select(t => t!.Value)
                    .DefaultIfEmpty(0)
                    .Max() is int t and > 0 ? t : (int?)null;

                // =========================
                // HEADER: icon + name + stars
                // =========================
                builder.AppendLine($"{craftIcon} **{recipe.Name}** {GetStars(recipeTier)}");

                // =========================
                // STATS
                // =========================
                if (item.Attack > 0)
                    builder.AppendLine($"⚔ Attack +{item.Attack}");

                if (item.Defense > 0)
                    builder.AppendLine($"🛡 Defense +{item.Defense}");

                if (item.Health > 0)
                    builder.AppendLine($"❤️ Health +{item.Health}");

                // =========================
                // MATERIALS GROUPED BY TIER
                // =========================
                var tierGroups = recipe.Materials
                    .GroupBy(mat => GetItemTier(mat.Key) ?? 0)
                    .OrderBy(g => g.Key);

                foreach (var tierGroup in tierGroups)
                {
                    if (tierGroup.Key > 0)
                        builder.AppendLine($"**Tier {tierGroup.Key}**");

                    foreach (var mat in tierGroup)
                    {
                        var matDef = InventoryItemDefinitions.All.TryGetValue(mat.Key, out var d) ? d : null;
                        var name = matDef?.Name ?? mat.Key;
                        var matIcon = matDef?.SubCategory == "Rare" ? "★" : "✦";

                        builder.AppendLine($"{matIcon} {name} x{mat.Value}");
                    }
                }

                builder.AppendLine();
            }

            var firstItem = EquipmentRegistry.All[slotRecipes.First().ResultItem];
            var slotName = firstItem.Slot.ToString();
            var slotIcon = _slotIcons.ContainsKey(slotName) ? _slotIcons[slotName] : "⚒";

            var resultEmbed = new EmbedBuilder()
                .WithTitle($"{slotIcon} {slotName} Recipes")
                .WithDescription(builder.ToString())
                .WithColor(Color.DarkOrange)
                .Build();

            await FollowupAsync(embed: resultEmbed, ephemeral: true);
        }
    }
}