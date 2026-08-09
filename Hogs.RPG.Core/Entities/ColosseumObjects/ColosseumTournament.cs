using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities.ColosseumObjects
{
    /// <summary>
    /// One Colosseum tournament run (one per day). Tracks the overall
    /// lifecycle, prize pool, and holds the participants + matches that
    /// belong to it.
    ///
    /// Buy-in / prize economics: each real player pays BuyInGold to enter.
    /// Prizes are fixed (WinnerPrizeGold / RunnerUpPrizeGold) regardless of
    /// how many bots are in the field - the house absorbs any shortfall if
    /// the buy-in pool doesn't cover both prizes. Bots never receive gold
    /// even if they place 1st or 2nd.
    /// </summary>
    public class ColosseumTournament
    {
        public int Id { get; set; }

        public ColosseumTournamentStatus Status { get; set; } = ColosseumTournamentStatus.Registration;

        // =========================
        // TIMING
        // =========================
        public DateTime RegistrationOpenedAt { get; set; } = DateTime.UtcNow;

        // Registration closes 12 hours after opening. Builds lock at this point -
        // anyone who hasn't hit "lock in" gets a randomized build instead
        // (same generator used for bot participants).
        public DateTime RegistrationEndsAt { get; set; }

        // Set when the scheduler transitions Registration -> Locked and starts
        // filling bot slots / seeding the bracket.
        public DateTime? LockedAt { get; set; }

        // Set when a winner has been crowned and prizes paid out.
        public DateTime? CompletedAt { get; set; }

        // =========================
        // ECONOMY
        // =========================
        public int BuyInGold { get; set; } = 1000;

        public int WinnerPrizeGold { get; set; } = 10000;

        public int RunnerUpPrizeGold { get; set; } = 5000;

        // Arena Points given to every participant (real or bot) to spend on
        // their build. Stored on the tournament (not hardcoded in the build
        // service) so we can tune it per-run without a code change.
        public int BuildBudgetAP { get; set; } = 1000;

        // =========================
        // BRACKET SHAPE
        // =========================
        // Always 32. Real signups (capped at 20) fill first, bots pad out
        // the rest so the bracket is always a clean, fully-seeded size.
        public int BracketSize { get; set; } = 32;

        public int MaxRealPlayers { get; set; } = 20;

        // =========================
        // DISCORD CONTEXT
        // =========================
        // Channel where the "signups open" announcement and final results
        // are posted. Individual matches get their own threads (tracked on
        // ColosseumMatch), not posted directly here.
        public ulong AnnounceChannelId { get; set; }

        // =========================
        // RESULTS
        // =========================
        // Populated once the tournament completes. Null while InProgress.
        public int? WinnerParticipantId { get; set; }

        public int? RunnerUpParticipantId { get; set; }

        // =========================
        // NAVIGATION
        // =========================
        public List<ColosseumParticipant> Participants { get; set; } = new();

        public List<ColosseumMatch> Matches { get; set; } = new();
    }
}