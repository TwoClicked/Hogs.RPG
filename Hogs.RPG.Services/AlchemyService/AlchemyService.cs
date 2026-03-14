using Hogs.RPG.Core.GameData.Recipes;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.InventoryServices;

namespace Hogs.RPG.Services.AlchemyServices
{
    public class AlchemyService
    {
        private readonly InventoryService _inventoryService;

        public AlchemyService(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<string> CraftPotionAsync(ulong userId, string recipeId, int amount)
        {
            if (!RecipeRegistry.All.TryGetValue(recipeId, out var recipe))
                return "Unknown recipe.";

            foreach (var material in recipe.Materials)
            {
                var playerAmount = await _inventoryService.GetItemAmountAsync(userId, material.Key);

                if (playerAmount < material.Value * amount)
                    return $"You need {material.Value * amount}x {material.Key}.";
            }

            foreach (var material in recipe.Materials)
            {
                await _inventoryService.TakeItemAsync(
                    userId,
                    material.Key,
                    material.Value * amount
                );
            }

            await _inventoryService.GiveItemAsync(
                userId,
                recipe.ResultItem,
                recipe.ResultAmount * amount
            );

            return $"🧪 Crafted **{recipe.ResultAmount * amount}x {recipe.Name}**!";
        }
    }
}