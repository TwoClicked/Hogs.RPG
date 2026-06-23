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
                Name = "Zoom the Anti-Semestic",
                Description = "A twisted, Smeagol-like creature with a hunched body, pale skin, huge nervous eyes, and a creepy grin.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1517671542841016350/image.png?ex=6a372144&is=6a35cfc4&hm=f5a058086a7a465c8ffa7727ecaa7ddf905e536abd32185d23102f6f5e8ca8d0",
                HpMultiplier = 3f,
                AtkMultiplier = 2.25f,
                SpecialMechanicText = "💥 **Slam!** The Gatekeeper's fist shakes the entire floor — dealing 150% damage."
            },
            new()
            {
                Name = "Svea",
                Description = "The Midsummer Matriarch",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1517671773762490499/image.png?ex=6a37217b&is=6a35cffb&hm=3cc214056eb0ab9d463f395bef34fed998ab12fdd1dd42e75c8a7bb672940bc0",
                HpMultiplier = 3.5f,
                AtkMultiplier = 2.5f,
                SpecialMechanicText = "👁️ **Void Gaze!** Inflicts Weakened on all players for 3 floors."
            },
            new()
            {
                Name = "Mordrak",
                Description = "Fancy chap",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1517672135865270362/image.png?ex=6a3721d2&is=6a35d052&hm=a60d437f7d31a64f9b84078dfbd739d09968cdbcec875c978840095607aab210",
                HpMultiplier = 4f,
                AtkMultiplier = 2.75f,
                SpecialMechanicText = "🔥 **Berserker Rage!** The Tyrant hits for 200% damage but takes 25% more damage in return."
            },
            new()
            {
                Name = "Varkor, the Ashen King",
                Description = "Once a noble knight sworn to protect his kingdom, Varkor fell into darkness after forging a forbidden bond with the ancient dragon Nytherax.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1504498444230721618/image0.png?ex=6a36aadd&is=6a35595d&hm=c765963c52841ba76c9f97a5cfccdb81d52f4473b417abebf19aae0c49b18d0f",
                HpMultiplier = 4.5f,
                AtkMultiplier = 3f,
                SpecialMechanicText = "☠️ **Venom Spit!** Inflicts Bleeding on all players for the rest of the run."
            },
            new()
            {
                Name = "Azrakar, Herald of the Falling Star",
                Description = "Forged within the heart of a dying world, Azrakar is an ancient celestial destroyer born from fire",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1504510410861318296/image0.png?ex=6a36b602&is=6a356482&hm=24df6c513967dcef5e89dc44a10e1b1ccae09e05b36a1ece818efc5bddc99855",
                HpMultiplier = 5f,
                AtkMultiplier = 3.5f,
                SpecialMechanicText = "💀 **Doom Proclamation!** Strips one random buff from each player."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "A being of fractured will, said to strip the strength from any who oppose it.",
                ImageUrl = "",
                HpMultiplier = 5.5f,
                AtkMultiplier = 3.75f,
                SpecialMechanicText = "🌀 **Unraveling Grasp!** Tears two buffs from every player on defeat."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "Its presence alone saps the resolve of the bravest climbers.",
                ImageUrl = "",
                HpMultiplier = 6f,
                AtkMultiplier = 4f,
                SpecialMechanicText = "😵 **Crushing Despair!** Inflicts Weakened x3 on every player."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "A rotting horror that leaves nothing but ruin in its wake.",
                ImageUrl = "",
                HpMultiplier = 6.5f,
                AtkMultiplier = 4.25f,
                SpecialMechanicText = "🩸 **Festering Wound!** Inflicts permanent Bleeding and Weakened x2."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "It does not fight to wound — it fights to erase.",
                ImageUrl = "",
                HpMultiplier = 7f,
                AtkMultiplier = 4.5f,
                SpecialMechanicText = "💀 **Total Erasure!** Strips every buff from every player."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "Wherever it walks, the air itself begins to rot.",
                ImageUrl = "",
                HpMultiplier = 7.5f,
                AtkMultiplier = 4.75f,
                SpecialMechanicText = "🩸 **Plague Bloom!** Inflicts permanent Bleeding on every player."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "A predator that hunts strength itself, leaving its prey hollow.",
                ImageUrl = "",
                HpMultiplier = 8f,
                AtkMultiplier = 5f,
                SpecialMechanicText = "🌀 **Hollowing Strike!** Strips two buffs and inflicts Weakened x2."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "Ancient and merciless, it remembers every climber who has fallen before.",
                ImageUrl = "",
                HpMultiplier = 8.5f,
                AtkMultiplier = 5.25f,
                SpecialMechanicText = "💀 **Final Reckoning!** Strips every buff and inflicts permanent Bleeding."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "Its howl alone is enough to wither the strongest will.",
                ImageUrl = "",
                HpMultiplier = 9f,
                AtkMultiplier = 5.5f,
                SpecialMechanicText = "🩸 **Withering Howl!** Inflicts permanent Bleeding and Weakened x3."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "It feeds on confidence, leaving climbers bare before the dark.",
                ImageUrl = "",
                HpMultiplier = 9.5f,
                AtkMultiplier = 5.75f,
                SpecialMechanicText = "💀 **Soul Strip!** Strips every buff and inflicts Weakened x3."
            },
            new()
            {
                Name = "[Boss Name TBD]",
                Description = "The final terror of this stretch of the tower — nothing escapes it whole.",
                ImageUrl = "",
                HpMultiplier = 10f,
                AtkMultiplier = 6f,
                SpecialMechanicText = "☠️ **Apex Curse!** Strips every buff, inflicts permanent Bleeding, and Weakened x3."
            }
        };

        public static TowerBossDefinition GetForFloor(int floor)
        {
            int index = ((floor / 25) - 1) % All.Count;
            return All[Math.Max(0, index)];
        }
    }
}
