using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities.RaidObjects
{
    public class RelicDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public RelicAffinity Affinity { get; set; }
        public RelicBonusType BonusType { get; set; }

        // Index 0 = Rank 1, Index 4 = Rank 5
        public float[] BonusPerRank { get; set; } = new float[5];

        // Null for pure stat bonuses, populated for proc/special effects
        public string? SpecialEffectDescription { get; set; }
    }
}