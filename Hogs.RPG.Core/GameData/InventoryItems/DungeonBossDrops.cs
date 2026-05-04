using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public class DungeonBossDrops
    {
        // =========================
        // LEVEL 5 - ROT FATHER MALCHOR (NEW)
        // =========================
        public static readonly ItemDefinition MalchorGripsItem = new()
        {
            Id = "malchor_grips",
            Name = "Malchor's Blightweave Grips",
            Icon = " <:MalchorsGrips:1500819542014689320>",
            Type = "Equipment",
            Description = "Gloves woven from the rotten bark of cursed trees. Still warm."
        };

        // =========================
        // LEVEL 10 - FANCULO
        // =========================
        public static readonly ItemDefinition FanculoHelmItem = new()
        {
            Id = "fanculo_helm",
            Name = "Fanculo Horned Helm",
            Icon = "<:fanculo_helm:1485380941416497294>",
            Type = "Equipment",
            Description = "A battle-worn helm infused with chaotic Viking energy."
        };

        // =========================
        // LEVEL 15 - HROTHGAR
        // =========================
        public static readonly ItemDefinition HrothgarRingItem = new()
        {
            Id = "hrothgar_ring",
            Name = "Hrothgar's Frostbound Ring",
            Icon = "<:hrothgars_ring:1486419394920976475>",
            Type = "Equipment",
            Description = "A frozen band that pulses with the lifesteal power of the Frost-Bound Jarl."
        };

        // =========================
        // LEVEL 17 - AURELION THE OATHBREAKER PALADIN
        // =========================
        public static readonly ItemDefinition OathcrushLegguardsItem = new()
        {
            Id = "oathcrush_legguards",
            Name = "Oathcrush Legguards",
            Icon = "<:OathcrushLegguards:1500820622328991897> ",
            Type = "Equipment",
            Description = "Leg armor torn from a fallen paladin. The divine sigils are shattered — but still warm."
        };

        // =========================
        // LEVEL 18 - AMPHIVOS TATEROUS
        // =========================
        public static readonly ItemDefinition TaterousBattleaxeItem = new()
        {
            Id = "taterous_battleaxe",
            Name = "Taterous Battleaxe",
            Icon = "<:Taterous_Battleaxe:1500817893410213938>",
            Type = "Equipment",
            Description = "Forged from enchanted root and starchy earth. Somehow both devastating and ridiculous."
        };

        // =========================
        // LEVEL 20 - LUMINARA
        // =========================
        public static readonly ItemDefinition LuminaraAmuletItem = new()
        {
            Id = "luminara_amulet",
            Name = "Luminara's Moonlit Amulet",
            Icon = "<:luminaras_amulet:1486423884688396289>",
            Type = "Equipment",
            Description = "A glowing charm radiating deceptive beauty, masking a sinister, protective aura."
        };

        // =========================
        // LEVEL 22 - SKARR THE CLOWN OF CARNAGE
        // =========================
        public static readonly ItemDefinition SkarrSawbladeshieldItem = new()
        {
            Id = "skarr_sawbladeshield",
            Name = "Skarr's Sawblade Shield",
            Icon = "<:SkarrsSawbladeShield:1500820862394433627>",
            Type = "Equipment",
            Description = "A spinning shield of serrated blades. The laughter painted on it is not comforting."
        };

        // =========================
        // LEVEL 23 - GRIMBLEX THE GILDED TYRANT
        // =========================
        public static readonly ItemDefinition ShadowsaphireSignetItem = new()
        {
            Id = "shadowsaphire_signet",
            Name = "Shadowsaphire Signet",
            Icon = "<:ShadowsaphireSignet:1500821445968920699>",
            Type = "Equipment",
            Description = "A ring set with a gem that absorbs light. Grimblex hoarded dozens — this one is yours now."
        };

        // =========================
        // LEVEL 25 - THORKELL
        // =========================
        public static readonly ItemDefinition ThorkellBootsItem = new()
        {
            Id = "thorkell_boots",
            Name = "Boots of the Warborn",
            Icon = "<:thorkell_boots:1486426976297418852>",
            Type = "Equipment",
            Description = "Heavy war boots infused with divine fury, allowing the wearer to crush through all resistance."
        };

        // =========================
        // LEVEL 27 - GRITCH THE GILDED PROWLER
        // =========================
        public static readonly ItemDefinition GritchWarplateItem = new()
        {
            Id = "gritch_warplate",
            Name = "Gritch's Star-Iron Warplate",
            Icon = "<:Gritchs_StarIron_Warplate:1500821752815685772> ",
            Type = "Equipment",
            Description = "Armor hammered from Star-Iron ore. It hums faintly. You don't want to know why."
        };
    }
}
