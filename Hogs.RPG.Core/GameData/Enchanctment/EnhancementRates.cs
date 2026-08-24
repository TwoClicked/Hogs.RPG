namespace Hogs.RPG.Core.GameData.Enhancement
{
    // =========================
    // 🔨 ENHANCEMENT SUCCESS RATES
    // Indexed by TARGET level (the level you're attempting to reach),
    // matching the same "target level" convention used for Blackstone
    // cost (10 x target level). Index 0 is unused padding so
    // Rates[9] reads naturally as "the rate for reaching +9".
    //
    // +1 to +8 is a guaranteed success on purpose — this is the "free"
    // grind zone. From +9 onward the odds drop fast, and PRI-PEN sit
    // under 1% so Cron Stones and Concentrated Blackstones actually
    // matter at the top end.
    // =========================
    public static class EnhancementRates
    {
        // Index:      0     1      2      3      4      5      6      7      8      9     10     11     12     13     14    15    16(PRI) 17(DUO) 18(TRI) 19(TET) 20(PEN)
        private static readonly double[] BaseSuccessPercent =
        {
            0.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 55.0, 45.0, 35.0, 25.0, 15.0, 8.0, 4.0, 0.9, 0.6, 0.4, 0.25, 0.1
        };

        public const int MaxLevel = 20; // PEN

        /// <summary>
        /// Base success % (before Cron Stones) for attempting to reach the given target level.
        /// </summary>
        public static double GetBaseSuccessPercent(int targetLevel)
        {
            if (targetLevel < 1 || targetLevel > MaxLevel)
                return 0.0;

            return BaseSuccessPercent[targetLevel];
        }
    }
}