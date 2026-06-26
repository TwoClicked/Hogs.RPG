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
                SpecialMechanicText = "👁️ **Void Gaze!** Inflicts Weakened on all players until cleansed."
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
                SpecialMechanicText = "💀 **Doom Proclamation!** Steals 2 buff stacks from each player."
            },
            new()
            {
                Name = "Wussy The Drowned Warden",
                Description = "A being of fractured will, said to strip the strength from any who oppose it.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1519072933530566778/IMG_3647.png?ex=6a3c3a6a&is=6a3ae8ea&hm=8d7ca1e1e671fe84761992dd9fae5319a25037c2e479f8fdb19f3aa41f2d12e3",
                HpMultiplier = 5.5f,
                AtkMultiplier = 3.75f,
                SpecialMechanicText = "🌀 **Unraveling Grasp!** Steals 4 buff stacks from every player on defeat."
            },
            new()
            {
                Name = "Azrael The Angel of Death",
                Description = "Its presence alone saps the resolve of the bravest climbers.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1518700263773311197/image0.jpg?ex=6a3c30d6&is=6a3adf56&hm=581600c9bc94ed6d4f61da4ded12c621f12bf7acf9a83f25325eaa2f9763100d",
                HpMultiplier = 6f,
                AtkMultiplier = 4f,
                SpecialMechanicText = "😵 **Crushing Despair!** Steals 6 buff stacks and inflicts Weakened x3 on every player."
            },
            new()
            {
                Name = "The Faceless Terror",
                Description = "A rotting horror that leaves nothing but ruin in its wake.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1510773840660271265/1780266346003.png?ex=6a3bb308&is=6a3a6188&hm=49374d13fe19f6ec02c2c58a6c00b89d34eb2ffdffa2b98376ae5a3c61000736",
                HpMultiplier = 6.5f,
                AtkMultiplier = 4.25f,
                SpecialMechanicText = "🩸 **Festering Wound!** Steals 8 buff stacks and inflicts permanent Bleeding and Weakened x2."
            },
            new()
            {
                Name = "Nyxaroth",
                Description = "It does not fight to wound — it fights to erase.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1504505688842436749/image0.png?ex=6a3bf79c&is=6a3aa61c&hm=a7ab9d5dda32e268377753124e2708a60701155e7c3539297397c13c2b76b0f9",
                HpMultiplier = 7f,
                AtkMultiplier = 4.5f,
                SpecialMechanicText = "💀 **Total Erasure!** Steals 10 buff stacks from every player."
            },
            new()
            {
                Name = "Lysandra",
                Description = "Wherever it walks, the air itself begins to rot.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1504501125145956544/image0.png?ex=6a3bf35c&is=6a3aa1dc&hm=a6c884f7524451535de1d4cd1a7d6a95165c10632e2f2f86ada3576397421da2",
                HpMultiplier = 7.5f,
                AtkMultiplier = 4.75f,
                SpecialMechanicText = "🩸 **Plague Bloom!** Inflicts permanent Bleeding on every player."
            },
            new()
            {
                Name = "Seraphiel",
                Description = "A predator that hunts strength itself, leaving its prey hollow.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1504502760475852911/image0.png?ex=6a3bf4e2&is=6a3aa362&hm=a4058dbdeb4e509de1c57471a2d7e237613fc63cff899b48e94d6447ad5f889c",
                HpMultiplier = 8f,
                AtkMultiplier = 5f,
                SpecialMechanicText = "🌀 **Hollowing Strike!** Steals 4 buff stacks and inflicts Weakened x2."
            },
            new()
            {
                Name = "Skjarr The Brew Cursed",
                Description = "Ancient and merciless, it remembers every climber who has fallen before.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1491147055052095498/5C6805BB-4015-430F-8009-3E63E31C9F3E.png?ex=6a3c25ea&is=6a3ad46a&hm=eae0bf001c0155c6832eb41c8f6bd566b9372871d2f6fd18a3c559e1a867d29c",
                HpMultiplier = 8.5f,
                AtkMultiplier = 5.25f,
                SpecialMechanicText = "💀 **Final Reckoning!** Steals 8 buff stacks and inflicts permanent Bleeding."
            },
            new()
            {
                Name = "Viscynia the Fallen Valkyrie",
                Description = "Its howl alone is enough to wither the strongest will.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1487173038515556413/f88ea60e-b6ce-44de-a563-a0232511bf56.jpg?ex=6a3c3152&is=6a3adfd2&hm=74ec03f0d7c9eb5ae8d759fe5962fec6e377c6c5510065e23e787c780d83bb62",
                HpMultiplier = 9f,
                AtkMultiplier = 5.5f,
                SpecialMechanicText = "🩸 **Withering Howl!** Inflicts permanent Bleeding and Weakened x3."
            },
            new()
            {
                Name = "The Veiled Thunderer",
                Description = "It feeds on confidence, leaving climbers bare before the dark.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486710664230731930/1774529681198.png?ex=6a3bd434&is=6a3a82b4&hm=d4ea7f365cd888ee28176c8daaee8634e2299019ad997b7c0c751c3768745970",
                HpMultiplier = 9.5f,
                AtkMultiplier = 5.75f,
                SpecialMechanicText = "💀 **Soul Strip!** Steals 8 buff stacks and inflicts Weakened x3."
            },
            new()
            {
                Name = "Hræzmir",
                Description = "The final terror of this stretch of the tower — nothing escapes it whole.",
                ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486694307158298767/file_00000000bf70720a9fd5ef91dd9325bd.png?ex=6a3bc4f8&is=6a3a7378&hm=cbb204293e82bba86689e33fb68b629c421921848e8d13d8ef2024491dbf1034",
                HpMultiplier = 10f,
                AtkMultiplier = 6f,
                SpecialMechanicText = "☠️ **Apex Curse!** Steals 10 buff stacks, inflicts permanent Bleeding, and Weakened x3."
            }
        };

        public static TowerBossDefinition GetForFloor(int floor)
        {
            int index = ((floor / 25) - 1) % All.Count;
            return All[Math.Max(0, index)];
        }
    }
}
