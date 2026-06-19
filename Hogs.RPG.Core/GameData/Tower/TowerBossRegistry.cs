namespace Hogs.RPG.Core.GameData.Tower
{
    public class TowerBossDefinition
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public float HpMultiplier { get; set; } = 3f;
        public float AtkMultiplier { get; set; } = 2f;
        public string SpecialMechanicText { get; set; } = "";
    }

    public static class TowerBossRegistry
    {
        public static readonly List<TowerBossDefinition> All = new()
        {
            new()
            {
                Name = "The Gatekeeper",
                Description = "An ancient construct of stone and shadow, bound to guard the tower's depths for eternity.",
                ImageUrl = "https://placeholder.com/gatekeeper.png",
                HpMultiplier = 3f,
                AtkMultiplier = 1.8f,
                SpecialMechanicText = "💥 **Slam!** The Gatekeeper's fist shakes the entire floor — dealing 150% damage."
            },
            new()
            {
                Name = "Void Wraith",
                Description = "A twisted specter that feeds on the despair of climbers who made it this far.",
                ImageUrl = "https://placeholder.com/void_wraith.png",
                HpMultiplier = 3.5f,
                AtkMultiplier = 2f,
                SpecialMechanicText = "👁️ **Void Gaze!** Inflicts Weakened on all players for 3 floors."
            },
            new()
            {
                Name = "The Iron Tyrant",
                Description = "A warlord sealed inside cursed armor. His rage only grows with each floor climbed above him.",
                ImageUrl = "https://placeholder.com/iron_tyrant.png",
                HpMultiplier = 4f,
                AtkMultiplier = 2.2f,
                SpecialMechanicText = "🔥 **Berserker Rage!** The Tyrant hits for 200% damage but takes 25% more damage in return."
            },
            new()
            {
                Name = "Serpent Queen Vasara",
                Description = "A venomous ruler who has commanded the tower's mid-floors for three centuries.",
                ImageUrl = "https://placeholder.com/serpent_queen.png",
                HpMultiplier = 4.5f,
                AtkMultiplier = 2.4f,
                SpecialMechanicText = "☠️ **Venom Spit!** Inflicts Bleeding on all players for the rest of the run."
            },
            new()
            {
                Name = "The Doom Sovereign",
                Description = "The tower's true master. Those who reach this floor have seen things no living being should.",
                ImageUrl = "https://placeholder.com/doom_sovereign.png",
                HpMultiplier = 5f,
                AtkMultiplier = 2.8f,
                SpecialMechanicText = "💀 **Doom Proclamation!** Strips one random buff from each player."
            }
        };

        public static TowerBossDefinition GetForFloor(int floor)
        {
            int index = ((floor / 25) - 1) % All.Count;
            return All[Math.Max(0, index)];
        }
    }
}
