using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Pets;

namespace Hogs.RPG.Core.GameData.Registries
{
    public static class PetRegistry
    {
        public static readonly Dictionary<string, PetDefinition> All = new()
        {
            // ===== Tier 1 =====
            { Tier1Pets.VerdantWisp.Id, Tier1Pets.VerdantWisp },

            // ===== Tier 2 — Pet Dungeon Drops =====
            { Tier2Pets.BlazefangRaptor.Id,  Tier2Pets.BlazefangRaptor  },  // ⚔️ Attack pet
            { Tier2Pets.IronshellBoar.Id,    Tier2Pets.IronshellBoar    },  // 🛡️ Defense pet
            { Tier2Pets.TidecallSerpent.Id,  Tier2Pets.TidecallSerpent  },  // ❤️ Health pet

            // ===== Tier 3 — Evolved =====
            { Tier3Pet.PrimalChimera.Id, Tier3Pet.PrimalChimera },
        };

        public static PetDefinition Get(string id)
        {
            if (!All.TryGetValue(id, out var pet))
                throw new Exception($"Pet '{id}' not found in registry.");

            return pet;
        }
    }
}
