using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities
{
    public class RaidDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int Tier { get; set; }
        public int RequiredLevel { get; set; }

        public string ImageUrl { get; set; }

        // Boss scaling multipliers (applied against average party stats)
        public float HpMultiplier { get; set; }
        public float AttackMultiplier { get; set; }
        public float DefenseMultiplier { get; set; }

        // % chance per round the boss swaps aggro target
        public float AggroSwapChance { get; set; }

        // Abilities available to this raid's boss
        public List<BossAbilityType> AbilityPool { get; set; } = new();

        // Multiple materials required to craft the key
        public List<RaidKeyIngredient> KeyIngredients { get; set; } = new();
    }
}