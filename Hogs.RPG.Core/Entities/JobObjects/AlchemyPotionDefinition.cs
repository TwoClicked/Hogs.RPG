namespace Hogs.RPG.Core.Entities
{
    public class AlchemyPotionDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }

        // Alchemist level required to brew
        public int RequiredAlchemistLevel { get; set; }

        // Ingredients consumed per brew
        public Dictionary<string, int> IngredientRequirements { get; set; } = new();

        // XP granted when brewed
        public int AlchemyXpReward { get; set; }

        // Potion type — determines which buff slot it uses
        // "stat" = ActiveStatBuff slot
        // "utility" = ActiveUtilityBuff slot
        // "instant" = no slot, fires immediately
        public string PotionType { get; set; }

        // Duration in minutes (0 for instant potions)
        public int DurationMinutes { get; set; }

        // Effect identifier — used by AlchemyBuffService to apply the correct effect
        public string EffectId { get; set; }

        // Effect value — percentage or flat amount depending on EffectId
        public double EffectValue { get; set; }

        // Secondary effect (optional — for potions with two effects like Berserker Brew)
        public string? SecondaryEffectId { get; set; }
        public double SecondaryEffectValue { get; set; }

        // Human readable description shown to player
        public string Description { get; set; }
    }
}