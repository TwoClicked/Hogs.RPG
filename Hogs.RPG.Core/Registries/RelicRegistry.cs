using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Relics;

namespace Hogs.RPG.Core.GameData.Registries
{
    public static class RelicRegistry
    {
        public static readonly Dictionary<string, RelicDefinition> All = new()
        {
            // Tank
            { AllRelics.IronVow.Id, AllRelics.IronVow },
            { AllRelics.FortressHeart.Id, AllRelics.FortressHeart },
            { AllRelics.WardenResolve.Id, AllRelics.WardenResolve },
            { AllRelics.Unyielding.Id, AllRelics.Unyielding },

            // DPS
            { AllRelics.BerserkersBrand.Id, AllRelics.BerserkersBrand },
            { AllRelics.ExecutionersMark.Id, AllRelics.ExecutionersMark },
            { AllRelics.Bloodlust.Id, AllRelics.Bloodlust },
            { AllRelics.Relentless.Id, AllRelics.Relentless },

            // Healer
            { AllRelics.MendersGrace.Id, AllRelics.MendersGrace },
            { AllRelics.VialMastery.Id, AllRelics.VialMastery },
            { AllRelics.SpiritWard.Id, AllRelics.SpiritWard },
            { AllRelics.BoundlessCare.Id, AllRelics.BoundlessCare },

            // Universal
            { AllRelics.WanderersCoin.Id, AllRelics.WanderersCoin },
            { AllRelics.ScholarsTome.Id, AllRelics.ScholarsTome },
            { AllRelics.BeastBond.Id, AllRelics.BeastBond },
            { AllRelics.Plunderer.Id, AllRelics.Plunderer },
        };

        public static RelicDefinition Get(string id)
        {
            if (!All.TryGetValue(id, out var relic))
                throw new Exception($"Relic '{id}' not found in registry.");

            return relic;
        }
    }
}