using Hogs.RPG.Core.Entities.TowerObjects;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class TowerCompletedThreadRepository
    {
        private readonly GameDbContext _context;

        public TowerCompletedThreadRepository(GameDbContext context)
        {
            _context = context;
        }

        // Queues a thread for the next daily cleanup pass. Safe to call more than once
        // for the same thread (e.g. if a run somehow ends twice) — duplicates are ignored.
        public async Task MarkCompletedAsync(ulong threadId)
        {
            bool exists = await _context.TowerCompletedThreads.AnyAsync(t => t.ThreadId == threadId);
            if (exists) return;

            _context.TowerCompletedThreads.Add(new TowerCompletedThread { ThreadId = threadId });
            await _context.SaveChangesAsync();
        }

        public async Task<List<ulong>> GetPendingAsync()
        {
            return await _context.TowerCompletedThreads
                .Select(t => t.ThreadId)
                .ToListAsync();
        }

        public async Task RemoveAsync(ulong threadId)
        {
            var entity = await _context.TowerCompletedThreads.FirstOrDefaultAsync(t => t.ThreadId == threadId);
            if (entity == null) return;

            _context.TowerCompletedThreads.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}