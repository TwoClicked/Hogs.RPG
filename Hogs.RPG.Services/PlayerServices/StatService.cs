using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.RelicServices;

namespace Hogs.RPG.Services.GameplayServices
{
    public class StatService
    {
        private readonly EquipmentService _equipmentService;
        private readonly PetService _petService;
        private readonly RelicService _relicService;

        public StatService(
            EquipmentService equipmentService,
            PetService petService,
            RelicService relicService)
        {
            _equipmentService = equipmentService;
            _petService = petService;
            _relicService = relicService;
        }

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
            // 🐾 PET STATS
            // =========================
            var pet = await _petService.GetEquippedPetAsync(player.DiscordId);

            if (pet != null && PetRegistry.All.TryGetValue(pet.PetId, out var petDef))
            {
                var (petAtk, petDefStat, petHp) = _petService.CalculateStats(pet);
                attack += petAtk;
                defense += petDefStat;
                health += petHp;
            }

            // =========================
            // 💎 RELIC BONUSES
            // =========================
            var relicBonuses = await _relicService.GetRelicBonusesAsync(player.DiscordId);

            attack = (int)(attack * (1f + relicBonuses.AttackPercent));
            defense = (int)(defense * (1f + relicBonuses.DefensePercent));
            health = (int)(health * (1f + relicBonuses.MaxHpPercent));

            return (attack, defense, health);
        }

        public (int attack, int defense, int health) CalculateStats(Player player)
        {
            return CalculateStatsAsync(player).GetAwaiter().GetResult();
        }
    }
}