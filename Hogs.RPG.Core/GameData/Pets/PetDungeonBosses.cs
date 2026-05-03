using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.DungeonBosses
{
    public static class PetDungeonBosses
    {
        // =========================
        // ⚔️ ATTACK PET BOSS
        // Blazewing's Gorge — Lv 15
        // =========================
        public static readonly DungeonBossDefinition Blazewing = new()
        {
            Id = "blazewing",
            Name = "Blazewing, the Scorched Alpha",
            Description = "A colossal raptor wreathed in fire, the apex predator of the gorge. Nothing escapes its dive.",

            MaxHealth = 1500,
            Attack = 175,
            Defense = 25,

            ImageUrl = "",

            BehaviorId = "rage",
            AbilitiesText = "Enters rage at low HP, doubling damage temporarily.",

            PetDrops = new List<PetDrop>
            {
                new PetDrop { PetId = "blazefang_raptor", ChancePercent = 3 }
            }
        };

        // =========================
        // 🛡️ DEFENSE PET BOSS
        // Stonehall Depths — Lv 20
        // =========================
        public static readonly DungeonBossDefinition Stonelord = new()
        {
            Id = "stonelord",
            Name = "Stonelord Kragul",
            Description = "An ancient stone colossus sealed beneath Stonehall, awakened by tremors and thirst for destruction.",

            MaxHealth = 2300,
            Attack = 220,
            Defense = 55,

            ImageUrl = "",

            BehaviorId = "crushing_blow",
            AbilitiesText = "Occasionally unleashes a devastating blow ignoring part of the player's defense.",

            PetDrops = new List<PetDrop>
            {
                new PetDrop { PetId = "ironshell_boar", ChancePercent = 3 }
            }
        };

        // =========================
        // ❤️ HEALTH PET BOSS
        // Drowned Archives — Lv 25
        // =========================
        public static readonly DungeonBossDefinition Tidewarden = new()
        {
            Id = "tidewarden",
            Name = "Tidewarden Marath",
            Description = "A serpentine leviathan that guards the flooded ruins of an ancient library. Feeds on the drowned.",

            MaxHealth = 3400,
            Attack = 295,
            Defense = 58,

            ImageUrl = "",

            BehaviorId = "lifesteal_smash",
            AbilitiesText = "Unleashes a heavy blow, restoring health equal to damage dealt.",

            PetDrops = new List<PetDrop>
            {
                new PetDrop { PetId = "tidecall_serpent", ChancePercent = 3 }
            }
        };
    }
}