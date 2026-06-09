using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Data.Repositories
{
    public class PlayerRepository
    {
        private readonly GameDbContext _context;

        public PlayerRepository(GameDbContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetByDiscordIdAsync(ulong discordId)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.DiscordId == discordId);

            player?.DeserializeBuffs();
            return player;
        }

        public async Task<Player> CreatePlayerAsync(Player player)
        {
            player.SerializeBuffs();

            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return player;
        }

        public async Task UpdatePlayerAsync(Player player)
        {
            player.SerializeBuffs();

            player.XpBoostExpiry = ToUtc(player.XpBoostExpiry);
            player.StaminaBoostExpiry = ToUtc(player.StaminaBoostExpiry);
            player.ActiveStatBuffExpiry = ToUtc(player.ActiveStatBuffExpiry);
            player.ActiveUtilityBuffExpiry = ToUtc(player.ActiveUtilityBuffExpiry);

            _context.Players.Update(player);
            await _context.SaveChangesAsync();

            AutocompleteCache<Player>.Invalidate(player.DiscordId);
        }

        // =========================
        // LEADERBOARD QUERIES
        // =========================

        public async Task<List<Player>> GetTopGoldAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.Gold)
                .Take(count)
                .ToListAsync();

            foreach (var p in players)
                p.DeserializeBuffs();

            return players;
        }

        public async Task<List<Player>> GetTopXPAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.XP)
                .Take(count)
                .ToListAsync();

            foreach (var p in players)
                p.DeserializeBuffs();

            return players;
        }

        public async Task<List<Player>> GetTopDungeonRunsAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.DungeonRunsCompleted)
                .Take(count)
                .ToListAsync();

            foreach (var p in players)
                p.DeserializeBuffs();

            return players;
        }

        public async Task<List<Player>> GetAllPlayersAsync()
        {
            return await _context.Players.ToListAsync();
        }

        public async Task<List<Player>> GetTopForGearScoreAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.Level)
                .Take(count)
                .ToListAsync();

            foreach (var p in players)
                p.DeserializeBuffs();

            return players;
        }

        public async Task<List<Player>> GetTopRaidsCompletedAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.RaidsCompleted)
                .Take(count)
                .ToListAsync();

            foreach (var p in players) p.DeserializeBuffs();
            return players;
        }

        public async Task<List<Player>> GetTopBossDamageAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.TotalBossDamage)
                .Take(count)
                .ToListAsync();

            foreach (var p in players) p.DeserializeBuffs();
            return players;
        }

        public async Task<List<Player>> GetTopDeathsAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.Deaths)
                .Take(count)
                .ToListAsync();

            foreach (var p in players) p.DeserializeBuffs();
            return players;
        }

        public async Task<List<Player>> GetTopTrailsAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.TrailsCompleted)
                .Take(count)
                .ToListAsync();

            foreach (var p in players)
                p.DeserializeBuffs();

            return players;
        }


        // =========================
        // RANK QUERIES (position among all players)
        // =========================

        public async Task<int> GetRankByGoldAsync(ulong discordId)
        {
            var player = await GetByDiscordIdAsync(discordId);
            if (player == null) return 0;
            return await _context.Players.CountAsync(p => p.Gold > player.Gold) + 1;
        }

        public async Task<int> GetRankByLevelAsync(ulong discordId)
        {
            var player = await GetByDiscordIdAsync(discordId);
            if (player == null) return 0;
            return await _context.Players.CountAsync(p =>
                p.Level > player.Level || (p.Level == player.Level && p.XP > player.XP)) + 1;
        }

        public async Task<int> GetRankByDungeonRunsAsync(ulong discordId)
        {
            var player = await GetByDiscordIdAsync(discordId);
            if (player == null) return 0;
            return await _context.Players.CountAsync(p => p.DungeonRunsCompleted > player.DungeonRunsCompleted) + 1;
        }

        public async Task<int> GetRankByRaidsAsync(ulong discordId)
        {
            var player = await GetByDiscordIdAsync(discordId);
            if (player == null) return 0;
            return await _context.Players.CountAsync(p => p.RaidsCompleted > player.RaidsCompleted) + 1;
        }

        public async Task<int> GetRankByBossDamageAsync(ulong discordId)
        {
            var player = await GetByDiscordIdAsync(discordId);
            if (player == null) return 0;
            return await _context.Players.CountAsync(p => p.TotalBossDamage > player.TotalBossDamage) + 1;
        }

        public async Task<int> GetRankByDeathsAsync(ulong discordId)
        {
            var player = await GetByDiscordIdAsync(discordId);
            if (player == null) return 0;
            return await _context.Players.CountAsync(p => p.Deaths > player.Deaths) + 1;
        }

        public async Task<List<Player>> GetTopSmithingLevelAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.SmithingLevel)
                .ThenByDescending(p => p.SmithingXP)
                .Take(count)
                .ToListAsync();

            foreach (var p in players)
                p.DeserializeBuffs();

            return players;
        }

        private static DateTime? ToUtc(DateTime? dt)
    => dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;

        public async Task<List<Player>> GetTopAlchemistLevelAsync(int count)
        {
            var players = await _context.Players
                .OrderByDescending(p => p.AlchemistLevel)
                .ThenByDescending(p => p.AlchemistXP)
                .Take(count)
                .ToListAsync();

            foreach (var p in players)
                p.DeserializeBuffs();

            return players;
        }

        public async Task<int> GetRankByAlchemistLevelAsync(ulong discordId)
        {
            var player = await GetByDiscordIdAsync(discordId);
            if (player == null) return 0;
            return await _context.Players.CountAsync(p =>
                p.AlchemistLevel > player.AlchemistLevel ||
                (p.AlchemistLevel == player.AlchemistLevel && p.AlchemistXP > player.AlchemistXP)) + 1;
        }

        public async Task<int> GetRankBySmithingLevelAsync(ulong discordId)
        {
            var player = await GetByDiscordIdAsync(discordId);
            if (player == null) return 0;
            return await _context.Players.CountAsync(p =>
                p.SmithingLevel > player.SmithingLevel ||
                (p.SmithingLevel == player.SmithingLevel && p.SmithingXP > player.SmithingXP)) + 1;
        }

        public async Task<int> GetRankByTrailsAsync(ulong discordId)
        {
            var player = await GetByDiscordIdAsync(discordId);
            if (player == null) return 0;
            return await _context.Players.CountAsync(p => p.TrailsCompleted > player.TrailsCompleted) + 1;
        }

        public async Task<int> GetTotalPlayerCountAsync()
            => await _context.Players.CountAsync();
    }
}