using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Raids;

namespace Hogs.RPG.Core.GameData.Registries
{
    public static class RaidRegistry
    {
        public static readonly Dictionary<string, RaidDefinition> All = new()
        {
            { AllRaids.Lair.Id, AllRaids.Lair },
            { AllRaids.Stronghold.Id, AllRaids.Stronghold },
            { AllRaids.Fortress.Id, AllRaids.Fortress },
            { AllRaids.Citadel.Id, AllRaids.Citadel },
            { AllRaids.WorldBoss.Id, AllRaids.WorldBoss },
        };

        public static RaidDefinition Get(string id)
        {
            if (!All.TryGetValue(id, out var raid))
                throw new Exception($"Raid '{id}' not found in registry.");

            return raid;
        }

        public static RaidDefinition? GetByTier(int tier)
        {
            return All.Values.FirstOrDefault(r => r.Tier == tier);
        }
    }
}