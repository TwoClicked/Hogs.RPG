using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public static class HuntDropItems
    {
        // ===== Forest Drops =====

        public static readonly ItemDefinition Fur = new()
        {
            Id = "fur",
            Name = "Fur",
            Icon = "🐺",
            Type = "Material",
            Description = "Soft fur from hunted animals."
        };

        public static readonly ItemDefinition Leather = new()
        {
            Id = "leather",
            Name = "Leather",
            Icon = "🦬",
            Type = "Material",
            Description = "Tough leather used for crafting."
        };

        public static readonly ItemDefinition Bone = new()
        {
            Id = "bone",
            Name = "Bone",
            Icon = "🦴",
            Type = "Material",
            Description = "A sturdy bone from small animals."
        };

        public static readonly ItemDefinition Feather = new()
        {
            Id = "feather",
            Name = "Feather",
            Icon = "🪶",
            Type = "Material",
            Description = "A light feather from birds."
        };

        public static readonly ItemDefinition Claws = new()
        {
            Id = "claws",
            Name = "Claws",
            Icon = "🐾",
            Type = "Material",
            Description = "Sharp claws taken from predators."
        };

        public static readonly ItemDefinition MonsterBlood = new()
        {
            Id = "monster_blood",
            Name = "Monster Blood",
            Icon = "🩸",
            Type = "Material",
            Description = "Blood harvested from dangerous beasts."
        };

        // ===== Wild Drops =====

        public static readonly ItemDefinition Hide = new()
        {
            Id = "hide",
            Name = "Hide",
            Icon = "🐻",
            Type = "Material",
            Description = "A tough animal hide."
        };

        public static readonly ItemDefinition Fang = new()
        {
            Id = "fang",
            Name = "Fang",
            Icon = "🦷",
            Type = "Material",
            Description = "A sharp fang from a predator."
        };

        public static readonly ItemDefinition Talon = new()
        {
            Id = "talon",
            Name = "Talon",
            Icon = "🦅",
            Type = "Material",
            Description = "A deadly bird talon."
        };

        public static readonly ItemDefinition Horn = new()
        {
            Id = "horn",
            Name = "Horn",
            Icon = "🐂",
            Type = "Material",
            Description = "A large animal horn."
        };

        public static readonly ItemDefinition SharpClaw = new()
        {
            Id = "sharp_claw",
            Name = "Sharp Claw",
            Icon = "🐆",
            Type = "Material",
            Description = "A razor sharp claw from a lynx."
        };

        // ===== Deep Area Drops =====

        public static readonly ItemDefinition SaberFang = new()
        {
            Id = "saber_fang",
            Name = "Saber Fang",
            Icon = "🦷",
            Type = "Material",
            Description = "A massive fang from a sabertooth."
        };

        public static readonly ItemDefinition GriffinFeather = new()
        {
            Id = "griffin_feather",
            Name = "Griffin Feather",
            Icon = "🪶",
            Type = "Material",
            Description = "A powerful feather from a griffin."
        };

        public static readonly ItemDefinition GiantAntler = new()
        {
            Id = "giant_antler",
            Name = "Giant Antler",
            Icon = "🦌",
            Type = "Material",
            Description = "A massive antler from a war elk."
        };

        public static readonly ItemDefinition ThickHide = new()
        {
            Id = "thick_hide",
            Name = "Thick Hide",
            Icon = "🐻",
            Type = "Material",
            Description = "Extremely thick hide from a dire bear."
        };

        public static readonly ItemDefinition DarkFeather = new()
        {
            Id = "dark_feather",
            Name = "Dark Feather",
            Icon = "🐦‍⬛",
            Type = "Material",
            Description = "A feather infused with dark energy."
        };

        // ===== Storm Area Drops =====

        public static readonly ItemDefinition StormFeather = new()
        {
            Id = "storm_feather",
            Name = "Storm Feather",
            Icon = "⚡",
            Type = "Material",
            Description = "A feather charged with storm energy."
        };

        public static readonly ItemDefinition AncientHide = new()
        {
            Id = "ancient_hide",
            Name = "Ancient Hide",
            Icon = "🐻",
            Type = "Material",
            Description = "An ancient hide from an old beast."
        };

        public static readonly ItemDefinition ShadowClaw = new()
        {
            Id = "shadow_claw",
            Name = "Shadow Claw",
            Icon = "🖤",
            Type = "Material",
            Description = "A claw infused with shadow magic."
        };

        public static readonly ItemDefinition TitanAntler = new()
        {
            Id = "titan_antler",
            Name = "Titan Antler",
            Icon = "🦌",
            Type = "Material",
            Description = "An enormous antler from a titan elk."
        };

        public static readonly ItemDefinition VoidFeather = new()
        {
            Id = "void_feather",
            Name = "Void Feather",
            Icon = "🌌",
            Type = "Material",
            Description = "A feather touched by the void."
        };

        // ===== Mythic Area Drops =====

        public static readonly ItemDefinition MythicHide = new()
        {
            Id = "mythic_hide",
            Name = "Mythic Hide",
            Icon = "💎",
            Type = "Material",
            Description = "A hide from a mythic creature."
        };

        public static readonly ItemDefinition SkyTalon = new()
        {
            Id = "sky_talon",
            Name = "Sky Talon",
            Icon = "☁️",
            Type = "Material",
            Description = "A talon from a sky tyrant."
        };

        public static readonly ItemDefinition AbyssClaw = new()
        {
            Id = "abyss_claw",
            Name = "Abyss Claw",
            Icon = "🌑",
            Type = "Material",
            Description = "A claw pulled from the abyss."
        };

        public static readonly ItemDefinition ColossusAntler = new()
        {
            Id = "colossus_antler",
            Name = "Colossus Antler",
            Icon = "🏔️",
            Type = "Material",
            Description = "An antler from a colossus elk."
        };

        public static readonly ItemDefinition DeathFeather = new()
        {
            Id = "death_feather",
            Name = "Death Feather",
            Icon = "☠️",
            Type = "Material",
            Description = "A feather from the death raven."
        };
    }
}