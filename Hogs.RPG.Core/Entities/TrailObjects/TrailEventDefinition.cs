public class TrailEventDefinition
{
    // Unique identifier for this event definition
    public string Id { get; set; }

    // Display name shown to the player
    public string Name { get; set; }

    // Flavour text shown when the event fires
    public string Description { get; set; }

    // Icon shown in the Discord embed
    public string Icon { get; set; }

    // Whether this event pauses for player input
    public TrailEventType Type { get; set; }

    // Base token modifier applied when this event resolves
    // Positive = bonus tokens, negative = token penalty
    // Applied as a flat amount on top of the trail's base yield
    public int TokenModifier { get; set; } = 0;

    // Weighting in the event pool — higher = more likely to appear
    public int Weight { get; set; } = 10;
}