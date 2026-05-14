using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public static class RaidKeyItems
    {
        public static readonly ItemDefinition LairKey = new()
        {
            Id = "raid_key_t1",
            Name = "Scorched Lair Key",
            Icon = "🗝️",
            Type = "Key",
            Description = "A key forged from T1 hunt materials. Required to enter a Tier 1 Raid.",
            Tier = 1,
            SubCategory = "Raid"
        };

        public static readonly ItemDefinition StrongholdKey = new()
        {
            Id = "raid_key_t2",
            Name = "Stronghold Key",
            Icon = "🗝️",
            Type = "Key",
            Description = "A key forged from T2 hunt materials. Required to enter a Tier 2 Raid.",
            Tier = 2,
            SubCategory = "Raid"
        };

        public static readonly ItemDefinition FortressKey = new()
        {
            Id = "raid_key_t3",
            Name = "Fortress Key",
            Icon = "🗝️",
            Type = "Key",
            Description = "A key forged from T3 hunt materials. Required to enter a Tier 3 Raid.",
            Tier = 3,
            SubCategory = "Raid"
        };

        public static readonly ItemDefinition CitadelKey = new()
        {
            Id = "raid_key_t4",
            Name = "Citadel Key",
            Icon = "🗝️",
            Type = "Key",
            Description = "A key forged from T4 hunt materials. Required to enter a Tier 4 Raid.",
            Tier = 4,
            SubCategory = "Raid"
        };

        public static readonly ItemDefinition WorldBossKey = new()
        {
            Id = "raid_key_t5",
            Name = "World Boss Key",
            Icon = "🗝️",
            Type = "Key",
            Description = "A key forged from T5 hunt materials. Required to enter a Tier 5 Raid.",
            Tier = 5,
            SubCategory = "Raid"
        };
    }
}