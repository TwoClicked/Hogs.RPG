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

            // =========================
            // CALCULATE MAX IF NEEDED
            // =========================
            if (amount == -1)
            {
                int maxCraftable = int.MaxValue;

                foreach (var material in recipe.Materials)
                {
                    var playerAmount = await _inventoryService.GetItemAmountAsync(userId, material.Key);

                    if (playerAmount == 0)
                    {
                        maxCraftable = 0;
                        break;
                    }

                    int possible = playerAmount / material.Value;

                    if (possible < maxCraftable)
                        maxCraftable = possible;
                }

                amount = maxCraftable;
            }

            // =========================
            // VALIDATION
            // =========================
            if (amount <= 0)
                return "You don't have enough materials to craft this.";

            foreach (var material in recipe.Materials)
            {
                var playerAmount = await _inventoryService.GetItemAmountAsync(userId, material.Key);

                if (playerAmount < material.Value * amount)
                    return $"You need {material.Value * amount}x {material.Key}.";
            }

            // =========================
            // REMOVE MATERIALS
            // =========================
            foreach (var material in recipe.Materials)
            {
                await _inventoryService.TakeItemAsync(
                    userId,
                    material.Key,
                    material.Value * amount
                );
            }

            // =========================
            // GIVE RESULT
            // =========================
            await _inventoryService.GiveItemAsync(
                userId,
                recipe.ResultItem,
                recipe.ResultAmount * amount
            );

            return $"🧪 Crafted **{recipe.ResultAmount * amount}x {recipe.Name}**!";
        }
    }
}