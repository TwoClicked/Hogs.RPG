using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.GameData.Colosseum;
using Hogs.RPG.Core.GameData.Registries;

namespace Hogs.RPG.Core.Registries
{
    /// <summary>
    /// Single source of truth for Colosseum stat math - both
    /// ColosseumCombatService (actual combat) and the DM build UI (display)
    /// call into this, so the numbers a player sees while building are
    /// guaranteed to match what combat actually uses.
    /// </summary>
    public static class ColosseumStatCalculator
    {
        public const int BaseAttack = 5;
        public const int BaseDefense = 5;
        public const int BaseHealth = 100;
        public const int ColosseumPetLevel = 15;

        // Full build totals - base + all 9 gear slots + pet + buffs.
        public static (int attack, int defense, int health) CalculateStats(ColosseumBuild build)
        {
            int attack = BaseAttack;
            int defense = BaseDefense;
            int health = BaseHealth;

            var slots = new[]
            {
                (EquipmentSlot.MainHand, build.GearMainHandId),
                (EquipmentSlot.OffHand, build.GearOffHandId),
                (EquipmentSlot.Helmet, build.GearHelmetId),
                (EquipmentSlot.Body, build.GearBodyId),
                (EquipmentSlot.Legs, build.GearLegsId),
                (EquipmentSlot.Gloves, build.GearGlovesId),
                (EquipmentSlot.Boots, build.GearBootsId),
                (EquipmentSlot.Ring, build.GearRingId),
                (EquipmentSlot.Amulet, build.GearAmuletId),
            };

            foreach (var (slot, purchasedId) in slots)
            {
                var itemId = ColosseumPriceRegistry.ResolveGearId(slot, purchasedId);
                var (a, d, h) = GetGearItemStats(itemId);
                attack += a;
                defense += d;
                health += h;
            }

            var petId = ColosseumPriceRegistry.ResolvePetId(build.PetId);
            var (petA, petD, petH) = GetPetStats(petId);
            attack += petA;
            defense += petD;
            health += petH;

            attack += build.BuffAttackPurchases * ColosseumBuffShop.AttackBuffAmount;
            defense += build.BuffDefensePurchases * ColosseumBuffShop.DefenseBuffAmount;
            health += build.BuffHealthPurchases * ColosseumBuffShop.HealthBuffAmount;

            return (attack, defense, health);
        }

        // A single gear item's own stat contribution - used for select menu
        // option descriptions and per-slot summary lines.
        public static (int attack, int defense, int health) GetGearItemStats(string itemId)
        {
            return EquipmentRegistry.All.TryGetValue(itemId, out var item)
                ? (item.Attack, item.Defense, item.Health)
                : (0, 0, 0);
        }

        // A pet's stat contribution at the fixed Colosseum pet level.
        public static (int attack, int defense, int health) GetPetStats(string petId)
        {
            if (!PetRegistry.All.TryGetValue(petId, out var petDef))
                return (0, 0, 0);

            int attack = petDef.BaseAttack + (int)(ColosseumPetLevel * petDef.Scaling);
            int defense = petDef.BaseDefense + (int)(ColosseumPetLevel * petDef.Scaling);
            int health = petDef.BaseHealth + (int)(ColosseumPetLevel * petDef.Scaling * 5);

            return (attack, defense, health);
        }

        // Formats a stat triple as compact display text, e.g. "+15 ATK / +3 HP"
        // - omits any stat that's zero so items with a single-stat focus
        // (a pure attack weapon, say) don't show "+0 DEF" clutter.
        public static string FormatStats(int attack, int defense, int health)
        {
            var parts = new List<string>();
            if (attack != 0) parts.Add($"{(attack > 0 ? "+" : "")}{attack} ATK");
            if (defense != 0) parts.Add($"{(defense > 0 ? "+" : "")}{defense} DEF");
            if (health != 0) parts.Add($"{(health > 0 ? "+" : "")}{health} HP");
            return parts.Count > 0 ? string.Join(" / ", parts) : "No stats";
        }
    }
}