namespace Hogs.RPG.Core.Enums
{
    /// <summary>
    /// Lifecycle states for a single Colosseum tournament (ColosseumTournament).
    /// One tournament moves through these states in order, left to right,
    /// with the exception of Cancelled which can be reached from Registration
    /// or Locked if something goes wrong before the bracket starts.
    /// </summary>
    public enum ColosseumTournamentStatus
    {
        // Signup is open. Real players are buying in and building their loadouts
        // in DM. Bots have not been generated yet - we wait until the window
        // closes so we know exactly how many bot slots we need to fill.
        Registration,

        // Registration window has closed. Builds are frozen (locked-in builds stay
        // as-is, anyone who didn't lock gets a randomized build), bot participants
        // are generated to fill the remaining slots up to 32, and the bracket is
        // being seeded. This is a short transitional state before InProgress.
        Locked,

        // The double-elimination bracket is actively resolving matches.
        InProgress,

        // A winner has been crowned, prizes have been paid out, and results
        // have been posted. Terminal state.
        Completed,

        // Something stopped the tournament before it could finish normally
        // (e.g. not enough real signups and an admin cancels, or a manual abort).
        // Any collected buy-ins should be refunded when this state is reached.
        Cancelled
    }
}