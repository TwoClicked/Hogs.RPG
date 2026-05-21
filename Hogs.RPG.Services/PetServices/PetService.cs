using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;

namespace Hogs.RPG.Services.PetServices
{
    public class PetService
    {
        private readonly PetRepository _repo;
        private readonly PlayerRepository _playerRepo;

        public PetService(PetRepository repo, PlayerRepository playerRepo)
        {
            _repo = repo;
            _playerRepo = playerRepo;
        }

        // =========================
        // 🐾 GIVE PET
        // =========================
        public async Task GivePetAsync(ulong userId, string petId)
        {
            var existing = await _repo.GetPetAsync(userId, petId);

            if (existing != null)
                return;

            var pet = new PlayerPet
            {
                DiscordId = userId,
                PetId = petId
            };

            await _repo.AddPetAsync(pet);
        }

        // =========================
        // 🐾 EQUIP PET
        // =========================
        public async Task EquipPetAsync(ulong userId, string petId)
        {
            var pets = await _repo.GetPetsAsync(userId);

            if (pets.Any(p => p.PetId == petId && p.IsEquipped))
                return;

            foreach (var pet in pets)
            {
                pet.IsEquipped = pet.PetId == petId;
            }

            await _repo.SaveAsync();
        }

        // =========================
        // 🐾 GET EQUIPPED
        // =========================
        public async Task<PlayerPet?> GetEquippedPetAsync(ulong userId)
        {
            return await _repo.GetEquippedPetAsync(userId);
        }

        // =========================
        // 🧮 CALCULATE STATS
        // =========================
        public (int atk, int def, int hp) CalculateStats(PlayerPet pet)
        {
            if (!PetRegistry.All.TryGetValue(pet.PetId, out var def))
                return (0, 0, 0);

            int atk = def.BaseAttack + (int)(pet.Level * def.Scaling);
            int defense = def.BaseDefense + (int)(pet.Level * def.Scaling);
            int hp = def.BaseHealth + (int)(pet.Level * def.Scaling * 5);

            return (atk, defense, hp);
        }

        public int GetPetGearScore(PlayerPet pet)
        {
            var def = PetRegistry.Get(pet.PetId);

            int attack = def.BaseAttack + (int)(pet.Level * def.Scaling);
            int defense = def.BaseDefense + (int)(pet.Level * def.Scaling);
            int health = def.BaseHealth + (int)(pet.Level * def.Scaling);

            return attack + defense + health;
        }

        // =========================
        // 📈 ADD XP
        // =========================
        public async Task<(bool leveledUp, int newLevel)> AddXPAsync(ulong userId, int amount)
        {
            var pet = await _repo.GetEquippedPetAsync(userId);
            if (pet == null) return (false, 0);

            pet.XP += amount;

            bool leveledUp = false;

            while (true)
            {
                int required = 20 + (pet.Level * pet.Level * 15);

                if (pet.XP < required)
                    break;

                pet.XP -= required;
                pet.Level++;
                leveledUp = true;

                // 🎯 LEVEL 15 → FIRST PASSIVE
                if (pet.Level == 15 && pet.Passive1 == null)
                {
                    pet.Passive1 = GetRandomPassiveExcluding();
                }

                // 🎯 LEVEL 20 → SECOND PASSIVE
                if (pet.Level == 20 && pet.Passive2 == null)
                {
                    pet.Passive2 = GetRandomPassiveExcluding(pet.Passive1);
                }
            }

            await _repo.SaveAsync();

            return (leveledUp, pet.Level);
        }

        // =========================
        // 🐾 GET ALL PETS
        // =========================
        public async Task<List<PlayerPet>> GetPetsAsync(ulong userId)
        {
            return await _repo.GetPetsAsync(userId);
        }

        // =========================
        // 🐾 UNEQUIP PET
        // =========================
        public async Task UnequipPetAsync(ulong userId)
        {
            var pets = await _repo.GetPetsAsync(userId);

            foreach (var pet in pets)
            {
                pet.IsEquipped = false;
            }

            await _repo.SaveAsync();
        }

        // =========================
        // 🎲 REROLL PASSIVE
        // =========================
        public async Task<(bool success, string message)> RerollPassiveAsync(ulong userId, int slot, string sacrificePetId)
        {
            const int RerollCost = 5000;

            // Must have an equipped pet
            var equippedPet = await _repo.GetEquippedPetAsync(userId);
            if (equippedPet == null)
                return (false, "❌ You don't have a pet equipped.");

            // Slot must be unlocked
            if (slot == 1 && equippedPet.Passive1 == null)
                return (false, "❌ Your pet hasn't unlocked Passive Slot 1 yet. Reach **Level 15** first.");

            if (slot == 2 && equippedPet.Passive2 == null)
                return (false, "❌ Your pet hasn't unlocked Passive Slot 2 yet. Reach **Level 20** first.");

            // Sacrifice pet must be owned and unequipped
            var sacrificePet = await _repo.GetPetAsync(userId, sacrificePetId);
            if (sacrificePet == null)
                return (false, "❌ You don't own that pet.");

            if (sacrificePet.IsEquipped)
                return (false, "❌ You can't sacrifice your equipped pet. Unequip it first.");

            // Sacrifice pet must be Tier 2
            if (!PetRegistry.All.TryGetValue(sacrificePetId, out var sacrificeDef) || sacrificeDef.Tier != 2)
                return (false, "❌ Only **Tier 2** pets can be used as a reroll sacrifice.");

            // Gold check
            var player = await _playerRepo.GetByDiscordIdAsync(userId);
            if (player == null)
                return (false, "❌ Player not found.");

            if (player.Gold < RerollCost)
                return (false, $"❌ You need **{RerollCost:N0} gold** to reroll. You only have **{player.Gold:N0}**.");

            // All checks passed — execute
            player.Gold -= RerollCost;

            PetPassive oldPassive;
            PetPassive newPassive;

            if (slot == 1)
            {
                oldPassive = equippedPet.Passive1!.Value;
                newPassive = GetRandomPassiveExcluding(equippedPet.Passive2);
                equippedPet.Passive1 = newPassive;
            }
            else
            {
                oldPassive = equippedPet.Passive2!.Value;
                newPassive = GetRandomPassiveExcluding(equippedPet.Passive1);
                equippedPet.Passive2 = newPassive;
            }

            _repo.RemovePet(sacrificePet);
            await _playerRepo.UpdatePlayerAsync(player);
            await _repo.SaveAsync();

            if (!PetRegistry.All.TryGetValue(equippedPet.PetId, out var petDef))
                return (false, "❌ Equipped pet not found in registry.");

            string displayName = equippedPet.CustomName ?? petDef.Name;
            string sacrificeName = $"{sacrificeDef.Icon} {sacrificeDef.Name}";

            return (true,
                $"🎲 **Passive Rerolled!**\n\n" +
                $"{sacrificeName} was consumed.\n\n" +
                $"**{displayName}** — Slot {slot}\n" +
                $"~~{PetPassiveFormatter.Format(oldPassive)}~~\n" +
                $"✨ {PetPassiveFormatter.Format(newPassive)}");
        }

        private PetPassive GetRandomPassiveExcluding(params PetPassive?[] exclude)
        {
            var available = Enum.GetValues(typeof(PetPassive))
                .Cast<PetPassive>()
                .Where(p => !exclude.Contains(p))
                .ToList();

            // Safety check (should never happen unless enum is tiny)
            if (available.Count == 0)
                throw new Exception("No available passives to assign.");

            return available[new Random().Next(available.Count)];
        }
    }
}