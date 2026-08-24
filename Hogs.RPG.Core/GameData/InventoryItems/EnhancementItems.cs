using Hogs.RPG.Core.Entities.EquipmentObjects;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    // =========================
    // 🔨 ENHANCEMENT MATERIALS
    // Type = "Enhancement" on all of these — deliberately NOT "Material",
    // so they never show up in /inventory (which only has fixed
    // Equipment / Material / Potion tabs) and never get swept into
    // systems that filter on Type == "Material" (e.g. Trail rewards).
    // Their only UI home is /enhance bag.
    //
    // Trading needs no extra work — TradeItemAutocompleteHandler reads
    // straight off inventory regardless of Type, so these are tradeable
    // the moment they're registered below.
    // =========================
    public static class EnhancementItems
    {
        // ===== Currency =====

        public static readonly ItemDefinition Blackstone = new()
        {
            Id = "blackstone",
            Name = "Blackstone",
            Icon = "🪨", // placeholder — swap for real custom emoji once uploaded
            Type = "Enhancement",
            SubCategory = "Currency",
            Description = "Consumed to attempt enhancing Global Boss Gear. Cost scales with target level."
        };

        public static readonly ItemDefinition CronStone = new()
        {
            Id = "cron_stone",
            Name = "Cron Stone",
            Icon = "💠", // placeholder
            Type = "Enhancement",
            SubCategory = "Currency",
            Description = "Adds +0.1% success chance to a single enhancement attempt, capped at +25% per attempt."
        };

        // ===== Components =====

        public static readonly ItemDefinition InfuseCrystal = new()
        {
            Id = "infuse_crystal",
            Name = "Infuse Crystal",
            Icon = "🔮", // placeholder
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Drops from the T6 raid. Combined with an Upgrade Piece to craft a Concentrated Blackstone."
        };

        // ===== Upgrade Pieces (slot-specific, ×9) =====
        // Drop from the new lvl 36/38/40 dungeons. Consumed only on a
        // successful +15 -> PRI enhance attempt (refunded on fail).

        public static readonly ItemDefinition HelmetUpgradePiece = new()
        {
            Id = "upgrade_piece_helmet",
            Name = "Upgrade Piece (Helmet)",
            Icon = "🧩", // placeholder
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "A fragment of overseer craftsmanship. Required to push a Helmet past +15."
        };

        public static readonly ItemDefinition BodyUpgradePiece = new()
        {
            Id = "upgrade_piece_body",
            Name = "Upgrade Piece (Body)",
            Icon = "🧩",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "A fragment of overseer craftsmanship. Required to push a Body piece past +15."
        };

        public static readonly ItemDefinition LegsUpgradePiece = new()
        {
            Id = "upgrade_piece_legs",
            Name = "Upgrade Piece (Legs)",
            Icon = "🧩",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "A fragment of overseer craftsmanship. Required to push Legs past +15."
        };

        public static readonly ItemDefinition GlovesUpgradePiece = new()
        {
            Id = "upgrade_piece_gloves",
            Name = "Upgrade Piece (Gloves)",
            Icon = "🧩",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "A fragment of overseer craftsmanship. Required to push Gloves past +15."
        };

        public static readonly ItemDefinition BootsUpgradePiece = new()
        {
            Id = "upgrade_piece_boots",
            Name = "Upgrade Piece (Boots)",
            Icon = "🧩",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "A fragment of overseer craftsmanship. Required to push Boots past +15."
        };

        public static readonly ItemDefinition AmuletUpgradePiece = new()
        {
            Id = "upgrade_piece_amulet",
            Name = "Upgrade Piece (Amulet)",
            Icon = "🧩",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "A fragment of overseer craftsmanship. Required to push an Amulet past +15."
        };

        public static readonly ItemDefinition RingUpgradePiece = new()
        {
            Id = "upgrade_piece_ring",
            Name = "Upgrade Piece (Ring)",
            Icon = "🧩",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "A fragment of overseer craftsmanship. Required to push a Ring past +15."
        };

        public static readonly ItemDefinition MainHandUpgradePiece = new()
        {
            Id = "upgrade_piece_mainhand",
            Name = "Upgrade Piece (Main Hand)",
            Icon = "🧩",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "A fragment of overseer craftsmanship. Required to push a Main Hand weapon past +15."
        };

        public static readonly ItemDefinition OffHandUpgradePiece = new()
        {
            Id = "upgrade_piece_offhand",
            Name = "Upgrade Piece (Off Hand)",
            Icon = "🧩",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "A fragment of overseer craftsmanship. Required to push an Off Hand item past +15."
        };

        // ===== Concentrated Blackstones (slot-specific, ×9) =====
        // Crafted from Upgrade Piece + Infuse Crystal via /enhance craft.
        // Required (in addition to normal Blackstone cost) to attempt +15 -> PRI.
        // On fail: this is consumed, but the Upgrade Piece is refunded.

        public static readonly ItemDefinition HelmetConcentratedBlackstone = new()
        {
            Id = "concentrated_blackstone_helmet",
            Name = "Concentrated Blackstone (Helmet)",
            Icon = "⬛", // placeholder
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Required to attempt enhancing a Helmet from +15 to PRI."
        };

        public static readonly ItemDefinition BodyConcentratedBlackstone = new()
        {
            Id = "concentrated_blackstone_body",
            Name = "Concentrated Blackstone (Body)",
            Icon = "⬛",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Required to attempt enhancing a Body piece from +15 to PRI."
        };

        public static readonly ItemDefinition LegsConcentratedBlackstone = new()
        {
            Id = "concentrated_blackstone_legs",
            Name = "Concentrated Blackstone (Legs)",
            Icon = "⬛",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Required to attempt enhancing Legs from +15 to PRI."
        };

        public static readonly ItemDefinition GlovesConcentratedBlackstone = new()
        {
            Id = "concentrated_blackstone_gloves",
            Name = "Concentrated Blackstone (Gloves)",
            Icon = "⬛",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Required to attempt enhancing Gloves from +15 to PRI."
        };

        public static readonly ItemDefinition BootsConcentratedBlackstone = new()
        {
            Id = "concentrated_blackstone_boots",
            Name = "Concentrated Blackstone (Boots)",
            Icon = "⬛",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Required to attempt enhancing Boots from +15 to PRI."
        };

        public static readonly ItemDefinition AmuletConcentratedBlackstone = new()
        {
            Id = "concentrated_blackstone_amulet",
            Name = "Concentrated Blackstone (Amulet)",
            Icon = "⬛",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Required to attempt enhancing an Amulet from +15 to PRI."
        };

        public static readonly ItemDefinition RingConcentratedBlackstone = new()
        {
            Id = "concentrated_blackstone_ring",
            Name = "Concentrated Blackstone (Ring)",
            Icon = "⬛",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Required to attempt enhancing a Ring from +15 to PRI."
        };

        public static readonly ItemDefinition MainHandConcentratedBlackstone = new()
        {
            Id = "concentrated_blackstone_mainhand",
            Name = "Concentrated Blackstone (Main Hand)",
            Icon = "⬛",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Required to attempt enhancing a Main Hand weapon from +15 to PRI."
        };

        public static readonly ItemDefinition OffHandConcentratedBlackstone = new()
        {
            Id = "concentrated_blackstone_offhand",
            Name = "Concentrated Blackstone (Off Hand)",
            Icon = "⬛",
            Type = "Enhancement",
            SubCategory = "Component",
            Description = "Required to attempt enhancing an Off Hand item from +15 to PRI."
        };
    }
}