using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class PetRepository
    {
        private readonly GameDbContext _context;

        public PetRepository(GameDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlayerPet>> GetPetsAsync(ulong discordId)
        {
            return await _context.PlayerPets
                .Where(x => x.DiscordId == discordId)
                .ToListAsync();
        }
        public async Task<PlayerPet?> GetByIdAsync(int petId)
        {
            return await _context.PlayerPets
                .FirstOrDefaultAsync(p => p.Id == petId);
        }

        public async Task<PlayerPet?> GetEquippedPetAsync(ulong discordId)
        {
            return await _context.PlayerPets
                .FirstOrDefaultAsync(x => x.DiscordId == discordId && x.IsEquipped);
        }

        public async Task<PlayerPet?> GetPetAsync(ulong discordId, string petId)
        {
            return await _context.PlayerPets
                .FirstOrDefaultAsync(x => x.DiscordId == discordId && x.PetId == petId);
        }

        public async Task AddPetAsync(PlayerPet pet)
        {
            _context.PlayerPets.Add(pet);
            await _context.SaveChangesAsync();
        }

        public async Task TransferPetAsync(int petId, ulong newOwnerId)
        {
            var pet = await GetByIdAsync(petId);

            if (pet == null)
                throw new Exception("Pet not found.");

            // Safety: cannot transfer equipped pets
            if (pet.IsEquipped)
                throw new Exception("Cannot transfer equipped pet.");

            pet.DiscordId = newOwnerId;
            pet.IsEquipped = false;

            await _context.SaveChangesAsync();
        }

        // =========================
        // 🐾 TOP PET LEVEL WITH PLAYER
        // =========================
        public async Task<List<PlayerPet>> GetTopPetLevelWithPlayerAsync(int count)
        {
            return await _context.PlayerPets
                .Include(p => p.Player)
                .OrderByDescending(p => p.Level)
                .Take(count)
                .ToListAsync();
        }

        // =========================
        // 🗑️ REMOVE PET (used for evolution)
        // =========================
        public void RemovePet(PlayerPet pet)
        {
            _context.PlayerPets.Remove(pet);
            // Note: caller is responsible for calling SaveAsync()
        }

        public async Task RemovePetAndSaveAsync(PlayerPet pet)
        {
            _context.PlayerPets.Remove(pet);
            await _context.SaveChangesAsync();
        }


        // =========================
        // 🐾 PET GEAR SCORE WITH PLAYER
        // =========================
        public async Task<List<PlayerPet>> GetTopForPetGearScoreWithPlayerAsync(int count)
        {
            return await _context.PlayerPets
                .Include(p => p.Player)
                .OrderByDescending(p => p.Level)
                .Take(count)
                .ToListAsync();
        }

        // =========================
        // SAVE CHANGES
        // =========================
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}