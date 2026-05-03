using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;

namespace Hogs.RPG.Services.PetServices
{
    public class EvolvePetService
    {
        private readonly PetRepository _repo;
        private readonly PetService _petService;

        // The 3 pets required for the Tier 3 evolution
        private const string AttackPetId = "blazefang_raptor";
        private const string DefensePetId = "ironshell_boar";
        private const string HealthPetId = "tidecall_serpent";
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

            // Check all 3 are owned
            if (attackPet == null || defensePet == null || healthPet == null)
            {
                var missing = new List<string>();
                if (attackPet == null) missing.Add($"⚔️ **Blazefang Raptor** (Attack pet — from Blazewing's Gorge)");
                if (defensePet == null) missing.Add($"🛡️ **Ironshell Boar** (Defense pet — from Stonehall Depths)");
                if (healthPet == null) missing.Add($"❤️ **Tidecall Serpent** (Health pet — from Drowned Archives)");

                return (false, $"❌ You are missing the following pets:\n{string.Join("\n", missing)}");
            }

            // Check none of them are currently equipped
            if (attackPet.IsEquipped || defensePet.IsEquipped || healthPet.IsEquipped)
            {
                return (false, "❌ You cannot evolve a pet that is currently equipped. Unequip the pet(s) first.");
            }

            // Check Tier 3 not already owned
            var existing = await _repo.GetPetAsync(userId, Tier3PetId);
            if (existing != null)
            {
                return (false, "❌ You already own the **Primal Chimera**!");
            }

            // Remove all 3 pets from DB
            _repo.RemovePet(attackPet);
            _repo.RemovePet(defensePet);
            _repo.RemovePet(healthPet);

            // Grant the Tier 3 pet
            await _petService.GivePetAsync(userId, Tier3PetId);

            if (!PetRegistry.All.TryGetValue(Tier3PetId, out var chimera))
                return (false, "❌ Evolution failed — Tier 3 pet not found in registry.");

            return (true,
                $"✨ **Evolution Complete!**\n\n" +
                $"The Blazefang Raptor, Ironshell Boar, and Tidecall Serpent merged into one!\n\n" +
                $"🐉 **Primal Chimera** has been added to your pet bag!\n" +
                $"Use `/pet-equip primal_chimera` to equip it.");
        }

        // =========================
        // CHECK STATUS (for /pet-evolve preview)
        // =========================
        public async Task<string> GetEvolveStatusAsync(ulong userId)
        {
            var pets = await _repo.GetPetsAsync(userId);

            bool hasAttack = pets.Any(p => p.PetId == AttackPetId);
            bool hasDefense = pets.Any(p => p.PetId == DefensePetId);
            bool hasHealth = pets.Any(p => p.PetId == HealthPetId);
            bool hasChimera = pets.Any(p => p.PetId == Tier3PetId);

            if (hasChimera)
                return "🐉 You already own the **Primal Chimera**!";

            string Check(bool has) => has ? "✅" : "❌";

            return $"**🧬 Evolution Progress — Primal Chimera**\n\n" +
                    $"{Check(hasAttack)}  ⚔️ Blazefang Raptor  *(Blazewing's Gorge — Lv 15)*\n" +
                    $"{Check(hasDefense)} 🛡️ Ironshell Boar    *(Stonehall Depths — Lv 20)*\n" +
                    $"{Check(hasHealth)}  ❤️ Tidecall Serpent  *(Drowned Archives — Lv 25)*\n\n" +
                    (hasAttack && hasDefense && hasHealth
                        ? "✨ **All 3 collected! Use `/pet-evolve confirm` to evolve!**"
                        : "Collect all 3 pets to unlock the evolution.");
        }
    }
}
