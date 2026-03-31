using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Recipes
{
    public static class AlchemyRecipes
    {
        public static readonly Recipe XpPotion = new()
        {
            Id = "xp_potion",
            Name = "XP Potion",
            ResultItem = "xp_potion",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "herb", 5 },
                { "crystal_leaf", 2 }
            }
        };

        public static readonly Recipe HealthPotion = new()
        {
            Id = "health_potion",
            Name = "Health Potion",
            ResultItem = "health_potion",
            ResultAmount = 1,
            Materials = new Dictionary<string, int>
            {
                { "red_mushroom", 1 },
                { "monster_blood", 3 }
            }
        };
    }
}