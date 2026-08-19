using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AchievementServices;

namespace Hogs.RPG.Services.PetServices
{
    public class EvolvePetService
    {
        private readonly PetRepository _repo;
        private readonly PetService _petService;
        private readonly PlayerRepository _playerRepository;
        private readonly AchievementService _achievementService;

        private const string AttackPetId = "armored_capybara";
        private const string DefensePetId = "el_tata_de_frog";
        private const string HealthPetId = "ice_wolf";
        private const string Tier3PetId = "capytara";

        public EvolvePetService(PetRepository repo, PetService petService, PlayerRepository playerRepository, AchievementService achievementService)
        {
            _repo = repo;
            _petService = petService;
            _playerRepository = playerRepository;
            _achievementService = achievementService;
        }

        // =========================
        // EVOLVE
        // =========================
        public async Task<(bool success, string message)> EvolveAsync(ulong userId)
        {
            var pets = await _repo.GetPetsAsync(userId);

            var attackPet = pets.FirstOrDefault(p => p.PetId == AttackPetId && !p.IsEquipped);
            var defensePet = pets.FirstOrDefault(p => p.PetId == DefensePetId && !p.IsEquipped);
            var healthPet = pets.FirstOrDefault(p => p.PetId == HealthPetId && !p.IsEquipped);

            if (attackPet == null || defensePet == null || healthPet == null)
            {
                string Describe(string petId, PlayerPet? unequippedMatch, string label)
                {
                    if (unequippedMatch != null) return "";

                    bool ownsAnyEquipped = pets.Any(p => p.PetId == petId && p.IsEquipped);
                    return ownsAnyEquipped
                        ? $"{label} (equipped — unequip it first)"
                        : $"{label} (missing)";
                }

                var missing = new List<string>();
                var attackLine = Describe(AttackPetId, attackPet, "⚔️ **Armored Capybara** (Attack pet — from Blazewing's Gorge)");
                var defenseLine = Describe(DefensePetId, defensePet, "🛡️ **El Tata de Frog** (Defense pet — from Stonehall Depths)");
                var healthLine = Describe(HealthPetId, healthPet, "❤️ **Ice Wolf** (Health pet — from Drowned Archives)");

                if (attackLine != "") missing.Add(attackLine);
                if (defenseLine != "") missing.Add(defenseLine);
                if (healthLine != "") missing.Add(healthLine);

                return (false, $"❌ You're not ready to evolve yet:\n{string.Join("\n", missing)}");
            }

            _repo.RemovePet(attackPet);
            _repo.RemovePet(defensePet);
            _repo.RemovePet(healthPet);

            await _petService.GivePetAsync(userId, Tier3PetId);

            if (!PetRegistry.All.TryGetValue(Tier3PetId, out var capytara))
                return (false, "❌ Evolution failed — Tier 3 pet not found in registry.");

            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player != null)
            {
                player.CapyTaraEvolved = true;
                await _playerRepository.UpdatePlayerAsync(player);
                await _achievementService.CheckAndAwardAsync(userId);
            }

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