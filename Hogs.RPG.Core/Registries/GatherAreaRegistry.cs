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
            }
        };
    }
}