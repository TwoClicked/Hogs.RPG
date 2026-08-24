using Hogs.RPG.Core.Enums.PlayerEnums;

namespace Hogs.RPG.Core.Entities.EnhancementObjects
{
    // A dry-run look at what an attempt would cost/require, with nothing
    // spent yet. The /enhance command uses this to render a confirm screen
    // before calling AttemptEnhanceAsync.
    public class EnhancePreview
    {
        public EquipmentSlot Slot { get; set; }
        public int CurrentLevel { get; set; }
        public int TargetLevel { get; set; }
        public bool IsMaxLevel { get; set; }

        public int BlackstoneCost { get; set; }
        public int BlackstonesOwned { get; set; }

        public bool RequiresConcentratedBlackstone { get; set; }
        public bool HasConcentratedBlackstone { get; set; }

        public int CronStonesToUse { get; set; } // already clamped to owned + the 25% cap
        public int CronStonesOwned { get; set; }

        public double BaseSuccessPercent { get; set; }
        public double BonusSuccessPercent { get; set; }
        public double EffectiveSuccessPercent { get; set; }

        public bool CanAttempt { get; set; }
        public string? BlockedReason { get; set; }
    }
}