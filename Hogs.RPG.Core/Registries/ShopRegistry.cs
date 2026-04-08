using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Shop;
using System.Collections.Generic;
using System.Linq;

namespace Hogs.RPG.Core.Registries
{
    public static class ShopRegistry
    {
        public static readonly Dictionary<string, ShopItemDefinition> All = new()
        {
            // ===== Viking Rise Resources =====
            { VikingRiseShopItems.Food.Id,       VikingRiseShopItems.Food       },
            { VikingRiseShopItems.Wood.Id,       VikingRiseShopItems.Wood       },
            { VikingRiseShopItems.Stone.Id,      VikingRiseShopItems.Stone      },
            { VikingRiseShopItems.Gold.Id,       VikingRiseShopItems.Gold       },
            { VikingRiseShopItems.SkipBank.Id,   VikingRiseShopItems.SkipBank   },
            { VikingRiseShopItems.ClearFines.Id, VikingRiseShopItems.ClearFines },
            { VikingRiseShopItems.SkipVd.Id,     VikingRiseShopItems.SkipVd     },

            // ===== Viking Rise Rank Spots =====
            { VikingRiseShopItems.Rank11To20.Id, VikingRiseShopItems.Rank11To20 },
            { VikingRiseShopItems.Rank6To10.Id,  VikingRiseShopItems.Rank6To10  },
            { VikingRiseShopItems.Rank4To5.Id,   VikingRiseShopItems.Rank4To5   },

            // ===== Discord Rewards =====
            { DiscordShopItems.CustomTitle.Id, DiscordShopItems.CustomTitle },
            { DiscordShopItems.NameColor.Id,   DiscordShopItems.NameColor   },
            { DiscordShopItems.VipLounge.Id,   DiscordShopItems.VipLounge   },
            { DiscordShopItems.Shoutout.Id,    DiscordShopItems.Shoutout    },
            { DiscordShopItems.PetRename.Id,   DiscordShopItems.PetRename   },

            // ===== RPG Perks =====
            { RpgPerkShopItems.DoubleXp.Id,      RpgPerkShopItems.DoubleXp      },
            { RpgPerkShopItems.StaminaBoost.Id,  RpgPerkShopItems.StaminaBoost  },
            { RpgPerkShopItems.LootCrate.Id,     RpgPerkShopItems.LootCrate     },
            { RpgPerkShopItems.StaminaReset.Id,  RpgPerkShopItems.StaminaReset  },
        };

        // Helper to get all items in a given category
        public static List<ShopItemDefinition> GetByCategory(ShopCategory category) =>
            All.Values.Where(i => i.Category == category).ToList();
    }
}