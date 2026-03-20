using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Equipment;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
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

        [SlashCommand("craft", "Craft an item")]
        public async Task Craft(
            [Autocomplete(typeof(CraftAutocompleteHandler))]
            string recipe)
        {
            var result = await _craftingService.CraftAsync(Context.User.Id, recipe);
            await RespondAsync(result);
        }

        [SlashCommand("recipes", "View crafting recipes")]
        public async Task Recipes(string slot = null)
        {
            await DeferAsync(ephemeral: true);

            var recipes = _craftingService.GetAllRecipes();
            var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);

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
                bool canCraft = true;

                foreach (var mat in recipe.Materials)
                {
                    var invItem = inventory.Find(i => i.ItemId == mat.Key);

                    if (invItem == null || invItem.Quantity < mat.Value)
                    {
                        canCraft = false;
                        break;
                    }
                }

                var icon = canCraft ? "✅" : "❌";

                builder.AppendLine($"{icon} **{recipe.Name}**");

                foreach (var mat in recipe.Materials)
                {
                    builder.AppendLine($"  {mat.Key} x{mat.Value}");
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