using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.GameData.Achievements;
using Hogs.RPG.Core.GameData.Enhancement;
using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.RelicServices;
using Hogs.RPG.Services.TowerServices;

namespace Hogs.RPG.Services.GameplayServices
{
    public class StatService
    {
        private readonly EquipmentService _equipmentService;
        private readonly PetService _petService;
        private readonly RelicService _relicService;
        private readonly SigilService _sigilService;

        public StatService(
            EquipmentService equipmentService,
            PetService petService,
            RelicService relicService,
            SigilService sigilService)
        {
            _equipmentService = equipmentService;
            _petService = petService;
            _relicService = relicService;
            _sigilService = sigilService;
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
            var equippedSlots = new (EquipmentSlot Slot, string? ItemId)[]
            {
                (EquipmentSlot.MainHand, player.MainHand),
                (EquipmentSlot.OffHand, player.OffHand),
                (EquipmentSlot.Helmet, player.Helmet),
                (EquipmentSlot.Body, player.Body),
                (EquipmentSlot.Legs, player.Legs),
                (EquipmentSlot.Gloves, player.Gloves),
                (EquipmentSlot.Boots, player.Boots),
                (EquipmentSlot.Ring, player.Ring),
                (EquipmentSlot.Amulet, player.Amulet)
            };

            foreach (var (slot, itemId) in equippedSlots)
            {
                if (string.IsNullOrEmpty(itemId))
                    continue;

                var item = _equipmentService.GetEquipment(itemId);
                if (item == null)
                    continue;

                attack += item.Attack;
                defense += item.Defense;
                health += item.Health;

                // 🔨 ENHANCEMENT BONUS
                // Only applies when the item equipped in this slot IS the
                // Global Boss Gear piece for that slot. A player can bank
                // enhancement levels on a slot before owning the piece
                // (see Player.cs), but the stat bonus stays dormant until
                // the real item is actually equipped here.
                if (itemId == EnhancementSlotMap.GetGlobalBossItemId(slot))
                {
                    int enhanceLevel = EnhancementSlotMap.GetEnhancementLevel(player, slot);
                    var (enhAtk, enhDef, enhHp) = EnhancementStatGains.GetCumulativeBonus(enhanceLevel);

                    attack += enhAtk;
                    defense += enhDef;
                    health += enhHp;
                }
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
            // ✨ SIGIL BONUSES (Tower of Doom)
            // =========================
            var sigilBonuses = await _sigilService.GetSigilBonusesAsync(player.DiscordId);

            attack = (int)(attack * (1f + sigilBonuses.AttackPercent));
            defense = (int)(defense * (1f + sigilBonuses.DefensePercent));
            health = (int)(health * (1f + sigilBonuses.MaxHpPercent));

            // =========================
            // 🏆 ACHIEVEMENT MILESTONE BONUSES
            // Flat stat bonuses from achievement count milestones
            // =========================
            var achBonus = AchievementMilestones.GetBonus(player.AchievementCount);
            attack += achBonus.BonusAttack;
            defense += achBonus.BonusDefense;
            health += achBonus.BonusHealth;

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