// Hogs.RPG.Data/Repositories/GearSetRepository.cs

using Hogs.RPG.Core.Entities.EquipmentObjects;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class GearSetRepository
    {
        private readonly GameDbContext _context;

        public GearSetRepository(GameDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL SETS FOR PLAYER
        // =========================
        public async Task<List<GearSet>> GetSetsAsync(ulong discordId)
        {
            return await _context.GearSets
                .Where(g => g.DiscordId == discordId)
                .OrderBy(g => g.SetIndex)
                .ToListAsync();
        }

        // =========================
        // GET SINGLE SET BY INDEX
        // =========================
        public async Task<GearSet?> GetSetAsync(ulong discordId, int setIndex)
        {
            return await _context.GearSets
                .FirstOrDefaultAsync(g => g.DiscordId == discordId && g.SetIndex == setIndex);
        }

        // =========================
        // SAVE (UPSERT)
        // =========================
        public async Task SaveSetAsync(GearSet gearSet)
        {
            var existing = await GetSetAsync(gearSet.DiscordId, gearSet.SetIndex);

            if (existing == null)
            {
                _context.GearSets.Add(gearSet);
            }
            else
            {
                existing.SetName = gearSet.SetName;
                existing.MainHand = gearSet.MainHand;
                existing.OffHand = gearSet.OffHand;
                existing.Helmet = gearSet.Helmet;
                existing.Body = gearSet.Body;
                existing.Legs = gearSet.Legs;
                existing.Gloves = gearSet.Gloves;
                existing.Boots = gearSet.Boots;
                existing.Ring = gearSet.Ring;
                existing.Amulet = gearSet.Amulet;

                _context.GearSets.Update(existing);
            }

            await _context.SaveChangesAsync();
        }

        // =========================
        // GET ALL SLOT 1 SETS (for pre-boss auto-swap)
        // =========================
        public async Task<List<GearSet>> GetAllSlot1SetsAsync()
        {
            return await _context.GearSets
                .Where(g => g.SetIndex == 1)
                .ToListAsync();
        }

        // =========================
        // DELETE
        // =========================
        public async Task DeleteSetAsync(ulong discordId, int setIndex)
        {
            var set = await GetSetAsync(discordId, setIndex);
            if (set == null) return;

            _context.GearSets.Remove(set);
            await _context.SaveChangesAsync();
        }
    }
}