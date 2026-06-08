namespace Hogs.RPG.Core.Entities.DungeonObjects
{
    public class ActiveDungeon
    {
        public ulong PlayerId { get; set; }

        public string DungeonId { get; set; }

        public int Floor { get; set; }

        public int Attack { get; set; }
        public int Defense { get; set; }

        public int PlayerHealth { get; set; }
        public int MaxHealth { get; set; }

        public int EnemyHealth { get; set; }
        public int EnemyMaxHealth { get; set; }

        public string CurrentImageUrl { get; set; }

        public bool IsBoss { get; set; }

        public bool IsActive { get; set; } = true;

        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }

        // =========================
        // EXISTING BOSS ABILITIES
        // =========================
        public bool RageTriggered { get; set; }
        public bool CloudActive { get; set; }

        // =========================
        // NEW BOSS ABILITIES
        // =========================

        // Intoxicate (Rot Father Malchor — Lv 5)
        // Triggers at 50% HP, poisons the player for 3 turns
        public bool IntoxicateTriggered { get; set; }
        public int PoisonTurnsRemaining { get; set; }

        // Chain Snare (Aurelion the Oathbreaker — Lv 17)
        // Set to true in AttackAsync when snare triggers; read in HandleChainSnare
        public bool ChainSnaredThisTurn { get; set; }

        // Star Iron Madness (Gritch — Lv 27)
        // Triggers at 50% HP, permanently doubles boss defense (applied in GetEnemyDefense)
        public bool StarIronMadnessTriggered { get; set; }

        // =========================
        // 🧪 ALCHEMIST POTION BUFFS
        // Populated at dungeon start from player's active utility buff
        // =========================
        public double DmgReductionPercent { get; set; } = 0;       // Antivenom
        public double DodgeChanceBonus { get; set; } = 0;           // Swiftfoot Brew
        public double FirstStrikeReductionPercent { get; set; } = 0; // Shadow Salve
        public bool HasRevival { get; set; } = false;               // Revival Draught
        public double GoldBoostPercent { get; set; } = 0;           // Gold Rush Flask
        public bool FirstStrikeUsed { get; set; } = false;          // Track if first strike reduction consumed
    }
}
