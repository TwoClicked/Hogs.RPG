using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.PlayerServices
{
    public class PlayerService
    {

        private readonly PlayerRepository _playerRepository;

        public PlayerService(PlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<Player> GetOrCreatePlayerAsync(ulong discordId, string username)
        {

            var player = await _playerRepository.GetByDiscordIdAsync(discordId); // Fetch player

            if (player != null)
                return player; // If player already exists, Return his character

            var newPlayer = new Player
            {
                DiscordId = discordId,
                Username = username,
                Level = 1,
                XP = 0,
                Gold = 50,
                Attack = 5,
                Defense = 3,
                Health = 25,
                LastHunt = ""
            };

            await _playerRepository.CreatePlayerAsync(newPlayer);

            return newPlayer;

        }

    }
}
