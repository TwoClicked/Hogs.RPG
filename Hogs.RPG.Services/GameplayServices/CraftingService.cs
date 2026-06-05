using Hogs.RPG.Core.Entities.RecipeObjects;
using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Services.InventoryServices;

namespace Hogs.RPG.Services.GameplayServices;

public class CraftingService
{
    private readonly InventoryService _inventoryService;

    public CraftingService(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task<string> CraftAsync(ulong userId, string recipeId)
    {
        if (!RecipeRegistry.All.TryGetValue(recipeId, out var recipe))
            return "Unknown recipe.";

        var inventory = await _inventoryService.GetInventoryAsync(userId);

        foreach (var material in recipe.Materials)
        {
            var item = inventory.Find(i => i.ItemId == material.Key);

            if (item == null || item.Quantity < material.Value)
                return $"You lack {material.Value} {material.Key}.";
        }

        foreach (var material in recipe.Materials)
        {
            await _inventoryService.TakeItemAsync(userId, material.Key, material.Value);
        }

        await _inventoryService.GiveItemAsync(userId, recipe.ResultItem, recipe.ResultAmount);

        return $"⚒ You crafted **{recipe.Name}**!";
    }

    public List<Recipe> GetAllRecipes()
    {
        return RecipeRegistry.All.Values.ToList();
    }
}