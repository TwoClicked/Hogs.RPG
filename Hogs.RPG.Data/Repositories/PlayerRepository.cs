using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Data.Repositories
{
    public class PlayerRepository
    {
        private readonly IGoogleSheetsService _sheets;

        private List<Player> _players = new();
        private bool _loaded = false;

        public PlayerRepository(IGoogleSheetsService sheets)
        {
            _sheets = sheets;
        }

        private async Task EnsureLoaded()
        {
            if (_loaded)
                return;

            var rows = await _sheets.ReadRangeAsync("Players", "A2:J");

            foreach (var row in rows)
            {
                var player = new Player
                {
                    PlayerId = int.Parse(row[0].ToString()),
                    DiscordId = ulong.Parse(row[1].ToString()),
                    Username = row[2].ToString(),
                    Level = int.Parse(row[3].ToString()),
                    XP = int.Parse(row[4].ToString()),
                    Gold = int.Parse(row[5].ToString()),
                    Attack = int.Parse(row[6].ToString()),
                    Defense = int.Parse(row[7].ToString()),
                    Health = int.Parse(row[8].ToString()),
                    LastHunt = row.Count > 9 ? row[9]?.ToString() : ""
                };

                _players.Add(player);
            }

            _loaded = true;
        }

        public async Task<Player?> GetByDiscordIdAsync(ulong discordId)
        {
            await EnsureLoaded();

            return _players.FirstOrDefault(p => p.DiscordId == discordId);
        }

        public async Task<Player> CreatePlayerAsync(Player player)
        {
            await EnsureLoaded();

            // Generate next PlayerId
            player.PlayerId = _players.Count == 0
                ? 1
                : _players.Max(p => p.PlayerId) + 1;

            // Add to cache
            _players.Add(player);

            // Write to Google Sheets
            await _sheets.AppendRowAsync("Players", new object[]
            {
                player.PlayerId,
                player.DiscordId.ToString(),
                player.Username,
                player.Level,
                player.XP,
                player.Gold,
                player.Attack,
                player.Defense,
                player.Health,
                player.LastHunt
            });

            return player;
        }
    }
}