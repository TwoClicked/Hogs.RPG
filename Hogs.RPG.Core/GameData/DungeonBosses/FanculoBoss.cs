using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.DungeonBosses
{
    public static class FanculoBoss
    {
        public static readonly DungeonBossDefinition Fanculo = new()
        {
            Id = "fanculo",
            Name = "Fanculo the Wandering Viking",
            Description = "A chaotic Viking riding a divine scooter, radiating raw strength. screaming FANCULO",

            MaxHealth = 950,
            Attack = 42,
            Defense = 18,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1482049411365470400/B2BA700C-F9CE-4B4B-8847-44D4EE18825C.png?ex=69bd7292&is=69bc2112&hm=d461b2ff15efb092afd84048e83339051c4b96b2ca5c1ee0138cbdca3932401c",

            AbilitiesText = "Enters rage at low HP, doubling damage temporarily."
        };
    }
}