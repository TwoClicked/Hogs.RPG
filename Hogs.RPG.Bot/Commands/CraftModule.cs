using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.GameplayServices;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    public class CraftModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CraftingService _craftingService;


        private readonly Dictionary<string, string> _slotIcons = new()
        {
            { "Main Hand", "🗡" },
            { "Off Hand", "🏹" },
            { "Helmet", "🪖" },
            { "Body", "🛡" },
            { "Legs", "👖" },
            { "Gloves", "🧤" },
            { "Boots", "🥾" },
            { "Ring", "💍" },
            { "Amulet", "📿" }
        };
        public CraftModule(CraftingService craftingService)
        {
            _craftingService = craftingService;
        }

        [SlashCommand("craft", "Craft an item")]
        public async Task Craft(string recipe)
        {
            await DeferAsync(ephemeral: true);

            var result = await _craftingService.CraftAsync(Context.User.Id, recipe);

            await FollowupAsync(result, ephemeral: true);
        }


        [SlashCommand("recipes", "View crafting recipes")]
        public async Task Recipes(string slot = null)
        {
            await DeferAsync(ephemeral: true);

            var recipes = _craftingService.GetAllRecipes();

            // Show categories
            if (string.IsNullOrWhiteSpace(slot))
            {
                var grouped = recipes.GroupBy(r => r.Slot);

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

            // Normalize slot input
            var slotKey = slot.Trim().ToLower();

            var slotRecipes = recipes
                .Where(r => r.Slot.ToLower().Replace(" ", "") == slotKey.Replace(" ", ""))
                .ToList();

            if (slotRecipes.Count == 0)
            {
                await FollowupAsync("No recipes found for that slot.", ephemeral: true);
                return;
            }

            var builder = new StringBuilder();

            foreach (var recipe in slotRecipes)
            {
                builder.AppendLine($"**{recipe.Name}**");

                foreach (var mat in recipe.Materials)
                {
                    builder.AppendLine($"  {mat.Key} x{mat.Value}");
                }

                builder.AppendLine();
            }

            var slotName = slotRecipes.First().Slot;
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