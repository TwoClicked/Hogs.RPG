// Hogs.RPG.Core/Entities/EquipmentDefinition.cs

using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class EquipmentDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public EquipmentSlot Slot { get; set; }

        // =========================
        // COMBAT STATS
        // =========================
        public int Attack { get; set; } = 0;

        public int Defense { get; set; } = 0;

        public int Health { get; set; } = 0;

        // =========================
        // HUNTING BONUSES
        // Per-piece bonus applied during /hunt
        // e.g. 0.005 = +0.5% XP and materials per piece
        // =========================
        public double HuntXpBonus { get; set; } = 0;

        public double HuntMaterialBonus { get; set; } = 0;

        // Marks this item as part of the hunting gear set.
        // Used to check for the 9-piece set bonus (+5% rare drop rate).
        public bool IsHuntingGear { get; set; } = false;
    }
}