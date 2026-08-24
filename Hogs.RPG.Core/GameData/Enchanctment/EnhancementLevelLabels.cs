namespace Hogs.RPG.Core.GameData.Enhancement
{
    // =========================
    // 🔨 ENHANCEMENT LEVEL LABELS
    // Single source of truth for turning a 0-20 level into display text.
    // Used by /enhance, /enhance bag, /profile, and feed announcements —
    // centralized here so those don't each maintain their own switch
    // statement that could drift out of sync.
    // =========================
    public static class EnhancementLevelLabels
    {
        private static readonly string[] TierNames =
        {
            "PRI", "DUO", "TRI", "TET", "PEN"
        };

        public static string GetLabel(int level)
        {
            if (level <= 0) return "";
            if (level <= 15) return $"+{level}";

            int tierIndex = level - 16; // 16 -> PRI (index 0) ... 20 -> PEN (index 4)
            return tierIndex >= 0 && tierIndex < TierNames.Length
                ? TierNames[tierIndex]
                : "";
        }

        // True for PRI/DUO/TRI/TET/PEN — useful later for flashier
        // feed announcements when a player crosses into prestige tiers.
        public static bool IsPrestigeTier(int level)
        {
            return level >= 16 && level <= EnhancementRates.MaxLevel;
        }
    }
}