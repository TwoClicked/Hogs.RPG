using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.GameData.InventoryItems;

namespace Hogs.RPG.Core.GameData.Enhancement
{
    // =========================
    // 🔨 ENHANCEMENT SLOT MAP
    // The one place that ties together, per EquipmentSlot:
    //   - the Global Boss Gear item ID that slot can enhance
    //   - the Player column holding that slot's enhancement level
    //   - the Upgrade Piece / Concentrated Blackstone item IDs for that slot
    //
    // EnhancementService, StatService (Phase 5), and the /enhance commands
    // (Phase 6) all read through this instead of each maintaining their
    // own switch — keeps the 9-slot mapping in exactly one place.
    // =========================
    public static class EnhancementSlotMap
    {
        public static string GetGlobalBossItemId(EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.MainHand => GlobalBossGear.AureliusSword.Id,
            EquipmentSlot.OffHand => GlobalBossGear.GravelmawShield.Id,
            EquipmentSlot.Helmet => GlobalBossGear.TyrHelm.Id,
            EquipmentSlot.Body => GlobalBossGear.XerathulArmor.Id,
            EquipmentSlot.Legs => GlobalBossGear.ThrolakLeggings.Id,
            EquipmentSlot.Gloves => GlobalBossGear.SerpentGloves.Id,
            EquipmentSlot.Boots => GlobalBossGear.SirRachaBoots.Id,
            EquipmentSlot.Ring => GlobalBossGear.PunisherRing.Id,
            EquipmentSlot.Amulet => GlobalBossGear.GullveigAmulet.Id,
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };

        public static int GetEnhancementLevel(Player player, EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.MainHand => player.MainHandEnhancementLevel,
            EquipmentSlot.OffHand => player.OffHandEnhancementLevel,
            EquipmentSlot.Helmet => player.HelmetEnhancementLevel,
            EquipmentSlot.Body => player.BodyEnhancementLevel,
            EquipmentSlot.Legs => player.LegsEnhancementLevel,
            EquipmentSlot.Gloves => player.GlovesEnhancementLevel,
            EquipmentSlot.Boots => player.BootsEnhancementLevel,
            EquipmentSlot.Ring => player.RingEnhancementLevel,
            EquipmentSlot.Amulet => player.AmuletEnhancementLevel,
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };

        public static void SetEnhancementLevel(Player player, EquipmentSlot slot, int newLevel)
        {
            switch (slot)
            {
                case EquipmentSlot.MainHand: player.MainHandEnhancementLevel = newLevel; break;
                case EquipmentSlot.OffHand: player.OffHandEnhancementLevel = newLevel; break;
                case EquipmentSlot.Helmet: player.HelmetEnhancementLevel = newLevel; break;
                case EquipmentSlot.Body: player.BodyEnhancementLevel = newLevel; break;
                case EquipmentSlot.Legs: player.LegsEnhancementLevel = newLevel; break;
                case EquipmentSlot.Gloves: player.GlovesEnhancementLevel = newLevel; break;
                case EquipmentSlot.Boots: player.BootsEnhancementLevel = newLevel; break;
                case EquipmentSlot.Ring: player.RingEnhancementLevel = newLevel; break;
                case EquipmentSlot.Amulet: player.AmuletEnhancementLevel = newLevel; break;
                default: throw new ArgumentOutOfRangeException(nameof(slot));
            }
        }

        public static string GetUpgradePieceItemId(EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.MainHand => EnhancementItems.MainHandUpgradePiece.Id,
            EquipmentSlot.OffHand => EnhancementItems.OffHandUpgradePiece.Id,
            EquipmentSlot.Helmet => EnhancementItems.HelmetUpgradePiece.Id,
            EquipmentSlot.Body => EnhancementItems.BodyUpgradePiece.Id,
            EquipmentSlot.Legs => EnhancementItems.LegsUpgradePiece.Id,
            EquipmentSlot.Gloves => EnhancementItems.GlovesUpgradePiece.Id,
            EquipmentSlot.Boots => EnhancementItems.BootsUpgradePiece.Id,
            EquipmentSlot.Ring => EnhancementItems.RingUpgradePiece.Id,
            EquipmentSlot.Amulet => EnhancementItems.AmuletUpgradePiece.Id,
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };

        public static string GetConcentratedBlackstoneItemId(EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.MainHand => EnhancementItems.MainHandConcentratedBlackstone.Id,
            EquipmentSlot.OffHand => EnhancementItems.OffHandConcentratedBlackstone.Id,
            EquipmentSlot.Helmet => EnhancementItems.HelmetConcentratedBlackstone.Id,
            EquipmentSlot.Body => EnhancementItems.BodyConcentratedBlackstone.Id,
            EquipmentSlot.Legs => EnhancementItems.LegsConcentratedBlackstone.Id,
            EquipmentSlot.Gloves => EnhancementItems.GlovesConcentratedBlackstone.Id,
            EquipmentSlot.Boots => EnhancementItems.BootsConcentratedBlackstone.Id,
            EquipmentSlot.Ring => EnhancementItems.RingConcentratedBlackstone.Id,
            EquipmentSlot.Amulet => EnhancementItems.AmuletConcentratedBlackstone.Id,
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };
    }
}