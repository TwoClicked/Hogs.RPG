using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class RelicRepository
    {
        private readonly GameDbContext _context;

        public RelicRepository(GameDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL RELICS
        // =========================
        public async Task<List<PlayerRelic>> GetRelicsAsync(ulong discordId)
        {
            return await _context.PlayerRelics
                .Where(r => r.DiscordId == discordId)
                .ToListAsync();
        }

        // =========================
        // GET EQUIPPED RELICS
        // =========================
        public async Task<List<PlayerRelic>> GetEquippedRelicsAsync(ulong discordId)
        {
            return await _context.PlayerRelics
                .Where(r => r.DiscordId == discordId && r.IsEquipped)
                .ToListAsync();
        }

        // =========================
        // GET BY ID
        // =========================
        public async Task<PlayerRelic?> GetByIdAsync(int relicId)
        {
            return await _context.PlayerRelics
                .FirstOrDefaultAsync(r => r.Id == relicId);
        }

        // =========================
        // ADD RELIC
        // =========================
        public async Task AddRelicAsync(PlayerRelic relic)
        {
            _context.PlayerRelics.Add(relic);
            await _context.SaveChangesAsync();
        }

        // =========================
        // GET SHARDS
        // =========================
        public async Task<List<PlayerRelicShard>> GetShardsAsync(ulong discordId)
        {
            return await _context.PlayerRelicShards
                .Where(s => s.DiscordId == discordId)
                .ToListAsync();
        }

        public async Task<PlayerRelicShard?> GetShardByTierAsync(ulong discordId, int tier)
        {
            return await _context.PlayerRelicShards
                .FirstOrDefaultAsync(s => s.DiscordId == discordId && s.Tier == tier);
        }

        // =========================
        // ADD SHARD
        // =========================
        public async Task AddShardAsync(ulong discordId, int tier, int amount)
        {
            var shard = await GetShardByTierAsync(discordId, tier);

            if (shard == null)
            {
                _context.PlayerRelicShards.Add(new PlayerRelicShard
                {
                    DiscordId = discordId,
                    Tier = tier,
                    Quantity = amount
                });
            }
            else
            {
                shard.Quantity += amount;
            }

            await _context.SaveChangesAsync();
        }

        // =========================
        // REMOVE SHARD
        // =========================
        public async Task<bool> RemoveShardAsync(ulong discordId, int tier, int amount)
        {
            var shard = await GetShardByTierAsync(discordId, tier);

            if (shard == null || shard.Quantity < amount)
                return false;

            shard.Quantity -= amount;

            if (shard.Quantity <= 0)
                _context.PlayerRelicShards.Remove(shard);

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // SAVE
        // =========================
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}