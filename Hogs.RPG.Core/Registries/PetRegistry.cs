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
            { Tier2Pets.ArmoredCapybara.Id,  Tier2Pets.ArmoredCapybara  },  // ⚔️ Attack pet
            { Tier2Pets.ElTataDeFrog.Id,    Tier2Pets.ElTataDeFrog    },  // 🛡️ Defense pet
            { Tier2Pets.IceWolf.Id , Tier2Pets.IceWolf  },  // ❤️ Health pet

            // ===== Tier 3 — Evolved =====
            { Tier3Pet.MythicalArmoredIceTurtle.Id, Tier3Pet.MythicalArmoredIceTurtle },
        };

        public static PetDefinition Get(string id)
        {
            if (!All.TryGetValue(id, out var pet))
                throw new Exception($"Pet '{id}' not found in registry.");

            return pet;
        }
    }
}
