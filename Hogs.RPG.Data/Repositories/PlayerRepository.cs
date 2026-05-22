using Hogs.RPG.Core.Entities;
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

            _context.Players.Update(player);
            await _context.SaveChangesAsync();
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

    }
}