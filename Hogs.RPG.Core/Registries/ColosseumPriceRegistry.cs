using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.GameData.Colosseum;

namespace Hogs.RPG.Core.Registries
{
    public static class ColosseumPriceRegistry
    {
        public static int GetGearCost(string equipmentId)
        {
            if (!ColosseumGearPrices.ApCostByItemId.TryGetValue(equipmentId, out var cost))
                throw new Exception($"Colosseum: equipment '{equipmentId}' has no configured AP cost.");

            return cost;
        }

        public static string ResolveGearId(EquipmentSlot slot, string? purchasedItemId)
        {
            if (!string.IsNullOrEmpty(purchasedItemId))
                return purchasedItemId;

            if (!ColosseumGearPrices.T1BaselineBySlot.TryGetValue(slot, out var baselineId))
                throw new Exception($"Colosseum: no T1 baseline configured for slot '{slot}'.");

            return baselineId;
        }

        public static int GetPetCost(string petId)
        {
            if (!ColosseumPetPrices.ApCostByPetId.TryGetValue(petId, out var cost))
                throw new Exception($"Colosseum: pet '{petId}' has no configured AP cost.");

            return cost;
        }

        public static string ResolvePetId(string? purchasedPetId) =>
            string.IsNullOrEmpty(purchasedPetId) ? ColosseumPetPrices.T1BaselinePetId : purchasedPetId;

        public static int GetPassiveCost(PetPassive passive)
        {
            if (!ColosseumPetPrices.ApCostByPassive.TryGetValue(passive, out var cost))
                throw new Exception($"Colosseum: passive '{passive}' has no configured AP cost.");

            return cost;
        }

        public static int GetBuffCost(BuffStat stat) => stat switch
        {
            BuffStat.Attack => ColosseumBuffShop.AttackBuffCost,
            BuffStat.Defense => ColosseumBuffShop.DefenseBuffCost,
            BuffStat.Health => ColosseumBuffShop.HealthBuffCost,
            _ => throw new Exception($"Colosseum: unknown buff stat '{stat}'.")
        };

        // =========================
        // GEAR OPTIONS BY SLOT
        // Computed from EquipmentRegistry + ColosseumGearPrices rather than
        // hand-listed a second time, so it can't drift out of sync. Excludes
        // T1 (cost 0) since that's the free baseline, not a purchase.
        // Used by ColosseumBotBuilderService and the DM build UI to know
        // what's actually pickable for a given slot.
        // =========================
        public static readonly Dictionary<EquipmentSlot, List<string>> PurchasableGearOptionsBySlot =
            EquipmentRegistry.All.Values
                .Where(e => ColosseumGearPrices.ApCostByItemId.TryGetValue(e.Id, out var cost) && cost > 0)
                .GroupBy(e => e.Slot)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Id).ToList());
    }
}