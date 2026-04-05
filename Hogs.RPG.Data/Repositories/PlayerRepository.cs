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

    }
}