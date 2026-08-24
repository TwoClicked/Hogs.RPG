namespace Hogs.RPG.Core.Entities.EnhancementObjects
{
    public class EnhanceAttemptResult
    {
        // False if the attempt couldn't even be made (missing materials, already max, etc.)
        // Check this before RollSucceeded.
        public bool Success { get; set; }
        public string? FailureReason { get; set; }

        // The actual %-chance roll outcome — only meaningful when Success == true.
        public bool RollSucceeded { get; set; }

        public int PreviousLevel { get; set; }
        public int NewLevel { get; set; } // same as PreviousLevel if the roll failed

        public int BlackstonesSpent { get; set; }
        public int CronStonesSpent { get; set; }
        public bool ConcentratedBlackstoneConsumed { get; set; }
        public bool UpgradePieceRefunded { get; set; } // true if a PRI attempt failed and the piece came back

        public double EffectiveSuccessPercent { get; set; }
    }
}