namespace Hogs.RPG.Core.GameData.Tower
{
    public class SigilDefinition
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Emoji { get; set; } = "";
        public string BonusPerStack { get; set; } = "";
        public string LoreText { get; set; } = "";
    }

    public static class SigilRegistry
    {
        public const int MaxStacks = 3;
        public const int CompensationGold = 250;

        public static readonly List<SigilDefinition> All = new()
        {
            new() { Id = "sigil_slaughter", Emoji = "🗡️", Name = "Sigil of Slaughter", BonusPerStack = "+2% damage",              LoreText = "Carved from the blade of a fallen champion." },
            new() { Id = "sigil_fortitude", Emoji = "🛡️", Name = "Sigil of Fortitude", BonusPerStack = "+2% defense",             LoreText = "A shard of the Gatekeeper's original seal." },
            new() { Id = "sigil_vitality",  Emoji = "❤️", Name = "Sigil of Vitality",  BonusPerStack = "+2% max HP",              LoreText = "Pulses with a faint heartbeat — it is not yours." },
            new() { Id = "sigil_leech",     Emoji = "🩸", Name = "Sigil of the Leech", BonusPerStack = "+1% lifesteal",           LoreText = "It drinks deep from those who carry it." },
            new() { Id = "sigil_greed",     Emoji = "💰", Name = "Sigil of Greed",     BonusPerStack = "+2% gold from all sources", LoreText = "Worth more than it looks. Always." },
            new() { Id = "sigil_wisdom",    Emoji = "⭐", Name = "Sigil of Wisdom",    BonusPerStack = "+2% XP from all sources", LoreText = "The tower teaches those who survive it." },
        };

        public static SigilDefinition? Get(string id) =>
            All.FirstOrDefault(s => s.Id == id);

        public static SigilDefinition Random(Random rng) =>
            All[rng.Next(All.Count)];
    }
}
