namespace Hogs.RPG.Core.GameData.Enhancement
{
    // =========================
    // 🔨 ENHANCEMENT STAT GAINS
    // Two-tier flat gain, per successful level:
    //   +1 to +15  (15 levels) -> +2 ATK / +2 DEF / +10 HP each
    //   PRI to PEN (5 levels)  -> +10 ATK / +10 DEF / +50 HP each
    //
    // GetCumulativeBonus returns the TOTAL bonus for having reached a
    // given level (not just that level's delta) — this is what
    // StatService needs, since it just wants "how much bonus does this
    // slot currently grant" given Player.{Slot}EnhancementLevel.
    // =========================
    public static class EnhancementStatGains
    {
        private const int Tier1Levels = 15; // +1 through +15
        private const int Tier1Attack = 2;
        private const int Tier1Defense = 2;
        private const int Tier1Health = 10;

        private const int Tier2Attack = 10; // PRI through PEN
        private const int Tier2Defense = 10;
        private const int Tier2Health = 50;

        public static (int attack, int defense, int health) GetCumulativeBonus(int level)
        {
            if (level <= 0) return (0, 0, 0);

            int tier1LevelsReached = Math.Min(level, Tier1Levels);
            int tier2LevelsReached = Math.Max(0, level - Tier1Levels);

            int attack = (tier1LevelsReached * Tier1Attack) + (tier2LevelsReached * Tier2Attack);
            int defense = (tier1LevelsReached * Tier1Defense) + (tier2LevelsReached * Tier2Defense);
            int health = (tier1LevelsReached * Tier1Health) + (tier2LevelsReached * Tier2Health);

            return (attack, defense, health);
        }

        // The gain a single successful attempt at this target level grants —
        // used for "what am I about to earn" display text on /enhance,
        // not for the running total (use GetCumulativeBonus for that).
        public static (int attack, int defense, int health) GetGainForLevel(int level)
        {
            if (level <= 0) return (0, 0, 0);

            return level <= Tier1Levels
                ? (Tier1Attack, Tier1Defense, Tier1Health)
                : (Tier2Attack, Tier2Defense, Tier2Health);
        }
    }
}