using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;

namespace Hogs.RPG.Services.PetServices
{
    public class EvolvePetService
    {
        private readonly PetRepository _repo;
        private readonly PetService _petService;

        private const string AttackPetId = "armored_capybara";
        private const string DefensePetId = "el_tata_de_frog";
        private const string HealthPetId = "ice_wolf";
        private const string Tier3PetId = "primal_chimera";

        public EvolvePetService(PetRepository repo, PetService petService)
        {
            _repo = repo;
            _petService = petService;
        }

        // =========================
        // EVOLVE
        // =========================
        public async Task<(bool success, string message)> EvolveAsync(ulong userId)
        {
            var pets = await _repo.GetPetsAsync(userId);

            var attackPet = pets.FirstOrDefault(p => p.PetId == AttackPetId);
            var defensePet = pets.FirstOrDefault(p => p.PetId == DefensePetId);
            var healthPet = pets.FirstOrDefault(p => p.PetId == HealthPetId);

            if (attackPet == null || defensePet == null || healthPet == null)
            {
                var missing = new List<string>();
                if (attackPet == null) missing.Add($"⚔️ **Armored Capybara** (Attack pet — from Blazewing's Gorge)");
                if (defensePet == null) missing.Add($"🛡️ **El Tata de Frog** (Defense pet — from Stonehall Depths)");
                if (healthPet == null) missing.Add($"❤️ **Ice Wolf** (Health pet — from Drowned Archives)");

                return (false, $"❌ You are missing the following pets:\n{string.Join("\n", missing)}");
            }

            if (attackPet.IsEquipped || defensePet.IsEquipped || healthPet.IsEquipped)
                return (false, "❌ You cannot evolve a pet that is currently equipped. Unequip the pet(s) first.");

            var existing = await _repo.GetPetAsync(userId, Tier3PetId);
            if (existing != null)
                return (false, "❌ You already own the **Capytara**!");

            _repo.RemovePet(attackPet);
            _repo.RemovePet(defensePet);
            _repo.RemovePet(healthPet);

            await _petService.GivePetAsync(userId, Tier3PetId);

            if (!PetRegistry.All.TryGetValue(Tier3PetId, out var chimera))
                return (false, "❌ Evolution failed — Tier 3 pet not found in registry.");

            return (true,
                $"✨ **Evolution Complete!**\n\n" +
                $"The Armored Capybara, El Tata de Frog, and Ice Wolf merged into one!\n\n" +
                $"🐉 **Capytara** has been added to your pet bag!\n" +
                $"Use `/pet-equip Capytara` to equip it.");
        }

        // =========================
        // CHECK STATUS
        // =========================
        public async Task<string> GetEvolveStatusAsync(ulong userId)
        {
            var pets = await _repo.GetPetsAsync(userId);

            bool hasAttack = pets.Any(p => p.PetId == AttackPetId);
            bool hasDefense = pets.Any(p => p.PetId == DefensePetId);
            bool hasHealth = pets.Any(p => p.PetId == HealthPetId);
            bool hasChimera = pets.Any(p => p.PetId == Tier3PetId);

            if (hasChimera)
                return "🐉 You already own the **Capytara**!";

            string Check(bool has) => has ? "✅" : "❌";

            return $"**🧬 Evolution Progress — Capytara**\n\n" +
                   $"{Check(hasAttack)}  ⚔️ Armored Capybara  *(Blazewing's Gorge — Lv 15)*\n" +
                   $"{Check(hasDefense)} 🛡️ El Tata de Frog   *(Stonehall Depths — Lv 20)*\n" +
                   $"{Check(hasHealth)}  ❤️ Ice Wolf          *(Drowned Archives — Lv 25)*\n\n" +
                   (hasAttack && hasDefense && hasHealth
                       ? "✨ **All 3 collected! Confirm below to evolve!**"
                       : "Collect all 3 pets to unlock the evolution.");
        }
    }
}
