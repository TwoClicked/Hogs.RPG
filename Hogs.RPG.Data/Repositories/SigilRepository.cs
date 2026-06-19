using Hogs.RPG.Core.Entities.SigilObjects;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class SigilRepository
    {
        private readonly GameDbContext _context;

        public SigilRepository(GameDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlayerSigil>> GetSigilsAsync(ulong discordId)
        {
            return await _context.PlayerSigils
                .Where(s => s.DiscordId == discordId)
                .ToListAsync();
        }

        public async Task<PlayerSigil?> GetSigilAsync(ulong discordId, string sigilId)
        {
            return await _context.PlayerSigils
                .FirstOrDefaultAsync(s => s.DiscordId == discordId && s.SigilId == sigilId);
        }

        public async Task<int> GetCountAsync(ulong discordId, string sigilId)
        {
            var sigil = await GetSigilAsync(discordId, sigilId);
            return sigil?.Count ?? 0;
        }

        public async Task IncrementAsync(ulong discordId, string sigilId)
        {
            var existing = await GetSigilAsync(discordId, sigilId);
            if (existing == null)
            {
                _context.PlayerSigils.Add(new PlayerSigil
                {
                    DiscordId = discordId,
                    SigilId = sigilId,
                    Count = 1
                });
            }
            else
            {
                existing.Count++;
                _context.PlayerSigils.Update(existing);
            }

            await _context.SaveChangesAsync();
        }
    }
}
