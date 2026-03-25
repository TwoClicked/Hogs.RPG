using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public static class DailyWeeklyBossDrops
    {
        // =========================
        // DAILY BOSSES
        // =========================

        public static readonly ItemDefinition GravelmawShieldItem = new()
        {
            Id = "gravelmaw_shield",
            Name = "Gravelmaw Bulwark",
            Icon = "<:gravelmaw_shield:1485349266133094450>",
            Type = "Equipment",
            Description = "A massive shield carved from the petrified hide of Gravelmaw."
        };

        public static readonly ItemDefinition SerpentGlovesItem = new()
        {
            Id = "serpent_gloves",
            Name = "Serpent Fang Gloves",
            Icon = "<:primordial_serpent_gloves:1485351574493728828>",
            Type = "Equipment",
            Description = "Venom-infused gloves that pulse with serpentine energy."
        };

        public static readonly ItemDefinition XerathulArmorItem = new()
        {
            Id = "xerathul_armor",
            Name = "Xerathul Abyss Plate",
            Icon = "<:xerathul_armour:1486416595633963209>",
            Type = "Equipment",
            Description = "Armor forged in abyssal flames, pulsing with dark power."
        };

        public static readonly ItemDefinition PunisherRingItem = new()
        {
            Id = "punisher_ring",
            Name = "Seal of Collection",
            Icon = "<:click_the_punisher_ring:1486431538030575826>",
            Type = "Equipment",
            Description = "A cursed ring that binds its wearer to an endless cycle of debt and power."
        };

        public static readonly ItemDefinition AureliusSwordItem = new()
        {
            Id = "aurelius_sword",
            Name = "Blade of Aurelius",
            Icon = "<:aurelius_sword:1485346163107303610>",
            Type = "Equipment",
            Description = "A legendary blade once wielded by the celestial warlord Aurelius."
        };

        public static readonly ItemDefinition TyrHelmItem = new()
        {
            Id = "tyr_helm",
            Name = "Helm of the High Overseer",
            Icon = "<:two_tier_tyr_helm:1486433644318031952>",
            Type = "Equipment",
            Description = "A golden helm symbolizing authority, hierarchy, and questionable leadership decisions."
        };

        public static readonly ItemDefinition ThrolakLeggingsItem = new()
        {
            Id = "thorlak_leggings",
            Name = "Bloodbreaker Warleggings",
            Icon = "<:king_thorlak_boots:1486438222698516540>",
            Type = "Equipment",
            Description = "Leggings worn by the undead king, stained with the blood of countless battles."
        };

        public static readonly ItemDefinition GullveigAmuletItem = new()
        {
            Id = "gullveig_amulet",
            Name = "Gullveig-Touched Amulet",
            Icon = "<:gullveig_huld_amulet:1486441541672243335>",
            Type = "Equipment",
            Description = "An ancient amulet infused with forbidden Seiðr magic, bending fate itself."
        };
    }
}