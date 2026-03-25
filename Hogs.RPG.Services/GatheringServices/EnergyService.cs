using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using System;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.GatheringServices
{
    public class EnergyService
    {
        private readonly PlayerRepository _playerRepository;

        private const int MaxEnergy = 100;
        private const int RegenMinutes = 1;

        public EnergyService(PlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public void RegenerateEnergy(Player player)
        {
            if (string.IsNullOrEmpty(player.LastEnergyUpdate))
            {
                player.LastEnergyUpdate = DateTimeOffset.UtcNow.ToString("o");
                return;
            }

            if (!DateTimeOffset.TryParse(player.LastEnergyUpdate, out var lastUpdate))
                return;

            var now = DateTimeOffset.UtcNow;
            var minutesPassed = (now - lastUpdate).TotalMinutes;

            int energyRecovered = (int)(minutesPassed / RegenMinutes);

            if (energyRecovered <= 0)
                return;

            player.Energy = Math.Min(MaxEnergy, player.Energy + energyRecovered);


            player.LastEnergyUpdate = lastUpdate
                .AddMinutes(energyRecovered * RegenMinutes)
                .ToString("o");
        }

        public bool HasEnergy(Player player, int cost)
        {
            return player.Energy >= cost;
        }

        public async Task SpendEnergy(Player player, int cost)
        {
            player.Energy -= cost;

            if (player.Energy < 0)
                player.Energy = 0;

            await _playerRepository.UpdatePlayerAsync(player);
        }
    }
}