using Discord;
using Discord.Interactions;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.GameData.Equipment;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Services.AlchemyServices;
using Hogs.RPG.Services.InventoryServices;
using System.Text;


[Group("alchemy", "Alchemy commands")]
[BossLock]
[GearSwapLock]
[TradeLock]
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
        [Autocomplete(typeof(PotionAutocompleteHandler))] string potion,
        [Autocomplete(typeof(CraftAmountAutocompleteHandler))] string amount = "1")
    {
        await DeferAsync(ephemeral: true); 

        int craftAmount;

        if (amount.ToLower() == "max")
        {
            craftAmount = -1;
        }
        else if (!int.TryParse(amount, out craftAmount))
        {
            await FollowupAsync("Invalid amount.", ephemeral: true);
            return;
        }

        var result = await _alchemyService.CraftPotionAsync(
            Context.User.Id,
            potion,
            craftAmount
        );

        await FollowupAsync(result, ephemeral: true);
    }

    // =========================
    // VIEW RECIPES
    // =========================
    [SlashCommand("recipes", "View potion recipes")]
    public async Task AlchemyRecipes()
    {
        await DeferAsync(ephemeral: true);

        var recipes = RecipeRegistry.All.Values;

        var potionRecipes = recipes
            .Where(r => r.ResultItem == "xp_potion" || r.ResultItem == "health_potion")
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
            if (!InventoryItemDefinitions.All.TryGetValue(recipe.ResultItem, out var item))
                continue;

            int maxCraftable = int.MaxValue;

            foreach (var mat in recipe.Materials)
            {
                var invItem = inventory.Find(i => i.ItemId == mat.Key);

                if (invItem == null || invItem.Quantity == 0)
                {
                    maxCraftable = 0;
                    break;
                }

                int possible = invItem.Quantity / mat.Value;

                if (possible < maxCraftable)
                    maxCraftable = possible;
            }

            bool canCraft = maxCraftable > 0;
            var icon = canCraft ? "🧪✅" : "🧪❌";

            builder.AppendLine($"{icon} **{item.Name}** (Max: {maxCraftable})");

            foreach (var mat in recipe.Materials)
            {
                var invItem = inventory.Find(i => i.ItemId == mat.Key);

                if (InventoryItemDefinitions.All.TryGetValue(mat.Key, out var matItem))
                {
                    int owned = invItem?.Quantity ?? 0;
                    builder.AppendLine($"  {matItem.Name} {owned}/{mat.Value}");
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