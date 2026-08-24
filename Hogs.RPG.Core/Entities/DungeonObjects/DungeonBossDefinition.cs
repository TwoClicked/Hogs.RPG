using Hogs.RPG.Core.Entities.PetObjects;
using Hogs.RPG.Core.Enums.PlayerEnums;

namespace Hogs.RPG.Core.Entities.DungeonObjects
{
    public class DungeonBossDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }

        public string AbilitiesText { get; set; }

        // Gear drop system
        public List<BossLoot> LootTable { get; set; } = new();
        public List<DungeonDrop> Drops { get; set; } = new();

        // ✅ Pet drop system (for pet dungeons)
        public List<PetDrop> PetDrops { get; set; } = new();

        // Abilities
        public string BehaviorId { get; set; }

        // =========================
        // 🔨 UPGRADE PIECE DROP (Enhancement system)
        // One slot is randomly picked from this list, then rolled against
        // UpgradePieceDropChancePercent. At most one Upgrade Piece per
        // clear — never independent per-slot rolls.
        // =========================
        public List<EquipmentSlot> UpgradePieceSlots { get; set; } = new();
        public double UpgradePieceDropChancePercent { get; set; } = 0;
    }
}