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
            Description = "An ancient amulet radiating venomous energy."
        };

        public static readonly ItemDefinition XerathulArmorItem = new()
        {
            Id = "xerathul_armor",
            Name = "Xerathul Abyss Plate",
            Icon = "<:xerathul_boots:1485378351479652480>",
            Type = "Equipment",
            Description = "Armor forged in abyssal flames, pulsing with dark power."
        };

        // =========================
        // WEEKLY BOSSES
        // =========================

        public static readonly ItemDefinition AureliusSwordItem = new()
        {
            Id = "aurelius_sword",
            Name = "Blade of Aurelius",
            Icon = "<:aurelius_sword:1485346163107303610>",
            Type = "Equipment",
            Description = "A legendary blade once wielded by the celestial warlord Aurelius."
        };
    }
}