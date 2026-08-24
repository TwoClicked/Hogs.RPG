namespace Hogs.RPG.Core.GameData.Enhancement
{
    // =========================
    // 🔨 ENHANCEMENT COSTS
    // =========================
    public static class EnhancementCosts
    {
        // Blackstone cost = 10 x target level.
        // e.g. +8 -> +9 costs 90, TET -> PEN (20) costs 200.
        private const int BlackstonePerLevel = 10;

        public static int GetBlackstoneCost(int targetLevel)
        {
            return targetLevel * BlackstonePerLevel;
        }

        // Cron Stones: each one adds this many percentage points to a
        // single attempt's success chance, capped so no attempt can be
        // boosted more than CronStoneCapPercent total.
        public const double CronStoneBonusPerStone = 0.1;
        public const double CronStoneCapPercent = 25.0;

        // Returns the effective bonus (in percentage points) for a given
        // number of Cron Stones spent on one attempt, already clamped to the cap.
        public static double GetCronStoneBonusPercent(int cronStonesUsed)
        {
            double raw = cronStonesUsed * CronStoneBonusPerStone;
            return raw > CronStoneCapPercent ? CronStoneCapPercent : raw;
        }

        // The number of Cron Stones at which the cap is already reached —
        // useful for the /enhance UI so it can stop letting the player add more.
        public static int MaxUsefulCronStones =>
            (int)(CronStoneCapPercent / CronStoneBonusPerStone); // 250

        // The target level at which a Concentrated Blackstone (slot-specific)
        // is additionally required, on top of the normal Blackstone cost.
        // This is the +15 -> PRI transition.
        public const int ConcentratedBlackstoneGateLevel = 16;

        public static bool RequiresConcentratedBlackstone(int targetLevel)
        {
            return targetLevel == ConcentratedBlackstoneGateLevel;
        }
    }
}