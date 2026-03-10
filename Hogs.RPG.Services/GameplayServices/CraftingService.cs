using Hogs.RPG.Core.Entities;
using Hogs.RPG.Services.InventoryServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.GameplayServices
{
    public class CraftingService
    {
        private readonly InventoryService _inventoryService;

        private readonly Dictionary<string, Recipe> _recipes = new()
{
    {
        "bone_dagger",
        new Recipe
        {
            Id = "bone_dagger",
            Name = "Bone_Dagger",
            Slot = "Main Hand",
            ResultItem = "bone_dagger",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "bone", 2 },
                { "leather", 1 }
            }
        }
    },
    {
        "hunter_bow",
        new Recipe
        {
            Id = "hunter_bow",
            Name = "Hunter_Bow",
            Slot = "Off Hand",
            ResultItem = "hunter_bow",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "bone", 2 },
                { "feather", 2 }
            }
        }
    },
    {
        "leather_armor",
        new Recipe
        {
            Id = "leather_armor",
            Name = "Leather_Armor",
            Slot = "Body",
            ResultItem = "leather_armor",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "leather", 3 },
                { "fur", 2 }
            }
        }
    }
};

        public CraftingService(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<string> CraftAsync(ulong userId, string recipeId)
        {
            if (!_recipes.ContainsKey(recipeId))
                return "Unknown recipe.";

            var recipe = _recipes[recipeId];

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
            return _recipes.Values.ToList();
        }

    }
}