using Hogs.RPG.Core.Entities.JobObjects;

namespace Hogs.RPG.Core.GameData.Smithing
{
    public static class SmeltingRecipes
    {
        public static readonly SmeltingRecipeDefinition BronzeBar = new()
        {
            BarId = "bronze_bar",
            BarName = "Bronze Bar",
            RequiredSmithingLevel = 1,
            OreRequirements = new() { ["bronze_ore"] = 2 }
        };

        public static readonly SmeltingRecipeDefinition IronBar = new()
        {
            BarId = "iron_bar",
            BarName = "Iron Bar",
            RequiredSmithingLevel = 15,
            OreRequirements = new() { ["iron_ore"] = 2 }
        };

        public static readonly SmeltingRecipeDefinition SteelBar = new()
        {
            BarId = "steel_bar",
            BarName = "Steel Bar",
            RequiredSmithingLevel = 30,
            OreRequirements = new() { ["iron_ore"] = 2, ["coal"] = 1 }
        };

        public static readonly SmeltingRecipeDefinition MithrilBar = new()
        {
            BarId = "mithril_bar",
            BarName = "Mithril Bar",
            RequiredSmithingLevel = 50,
            OreRequirements = new() { ["mithril_ore"] = 3, ["coal"] = 1 }
        };

        public static readonly SmeltingRecipeDefinition AdamantBar = new()
        {
            BarId = "adamant_bar",
            BarName = "Adamant Bar",
            RequiredSmithingLevel = 70,
            OreRequirements = new() { ["adamantite_ore"] = 4, ["coal"] = 2 }
        };

        public static readonly SmeltingRecipeDefinition RuniteBar = new()
        {
            BarId = "runite_bar",
            BarName = "Runite Bar",
            RequiredSmithingLevel = 85,
            OreRequirements = new() { ["runite_ore"] = 4, ["coal"] = 2 }
        };
    }
}