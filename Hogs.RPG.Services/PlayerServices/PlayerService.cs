using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.PlayerServices
{
    public class PlayerService
    {

        private readonly PlayerRepository _playerRepository;
        private readonly PetRepository _petRepository;

        public PlayerService(PlayerRepository playerRepository, PetRepository petRepository)
        {
            _playerRepository = playerRepository;
            _petRepository = petRepository;
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


                HunterStamina = 100,
                LastHunterStaminaUpdate = DateTimeOffset.UtcNow.ToString("o"),

                // Gathering
                Energy = 100,
                LastEnergyUpdate = DateTimeOffset.UtcNow.ToString("o")
            };

            await _playerRepository.CreatePlayerAsync(newPlayer);

            return newPlayer;

        }

        public async Task UpdatePlayerAsync(Player player)
        {
            await _playerRepository.UpdatePlayerAsync(player);
        }

        public async Task<Player> GetPlayerAsync(ulong discordId)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(discordId);

            if (player == null)
                throw new Exception("Player not found.");

            return player;
        }

        public async Task<List<PlayerPet>> GetPlayerPets(ulong discordId)
        {
            return await _petRepository.GetPetsAsync(discordId);
        }

        public async Task TransferPet(int petId, ulong fromUserId, ulong toUserId)
        {
            var pet = await _petRepository.GetByIdAsync(petId);

            if (pet == null)
                throw new Exception("Pet not found.");

            if (pet.DiscordId != fromUserId)
                throw new Exception("You do not own this pet.");

            if (pet.IsEquipped)
                throw new Exception("Cannot trade equipped pet.");

            await _petRepository.TransferPetAsync(petId, toUserId);
        }

    }
}
