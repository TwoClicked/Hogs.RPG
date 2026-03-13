using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Recipes;

public static class RecipeRegistry
{
    public static readonly Dictionary<string, Recipe> All = new()
    {
        { Tier1HunterRecipes.ClawDagger.Id, Tier1HunterRecipes.ClawDagger },
        { Tier1HunterRecipes.BoneBuckler.Id, Tier1HunterRecipes.BoneBuckler },
        { Tier1HunterRecipes.LeatherVest.Id, Tier1HunterRecipes.LeatherVest },
        { Tier1HunterRecipes.BoneHelm.Id, Tier1HunterRecipes.BoneHelm },
        { Tier1HunterRecipes.HunterLeggings.Id, Tier1HunterRecipes.HunterLeggings },
        { Tier1HunterRecipes.HideBoots.Id, Tier1HunterRecipes.HideBoots },
        { Tier1HunterRecipes.FurGloves.Id, Tier1HunterRecipes.FurGloves },
        { Tier1HunterRecipes.FeatherBand.Id, Tier1HunterRecipes.FeatherBand },
        { Tier1HunterRecipes.RavenCharm.Id, Tier1HunterRecipes.RavenCharm }
    };
}