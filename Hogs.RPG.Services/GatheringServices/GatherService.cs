using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.GameData.Gathering;
using Hogs.RPG.Services.Game;
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
        private readonly GameEventService _gameEventService;

        private readonly Random _random = new();

        public GatherService(
            PlayerRepository playerRepository,
            InventoryService inventoryService,
            EnergyService energyService,
            GameEventService gameEventService)
        {
            _playerRepository = playerRepository;
            _inventoryService = inventoryService;
            _energyService = energyService;
            _gameEventService = gameEventService;
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
                energy = player.Energy;

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
            // 🧪 ALCHEMY XP — 2 XP per energy spent in swamp
            // =========================
            if (areaId.ToLower() == "swamp")
            {
                player.AlchemistXP += energy * 2;

                int alchemyLevelUps = 0;
                while (player.AlchemistLevel < 99)
                {
                    int xpNeeded = player.AlchemistLevel * player.AlchemistLevel * 50;
                    if (player.AlchemistXP >= xpNeeded)
                    {
                        player.AlchemistLevel++;
                        alchemyLevelUps++;
                    }
                    else break;
                }

                await _playerRepository.UpdatePlayerAsync(player);

                if (alchemyLevelUps > 0)
                    await _gameEventService.SendAlchemistLevelUpAsync(player);
            }

            // =========================
            // 🔮 DRAGON CRYSTAL — special drop
            // Only rolls when mining at Smithing level 99
            // 0.03% chance per energy spent
            // =========================
            int crystalsFound = 0;

            if (areaId.ToLower() == "mine" && player.SmithingLevel >= 99)
            {
                for (int i = 0; i < energy; i++)
                {
                    if (_random.NextDouble() < 0.0003)
                        crystalsFound++;
                }

                if (crystalsFound > 0)
                    await _inventoryService.GiveItemAsync(userId, "dragon_crystal", crystalsFound);
            }

            // =========================
            // SPEND ENERGY
            // =========================
            await _energyService.SpendEnergy(player, energy);

            // =========================
            // BUILD RESULT
            // =========================
            var result = new StringBuilder();

            bool isMine = areaId.ToLower() == "mine";
            bool isSwamp = areaId.ToLower() == "swamp";

            string gatherIcon = isMine ? "⛏️" : isSwamp ? "🌿" : "🌿";
            string gatherVerb = isMine ? "mine" : isSwamp ? "forage" : "gather";

            result.AppendLine($"{gatherIcon} You {gatherVerb} in the {area.Name}...\n");

            foreach (var item in gathered)
            {
                var itemName = InventoryItemDefinitions.All.TryGetValue(item.Key, out var def)
                    ? def.Name
                    : item.Key;

                result.AppendLine($"+{item.Value} {itemName}");
            }

            if (crystalsFound > 0)
                result.AppendLine($"\n🔮 **DRAGON CRYSTAL FOUND x{crystalsFound}!** The forge awaits.");

            int energyMax = _energyService.GetMaxEnergy(player);
            result.AppendLine($"\n⚡ Energy: {player.Energy}/{energyMax}");

            return result.ToString();
        }
    }
}