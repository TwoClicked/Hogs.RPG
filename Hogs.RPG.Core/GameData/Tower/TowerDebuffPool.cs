using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Tower
{
    public class TowerDebuffDefinition
    {
        public TowerDebuffType Type { get; set; }
        public string Name { get; set; } = "";
        public string Emoji { get; set; } = "";
        public string Description { get; set; } = "";
        public int DefaultDuration { get; set; } = -1;
    }

    public static class TowerDebuffPool
    {
        public static readonly List<TowerDebuffDefinition> All = new()
        {
            new() { Type = TowerDebuffType.Bleeding,  Emoji = "🩸", Name = "Bleeding",  Description = "Lose 5% max HP at the start of each floor",         DefaultDuration = -1 },
            new() { Type = TowerDebuffType.Weakened,  Emoji = "😵", Name = "Weakened",  Description = "Deal 20% less damage",                               DefaultDuration =  3 },
            new() { Type = TowerDebuffType.Shackled,  Emoji = "🔗", Name = "Shackled",  Description = "Your next Rest is automatically skipped",              DefaultDuration = -1 },
            new() { Type = TowerDebuffType.Cursed,    Emoji = "👁️", Name = "Cursed",    Description = "One random buff is disabled for 3 floors",            DefaultDuration =  3 },
        };

        public static TowerDebuffDefinition Get(TowerDebuffType type) =>
            All.First(d => d.Type == type);
    }
}
