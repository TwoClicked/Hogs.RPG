using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.EquipmentObjects;

namespace Hogs.RPG.Core.GameData.Alchemy
{
    public static class AlchemyPotionItems
    {
        public static readonly ItemDefinition WeakStaminaVial = new() { Id = "weak_stamina_vial", Name = "Weak Stamina Vial", Icon = "🧪", Type = "Potion", Description = "Instantly restores 20 Hunter Stamina.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition ApprenticesBrew = new() { Id = "apprentices_brew", Name = "Apprentice's Brew", Icon = "🧪", Type = "Potion", Description = "+10% XP gain for 1 hour.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition WeakHuntersDraft = new() { Id = "weak_hunters_draft", Name = "Weak Hunter's Draft", Icon = "🧪", Type = "Potion", Description = "+10% loot drop chance for 1 hour.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition StaminaVial = new() { Id = "stamina_vial", Name = "Stamina Vial", Icon = "🧪", Type = "Potion", Description = "Instantly restores 40 Hunter Stamina.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition TrailTonic = new() { Id = "trail_tonic", Name = "Trail Tonic", Icon = "🧪", Type = "Potion", Description = "Grants +1 extra trail run today.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition HuntersDraft = new() { Id = "hunters_draft", Name = "Hunter's Draft", Icon = "🧪", Type = "Potion", Description = "+15% loot drop chance for 2 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition XpSerum = new() { Id = "xp_serum", Name = "XP Serum", Icon = "🧪", Type = "Potion", Description = "+20% XP gain for 2 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition WeakBerserkerBrew = new() { Id = "weak_berserker_brew", Name = "Weak Berserker Brew", Icon = "🧪", Type = "Potion", Description = "+15% ATK, -10% DEF for 1 hour.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition WeakIronbloodTonic = new() { Id = "weak_ironblood_tonic", Name = "Weak Ironblood Tonic", Icon = "🧪", Type = "Potion", Description = "+15% DEF, -10% ATK for 1 hour.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition GoldRushFlask = new() { Id = "gold_rush_flask", Name = "Gold Rush Flask", Icon = "🧪", Type = "Potion", Description = "+20% gold from dungeons for 2 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition BerserkerBrew = new() { Id = "berserker_brew", Name = "Berserker Brew", Icon = "🧪", Type = "Potion", Description = "+25% ATK, -15% DEF for 2 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition IronbloodTonic = new() { Id = "ironblood_tonic", Name = "Ironblood Tonic", Icon = "🧪", Type = "Potion", Description = "+25% DEF, -15% ATK for 2 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition Antivenom = new() { Id = "antivenom", Name = "Antivenom", Icon = "🧪", Type = "Potion", Description = "-20% dungeon damage taken for the next dungeon run.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition BlacksmithsElixir = new() { Id = "blacksmiths_elixir", Name = "Blacksmith's Elixir", Icon = "⚒️", Type = "Potion", Description = "NPCs buy max items in your shop at the next 12 UTC reset.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition SwiftfootBrew = new() { Id = "swiftfoot_brew", Name = "Swiftfoot Brew", Icon = "🧪", Type = "Potion", Description = "+15% dodge chance for the next dungeon run.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition RaidElixir = new() { Id = "raid_elixir", Name = "Raid Elixir", Icon = "🧪", Type = "Potion", Description = "+15% ATK and +15% DEF for 3 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition BloodPactPotion = new() { Id = "blood_pact_potion", Name = "Blood Pact Potion", Icon = "🧪", Type = "Potion", Description = "+50% ATK, -20% HP for 2 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition GreaterStaminaVial = new() { Id = "greater_stamina_vial", Name = "Greater Stamina Vial", Icon = "🧪", Type = "Potion", Description = "Fully restores all Hunter Stamina.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition RevivalDraught = new() { Id = "revival_draught", Name = "Revival Draught", Icon = "🧪", Type = "Potion", Description = "Survive one killing blow in your next dungeon run.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition GreaterHuntersDraft = new() { Id = "greater_hunters_draft", Name = "Greater Hunter's Draft", Icon = "🧪", Type = "Potion", Description = "+25% loot drop chance for 3 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition GreaterXpSerum = new() { Id = "greater_xp_serum", Name = "Greater XP Serum", Icon = "🧪", Type = "Potion", Description = "+35% XP gain for 3 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition ShadowSalve = new() { Id = "shadow_salve", Name = "Shadow Salve", Icon = "🧪", Type = "Potion", Description = "-30% enemy first strike damage in the next dungeon run.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition VoidTincture = new() { Id = "void_tincture", Name = "Void Tincture", Icon = "🔮", Type = "Potion", Description = "+50% XP gain and +25% loot drop chance for 4 hours.", SubCategory = "Alchemist" };
        public static readonly ItemDefinition DragonBlood = new() { Id = "dragons_blood", Name = "Dragon's Blood", Icon = "🐉", Type = "Potion", Description = "+40% ATK and +40% DEF for 2 hours.", SubCategory = "Alchemist" };
    }
}