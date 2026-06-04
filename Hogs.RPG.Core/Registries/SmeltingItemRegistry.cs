using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Smithing;

namespace Hogs.RPG.Core.Registries
{
    public static class SmithingItemRegistry
    {
        public static readonly Dictionary<string, SmithingItemDefinition> All = new()
        {
            { SmithingItems.BronzeSword.Id,  SmithingItems.BronzeSword  },
            { SmithingItems.BronzeAxe.Id,    SmithingItems.BronzeAxe    },
            { SmithingItems.IronSword.Id,    SmithingItems.IronSword    },
            { SmithingItems.IronDagger.Id,   SmithingItems.IronDagger   },
            { SmithingItems.SteelSword.Id,   SmithingItems.SteelSword   },
            { SmithingItems.SteelMace.Id,    SmithingItems.SteelMace    },
            { SmithingItems.MithrilSword.Id, SmithingItems.MithrilSword },
            { SmithingItems.MithrilShield.Id,SmithingItems.MithrilShield},
            { SmithingItems.AdamantSword.Id, SmithingItems.AdamantSword },
            { SmithingItems.AdamantHelm.Id,  SmithingItems.AdamantHelm  },
            { SmithingItems.RuneSword.Id,    SmithingItems.RuneSword    },
            { SmithingItems.RuneShield.Id,   SmithingItems.RuneShield   },
            { SmithingItems.DragonBlade.Id,  SmithingItems.DragonBlade  },
        };
    }
}