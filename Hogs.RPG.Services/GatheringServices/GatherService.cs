using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.GameData.Gathering;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.GatheringServices
{
    public class GatherService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly InventoryService _inventoryService;
        private readonly EnergyService _energyService;

        private readonly Random _random = new();

        public GatherService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            EnergyService energyService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _energyService = energyService;
        }

        public async Task<string> GatherAsync(ulong userId, string areaId, int energy)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(userId);

            if (player == null)
                return "You must start your adventure first.";

            _energyService.RegenerateEnergy(player);

            if (!GatherAreaRegistry.All.TryGetValue(areaId.ToLower(), out var area))
                return "Unknown gathering area.";

            if (energy == -1)
            {
                energy = player.Energy;
            }

            // =========================
            // VALIDATION
            // =========================
            if (energy <= 0)
                return "⚡ You have no energy left.";

            if (player.Energy < energy)
                return $"⚡ You only have {player.Energy} energy.";

            var gathered = new Dictionary<string, int>();

            // =========================
            // GATHER LOOP
            // =========================
            for (int i = 0; i < energy; i++)
            {
                double roll = _random.NextDouble();
                double cumulative = 0;

                foreach (var drop in area.Drops)
                {
                    cumulative += drop.Value;

                    if (roll <= cumulative)
                    {
                        if (!gathered.ContainsKey(drop.Key))
                            gathered[drop.Key] = 0;

                        gathered[drop.Key]++;
                        break;
                    }
                }
            }

            // =========================
            // GIVE ITEMS
            // =========================
            foreach (var item in gathered)
            {
                await _inventoryService.GiveItemAsync(userId, item.Key, item.Value);
            }

            // =========================
            // SPEND ENERGY
            // =========================
            await _energyService.SpendEnergy(player, energy);

            // =========================
            // BUILD RESULT
            // =========================
            var result = new StringBuilder();

            result.AppendLine($"🌿 You gather in the {area.Name}...\n");

            foreach (var item in gathered)
            {
                var itemName = InventoryItemDefinitions.All.TryGetValue(item.Key, out var def)
                    ? def.Name
                    : item.Key;

                result.AppendLine($"+{item.Value} {itemName}");
            }

            result.AppendLine($"\n⚡ Energy: {player.Energy}/100");

            return result.ToString();
        }
    }
}