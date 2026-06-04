using System.Collections.Generic;

namespace Hogs.RPG.GameData.Gathering
{
    public static class GatherAreaRegistry
    {
        public static readonly Dictionary<string, GatherArea> All = new()
        {
            ["forest"] = new GatherArea
            {
                Id = "forest",
                Name = "Forest",
                Drops = new Dictionary<string, double>
                {
                    ["herb"] = 0.5,
                    ["red_mushroom"] = 0.3,
                    ["crystal_leaf"] = 0.2
                }

            },

            ["mine"] = new GatherArea
            {
                Id = "mine",
                Name = "Mine",
                Drops = new Dictionary<string, double>
                {
                    ["bronze_ore"] = 0.40,
                    ["iron_ore"] = 0.28,
                    ["coal"] = 0.20,
                    ["mithril_ore"] = 0.07,
                    ["adamantite_ore"] = 0.03,
                    ["runite_ore"] = 0.02
                    // dragon_crystal is NOT in the drop table here
                    // it is rolled separately in SmithingService at lv99 only
                }
            }
        };
    }
}