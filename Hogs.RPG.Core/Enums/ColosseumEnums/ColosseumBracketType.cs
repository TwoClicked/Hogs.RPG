namespace Hogs.RPG.Core.Enums
{
    /// <summary>
    /// Identifies which part of the double-elimination bracket a given
    /// ColosseumMatch belongs to. A participant starts with two lives:
    /// their first loss drops them from the Winner Bracket into the
    /// Loser Bracket, and a second loss (a Loser Bracket loss) eliminates
    /// them from the tournament entirely.
    /// </summary>
    public enum ColosseumBracketType
    {
        // Standard bracket matches. Losing here doesn't eliminate you -
        // it drops you into the Loser Bracket instead.
        WinnerBracket,

        // The "second life" bracket. Everyone here has already lost once.
        // Losing a Loser Bracket match eliminates the participant.
        LoserBracket,

        // The winner of the Winner Bracket faces the winner of the Loser
        // Bracket. Because the Winner Bracket finalist has zero losses,
        // if they lose this match it's only their first loss - so the
        // tournament isn't over yet (see BracketReset below).
        GrandFinal,

        // Only created if the Loser Bracket winner beats the Winner Bracket
        // finalist in the GrandFinal. Both participants now have exactly
        // one loss each, so this single decider match settles the tournament.
        BracketReset
    }
}