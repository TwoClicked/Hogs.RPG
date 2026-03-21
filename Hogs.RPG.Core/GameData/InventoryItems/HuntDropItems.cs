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
            Icon = "<:fur:1484966348819009606>",
            Type = "Material",
            Description = "Soft fur from hunted animals."
        };

        public static readonly ItemDefinition Leather = new()
        {
            Id = "leather",
            Name = "Leather",
            Icon = "<:leather:1484966747109851227>",
            Type = "Material",
            Description = "Tough leather used for crafting."
        };

        public static readonly ItemDefinition Bone = new()
        {
            Id = "bone",
            Name = "Bone",
            Icon = "<:bones:1485050831815311370>",
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
            Icon = "<:claws:1484968552988410040>",
            Type = "Material",
            Description = "Sharp claws taken from predators."
        };

        public static readonly ItemDefinition MonsterBlood = new()
        {
            Id = "monster_blood",
            Name = "Monster Blood",
            Icon = "<:monster_blood:1484968995332423912>",
            Type = "Material",
            Description = "Blood harvested from dangerous beasts."
        };

        // ===== Wild Drops =====

        public static readonly ItemDefinition Hide = new()
        {
            Id = "hide",
            Name = "Hide",
            Icon = "<:hide:1484969483184373853>",
            Type = "Material",
            Description = "A tough animal hide."
        };

        public static readonly ItemDefinition Fang = new()
        {
            Id = "fang",
            Name = "Fang",
            Icon = "<:fang:1484970438382850145>",
            Type = "Material",
            Description = "A sharp fang from a predator."
        };

        public static readonly ItemDefinition Talon = new()
        {
            Id = "talon",
            Name = "Talon",
            Icon = "<:talon:1484970821423464588>",
            Type = "Material",
            Description = "A deadly bird talon."
        };

        public static readonly ItemDefinition Horn = new()
        {
            Id = "horn",
            Name = "Horn",
            Icon = "<:horn:1484971315881316362>",
            Type = "Material",
            Description = "A large animal horn."
        };

        public static readonly ItemDefinition SharpClaw = new()
        {
            Id = "sharp_claw",
            Name = "Sharp Claw",
            Icon = "<:sharp_claw:1484972131556135113>",
            Type = "Material",
            Description = "A razor sharp claw from a lynx."
        };

        // ===== Deep Area Drops =====

        public static readonly ItemDefinition SaberFang = new()
        {
            Id = "saber_fang",
            Name = "Saber Fang",
            Icon = "<:saber_fang:1484972884173656114>",
            Type = "Material",
            Description = "A massive fang from a sabertooth."
        };

        public static readonly ItemDefinition GriffinFeather = new()
        {
            Id = "griffin_feather",
            Name = "Griffin Feather",
            Icon = "<:griffin_feather:1484973554486476855>",
            Type = "Material",
            Description = "A powerful feather from a griffin."
        };

        public static readonly ItemDefinition GiantAntler = new()
        {
            Id = "giant_antler",
            Name = "Giant Antler",
            Icon = "<:giant_antler:1484974328855396382>",
            Type = "Material",
            Description = "A massive antler from a war elk."
        };

        public static readonly ItemDefinition ThickHide = new()
        {
            Id = "thick_hide",
            Name = "Thick Hide",
            Icon = "<:thick_hide:1484976111103250634>",
            Type = "Material",
            Description = "Extremely thick hide from a dire bear."
        };

        public static readonly ItemDefinition DarkFeather = new()
        {
            Id = "dark_feather",
            Name = "Dark Feather",
            Icon = "<:dark_feather:1484982076523679916>",
            Type = "Material",
            Description = "A feather infused with dark energy."
        };

        // ===== Storm Area Drops =====

        public static readonly ItemDefinition StormFeather = new()
        {
            Id = "storm_feather",
            Name = "Storm Feather",
            Icon = "<:storm_feather:1484982544515465277>",
            Type = "Material",
            Description = "A feather charged with storm energy."
        };

        public static readonly ItemDefinition AncientHide = new()
        {
            Id = "ancient_hide",
            Name = "Ancient Hide",
            Icon = "<:ancient_hide:1484983033286098994>",
            Type = "Material",
            Description = "An ancient hide from an old beast."
        };

        public static readonly ItemDefinition ShadowClaw = new()
        {
            Id = "shadow_claw",
            Name = "Shadow Claw",
            Icon = "<:shadow_claw:1484983863750168586>",
            Type = "Material",
            Description = "A claw infused with shadow magic."
        };

        public static readonly ItemDefinition TitanAntler = new()
        {
            Id = "titan_antler",
            Name = "Titan Antler",
            Icon = "<:titan_antler:1484984210073587752>",
            Type = "Material",
            Description = "An enormous antler from a titan elk."
        };

        public static readonly ItemDefinition VoidFeather = new()
        {
            Id = "void_feather",
            Name = "Void Feather",
            Icon = "<:void_feather:1484984812971495524>",
            Type = "Material",
            Description = "A feather touched by the void."
        };

        // ===== Mythic Area Drops =====

        public static readonly ItemDefinition MythicHide = new()
        {
            Id = "mythic_hide",
            Name = "Mythic Hide",
            Icon = "<:mythic_hide:1484985375121473726>",
            Type = "Material",
            Description = "A hide from a mythic creature."
        };

        public static readonly ItemDefinition SkyTalon = new()
        {
            Id = "sky_talon",
            Name = "Sky Talon",
            Icon = "<:sky_talon:1484985926366003271>",
            Type = "Material",
            Description = "A talon from a sky tyrant."
        };

        public static readonly ItemDefinition AbyssClaw = new()
        {
            Id = "abyss_claw",
            Name = "Abyss Claw",
            Icon = "<:abyss_claw:1484986317850022034>",
            Type = "Material",
            Description = "A claw pulled from the abyss."
        };

        public static readonly ItemDefinition ColossusAntler = new()
        {
            Id = "colossus_antler",
            Name = "Colossus Antler",
            Icon = "<:colossus_antler:1484986845782610063>",
            Type = "Material",
            Description = "An antler from a colossus elk."
        };

        public static readonly ItemDefinition DeathFeather = new()
        {
            Id = "death_feather",
            Name = "Death Feather",
            Icon = "<:death_feather:1484987349564915732>",
            Type = "Material",
            Description = "A feather from the death raven."
        };
    }
}