using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.AlchemyServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Core.GameData.Equipment;
using System.Text;

[Group("alchemy", "Alchemy commands")]
public class AlchemyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly AlchemyService _alchemyService;
    private readonly InventoryService _inventoryService;

    public AlchemyModule(
        AlchemyService alchemyService,
        InventoryService inventoryService)
    {
        _alchemyService = alchemyService;
        _inventoryService = inventoryService;
    }

    // =========================
    // CRAFT POTION
    // =========================
    [SlashCommand("craft", "Craft potions")]
    public async Task CraftPotion(
        [Autocomplete(typeof(PotionAutocompleteHandler))]
        string potion,
        int amount = 1)
    {

        await DeferAsync();

        var result = await _alchemyService.CraftPotionAsync(
            Context.User.Id,
            potion,
            amount
        );

        await FollowupAsync(result);
    }

    // =========================
    // VIEW RECIPES
    // =========================
    [SlashCommand("recipes", "View potion recipes")]
    public async Task AlchemyRecipes()
    {
        await DeferAsync(ephemeral: true);

        var recipes = RecipeRegistry.All.Values;

        // Only non-equipment = potions/alchemy
        var potionRecipes = recipes
            .Where(r => !EquipmentRegistry.All.ContainsKey(r.ResultItem))
            .ToList();

        var inventory = await _inventoryService.GetInventoryAsync(Context.User.Id);

        if (potionRecipes.Count == 0)
        {
            await FollowupAsync("No potion recipes found.", ephemeral: true);
            return;
        }

        var builder = new StringBuilder();

        foreach (var recipe in potionRecipes)
        {
            // Get result item safely
            if (!InventoryItemDefinitions.All.TryGetValue(recipe.ResultItem, out var item))
                continue;

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

            var icon = canCraft ? "🧪✅" : "🧪❌";

            builder.AppendLine($"{icon} **{item.Name}**");

            foreach (var mat in recipe.Materials)
            {
                if (InventoryItemDefinitions.All.TryGetValue(mat.Key, out var matItem))
                {
                    builder.AppendLine($"  {matItem.Name} x{mat.Value}");
                }
                else
                {
                    builder.AppendLine($"  {mat.Key} x{mat.Value}");
                }
            }

            builder.AppendLine();
        }

        var embed = new EmbedBuilder()
            .WithTitle("🧪 Alchemy Recipes")
            .WithDescription(builder.ToString())
            .WithColor(Color.Purple)
            .Build();

        await FollowupAsync(embed: embed, ephemeral: true);
    }
}