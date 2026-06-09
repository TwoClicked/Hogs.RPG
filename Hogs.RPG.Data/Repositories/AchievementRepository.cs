using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.AchievementObjects;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class AchievementRepository
    {
        private readonly GameDbContext _context;

        public AchievementRepository(GameDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL FOR PLAYER
        // =========================
        public async Task<List<PlayerAchievement>> GetAllAsync(ulong discordId)
        {
            return await _context.PlayerAchievements
                .Where(a => a.DiscordId == discordId)
                .OrderByDescending(a => a.EarnedAt)
                .ToListAsync();
        }

        // =========================
        // CHECK IF ALREADY EARNED
        // =========================
        public async Task<bool> HasEarnedAsync(ulong discordId, string achievementId)
        {
            return await _context.PlayerAchievements
                .AnyAsync(a => a.DiscordId == discordId && a.AchievementId == achievementId);
        }

        // =========================
        // AWARD ACHIEVEMENT
        // =========================
        public async Task AwardAsync(ulong discordId, string achievementId, bool isRetroactive = false)
        {
            var existing = await _context.PlayerAchievements
                .AnyAsync(a => a.DiscordId == discordId && a.AchievementId == achievementId);

            if (existing) return;

            _context.PlayerAchievements.Add(new PlayerAchievement
            {
                DiscordId = discordId,
                AchievementId = achievementId,
                EarnedAt = isRetroactive ? DateTime.MinValue : DateTime.UtcNow,
                IsRetroactive = isRetroactive
            });

            await _context.SaveChangesAsync();
        }

        // =========================
        // COUNT FOR PLAYER
        // =========================
        public async Task<int> GetCountAsync(ulong discordId)
        {
            return await _context.PlayerAchievements
                .CountAsync(a => a.DiscordId == discordId);
        }

        // =========================
        // GET ALL PLAYERS WITH ACHIEVEMENT COUNT (for leaderboard)
        // =========================
        public async Task<List<(ulong DiscordId, int Count)>> GetTopByCountAsync(int take = 5)
        {
            return await _context.PlayerAchievements
                .GroupBy(a => a.DiscordId)
                .Select(g => new { DiscordId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(take)
                .Select(x => new ValueTuple<ulong, int>((ulong)x.DiscordId, x.Count))
                .ToListAsync();
        }

        // =========================
        // GET RANK BY ACHIEVEMENT COUNT
        // =========================
        public async Task<int> GetRankAsync(ulong discordId)
        {
            int myCount = await GetCountAsync(discordId);

            int rank = await _context.PlayerAchievements
                .GroupBy(a => a.DiscordId)
                .Select(g => new { DiscordId = g.Key, Count = g.Count() })
                .CountAsync(x => x.Count > myCount);

            return rank + 1;
        }

        // =========================
        // GET ALL PLAYERS FOR RETROACTIVE MIGRATION
        // =========================
        public async Task<List<ulong>> GetAllPlayerDiscordIdsAsync()
        {
            return await _context.Players
                .Select(p => (ulong)p.DiscordId)
                .ToListAsync();
        }
    }
}