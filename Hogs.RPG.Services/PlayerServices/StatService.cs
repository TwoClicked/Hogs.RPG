using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.PetServices;

namespace Hogs.RPG.Services.GameplayServices
{
    public class StatService
    {
        private readonly EquipmentService _equipmentService;
        private readonly PetService _petService;

        public StatService(EquipmentService equipmentService, PetService petService)
        {
            _equipmentService = equipmentService;
            _petService = petService;
        }

        /// <summary>
        /// ✅ MAIN METHOD (ASYNC)
        /// Calculates FINAL stats including:
        /// - Base player stats
        /// - Equipment
        /// - Pet stats
        /// - Pet passives
        /// </summary>
        public async Task<(int attack, int defense, int health)> CalculateStatsAsync(Player player)
        {
            // =========================
            // 🧍 BASE STATS
            // =========================
            int attack = player.Attack;
            int defense = player.Defense;
            int health = player.MaxHealth;

            // =========================
            // 🛡 EQUIPMENT
            // =========================
            var equippedItems = new[]
            {
                player.MainHand,
                player.OffHand,
                player.Helmet,
                player.Body,
                player.Legs,
                player.Gloves,
                player.Boots,
                player.Ring,
                player.Amulet
            };

            foreach (var itemId in equippedItems)
            {
                if (string.IsNullOrEmpty(itemId))
                    continue;

                var item = _equipmentService.GetEquipment(itemId);
                if (item == null)
                    continue;

                attack += item.Attack;
                defense += item.Defense;
                health += item.Health;
            }

            // =========================
            // 🐾 PET STATS + PASSIVES
            // =========================
            var pet = await _petService.GetEquippedPetAsync(player.DiscordId);

            if (pet != null && PetRegistry.All.TryGetValue(pet.PetId, out var petDef))
            {
                // 🔢 Base pet stats (scaling)
                var (petAtk, petDefStat, petHp) = _petService.CalculateStats(pet);

                attack += petAtk;
                defense += petDefStat;
                health += petHp;

            }

            return (attack, defense, health);
        }

        /// <summary>
        /// ⚠️ SYNC WRAPPER (DO NOT REMOVE YET)
        /// Keeps old code working without refactoring everything
        /// </summary>
        public (int attack, int defense, int health) CalculateStats(Player player)
        {
            return CalculateStatsAsync(player).GetAwaiter().GetResult();
        }
    }
}