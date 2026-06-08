using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.Registries;
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

            // =========================
            // 🧪 ACTIVE STAT BUFF (Alchemist potions)
            // Applied last so percentage is on top of all gear + pet + relic
            // =========================
            if (player.ActiveStatBuffId != null &&
                player.ActiveStatBuffExpiry.HasValue &&
                player.ActiveStatBuffExpiry.Value > DateTime.UtcNow)
            {
                if (AlchemyPotionRegistry.All.TryGetValue(player.ActiveStatBuffId, out var statPotion))
                {
                    // Primary effect
                    if (statPotion.EffectId == "atk_boost")
                        attack = (int)(attack * (1 + statPotion.EffectValue / 100.0));
                    else if (statPotion.EffectId == "def_boost")
                        defense = (int)(defense * (1 + statPotion.EffectValue / 100.0));

                    // Secondary effect
                    if (statPotion.SecondaryEffectId == "def_penalty")
                        defense = (int)(defense * (1 - statPotion.SecondaryEffectValue / 100.0));
                    else if (statPotion.SecondaryEffectId == "atk_penalty")
                        attack = (int)(attack * (1 - statPotion.SecondaryEffectValue / 100.0));
                    else if (statPotion.SecondaryEffectId == "hp_penalty")
                        health = (int)(health * (1 - statPotion.SecondaryEffectValue / 100.0));
                    else if (statPotion.SecondaryEffectId == "def_boost")
                        defense = (int)(defense * (1 + statPotion.SecondaryEffectValue / 100.0));
                    else if (statPotion.SecondaryEffectId == "atk_boost")
                        attack = (int)(attack * (1 + statPotion.SecondaryEffectValue / 100.0));
                }
            }

            return (attack, defense, health);
        }

        public (int attack, int defense, int health) CalculateStats(Player player)
        {
            return CalculateStatsAsync(player).GetAwaiter().GetResult();
        }
    }
}