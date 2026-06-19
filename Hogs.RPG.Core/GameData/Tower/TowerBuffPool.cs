using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Tower
{
    public class TowerBuffDefinition
    {
        public TowerBuffType Type { get; set; }
        public string Name { get; set; } = "";
        public string Emoji { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public static class TowerBuffPool
    {
        public static readonly List<TowerBuffDefinition> All = new()
        {
            new() { Type = TowerBuffType.Bloodthirst,  Emoji = "🩸", Name = "Bloodthirst",   Description = "Lifesteal 10% of damage dealt per stack" },
            new() { Type = TowerBuffType.Executioner,  Emoji = "💀", Name = "Executioner",   Description = "+25% damage on elite and boss floors per stack" },
            new() { Type = TowerBuffType.DoubleStrike, Emoji = "⚡", Name = "Double Strike", Description = "20% chance to hit twice (stacks add 10% each)" },
            new() { Type = TowerBuffType.IronSkin,     Emoji = "🛡️", Name = "Iron Skin",     Description = "Reduce incoming damage by 15% per stack" },
            new() { Type = TowerBuffType.Thorns,       Emoji = "🌵", Name = "Thorns",        Description = "Reflect 15% of damage taken back per stack" },
            new() { Type = TowerBuffType.Precision,    Emoji = "🎯", Name = "Precision",     Description = "Ignore 33% of enemy defense per stack" },
            new() { Type = TowerBuffType.Evasion,      Emoji = "💨", Name = "Evasion",       Description = "15% dodge chance per stack (max 45%)" },
            new() { Type = TowerBuffType.Frenzy,       Emoji = "🔥", Name = "Frenzy",        Description = "+5% damage per consecutive floor cleared without taking a hit" },
        };

        public static TowerBuffDefinition Get(TowerBuffType type) =>
            All.First(b => b.Type == type);
    }
}
