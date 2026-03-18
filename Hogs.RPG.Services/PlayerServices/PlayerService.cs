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
                Gold = 0,
                Attack = 5,
                Defense = 5,
                Health = 100,
                MaxHealth = 100,

                // Hunt (legacy + new system)
                LastHunt = "",

                HunterStamina = 100,
                LastHunterStaminaUpdate = DateTimeOffset.UtcNow.ToString("o"),

                // Gathering
                Energy = 100,
                LastEnergyUpdate = DateTimeOffset.UtcNow.ToString("o")
            };

            await _playerRepository.CreatePlayerAsync(newPlayer);

            return newPlayer;

        }

    }
}
