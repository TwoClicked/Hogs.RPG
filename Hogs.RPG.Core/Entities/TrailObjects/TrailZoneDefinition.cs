namespace Hogs.RPG.Core.Entities.TrailObjects
{
    public class TrailZoneDefinition
    {
        // Unique identifier for this zone
        public string Id { get; set; }

        // Display name shown to the player
        public string Name { get; set; }

        // Flavour description shown at trail start
        public string Description { get; set; }

        // Icon shown in the Discord embed
        public string Icon { get; set; }

        // Minimum player level required to run this zone
        public int RequiredLevel { get; set; } = 1;

        // Base token yield range before event modifiers are applied
        public int MinTokens { get; set; }
        public int MaxTokens { get; set; }

        // The pool of events this zone can roll from.
        // Each entry is a TrailEventDefinition Id paired with its spawn weight.
        public Dictionary<string, int> EventPool { get; set; } = new();
    }
}