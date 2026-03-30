using Hogs.RPG.Core.Entities;
using static Hogs.RPG.Core.Entities.BossDefinition;

namespace Hogs.RPG.Core.GameData.GlobalBosses
{
    public static class AllBosses
    {
        public static readonly BossDefinition Aurelius = new()
        {
            Id = "aurelius_01",
            Name = "Aurelius",
            Description = "The Dream Eater — A dragon that invades dreams and distorts reality. Morose his worst nightmare.",
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1482035486892490843/IMG_4168.jpg",
            MaxHealth = 200000,
            Defense = 18,
            RewardGold = 750,
            Type = BossType.Daily,
            AbilitiesText = "Dream Eater; Dream Shield; Buldak Gae Power"
        };

        public static readonly BossDefinition Gravelmaw = new()
        {
            Id = "gravelmaw_02",
            Name = "Gravelmaw",
            Description = "Gravelmaw, the Bonewheel Tyrant — A monstrous beast twisted by dark rituals, bound to a cursed wheel of bone and roots. The cavern echoes with creaking wood and rattling skulls.",
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1482229938273914942/457b86b8-e3f0-4074-869a-f1fbffc5d99c.png",
            MaxHealth = 175000,
            Defense = 50,
            RewardGold = 750,
            Type = BossType.Daily,
            AbilitiesText = "Bonewheel Rampage; Graveclaw Slam; Skullstorm; Tireless Beast"
        };

        public static readonly BossDefinition PrimordialSerpent = new()
        {
            Id = "primordial_serpent_03",
            Name = "Primordial Serpent",
            Description = "The Devourer of Realms — An ancient serpent born from primordial chaos, corrupting the battlefield with venom and decay.",
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1482194388749910107/unnamed_37.jpg",
            MaxHealth = 150000,
            Defense = 35,
            RewardGold = 750,
            Type = BossType.Daily,
            AbilitiesText = "World Corrosion; Abyssal Constriction; Gaze of the End; Venom Spray"
        };

        public static readonly BossDefinition Xerathul = new()
        {
            Id = "xerathul_04",
            Name = "Xerathul",
            Description = "Devourer of Souls — A being born from a failed ritual, consuming souls to grow stronger as the world collapses around him.",
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1482119161080578250/file_00000000866871f49e808aa3caf46c18.png",
            MaxHealth = 180000,
            Defense = 20,
            RewardGold = 750,
            Type = BossType.Daily,
            AbilitiesText = "Abyss Core; Soul Storm; Chains of Damnation; Worldbreaker"
        };

        public static readonly BossDefinition TwoTierTyr = new()
        {
            Id = "two_tier_tyr_05",
            Name = "Two Tier Tyr",
            Description = "Fallen Asgard — A self-proclaimed reformer of Valhalla who introduced a \"fair\" system where only he benefited. Exiled after angering Thor, he now returns daily to enforce his absurd rules with divine authority.",
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486469816444518411/image.png",
            MaxHealth = 250000,
            Defense = 40,
            RewardGold = 750,
            Type = BossType.Daily,
            AbilitiesText = "Bifrost Regulation; Selective Justice; Administrative Shield; Summon Compliance Officers"
        };

        public static readonly BossDefinition KingThorlak = new()
        {
            Id = "king_thorlak_06",
            Name = "King Thorlak",
            Description = "Bloodbreaker — Once a legendary Viking Jarl, Thorlak sought glory beyond death and made a forbidden pact with ancient war spirits. Now risen as an undead draugr king, he grows stronger with every drop of blood spilled in battle.",
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486376651250995272/image.png",
            MaxHealth = 300000,
            Defense = 45,
            RewardGold = 750,
            Type = BossType.Daily,
            AbilitiesText = "Blood Feast; Draugr Awakening; Frostgrave Dominion; Curse of the Blood Rune"
        };

        public static readonly BossDefinition ClickPunisher = new()
        {
            Id = "click_punisher_07",
            Name = "Click the Punisher",
            Description = "The Collector of Debt — A cursed enforcer who punishes those who borrowed time and never paid it back. He marks his victims for collection, draining their life and crushing their hope with relentless penalties.",
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486376825969053726/image.png",
            MaxHealth = 220000,
            Defense = 30,
            RewardGold = 750,
            Type = BossType.Daily,
            AbilitiesText = "Penalty for Not Paying; Collection Notice; Late Fee; Foreclosure of Hope"
        };

        public static readonly BossDefinition GullveigHuld = new()
        {
            Id = "gullveig_huld_08",
            Name = "Gullveig-Huld",
            Description = "The Three-Times Burned — A witch of golden flame burned by the gods and reborn each time stronger. Master of Seiðr magic, she weaves fate itself, stealing power from the greedy and returning from death in blazing vengeance.",
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486376502571307219/image.png",
            MaxHealth = 260000,
            Defense = 28,
            RewardGold = 750,
            Type = BossType.Daily,
            AbilitiesText = "Golden Curse; Fate-Weaver’s Knot; Rebirth of Flame; Seiðr Dominion"
        };
    }
}