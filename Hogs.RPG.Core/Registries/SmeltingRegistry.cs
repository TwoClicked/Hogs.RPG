using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Smithing;

namespace Hogs.RPG.Core.Registries
{
    public static class SmeltingRegistry
    {
        public static readonly Dictionary<string, SmeltingRecipeDefinition> All = new()
        {
            { SmeltingRecipes.BronzeBar.BarId,  SmeltingRecipes.BronzeBar  },
            { SmeltingRecipes.IronBar.BarId,    SmeltingRecipes.IronBar    },
            { SmeltingRecipes.SteelBar.BarId,   SmeltingRecipes.SteelBar   },
            { SmeltingRecipes.MithrilBar.BarId, SmeltingRecipes.MithrilBar },
            { SmeltingRecipes.AdamantBar.BarId, SmeltingRecipes.AdamantBar },
            { SmeltingRecipes.RuniteBar.BarId,  SmeltingRecipes.RuniteBar  },
        };
    }
}