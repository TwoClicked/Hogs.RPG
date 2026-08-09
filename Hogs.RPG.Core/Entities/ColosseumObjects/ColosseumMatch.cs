using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities.ColosseumObjects
{
    /// <summary>
    /// One match in the double-elimination bracket. Because the bracket is
    /// always a fixed 32 slots (bots pad out any empty real-player seats),
    /// there are never byes to account for - every match always has two
    /// real participants (real player or bot) once the bracket is seeded.
    ///
    /// Bracket advancement is modeled with two forward pointers rather than
    /// a generic tree structure: NextMatchOnWinId tells ColosseumBracketService
    /// where the winner goes, and NextMatchOnLoseId tells it where the loser
    /// goes (only meaningful for WinnerBracket matches - a LoserBracket loss
    /// is elimination, so that match's NextMatchOnLoseId stays null). When
    /// advancing a participant into a target match, the service fills
    /// whichever of that match's two participant slots is still empty.
    /// </summary>
    public class ColosseumMatch
    {
        public int Id { get; set; }

        public int ColosseumTournamentId { get; set; }
        public ColosseumTournament ColosseumTournament { get; set; } = null!;

        // =========================
        // BRACKET POSITION
        // =========================
        public ColosseumBracketType BracketType { get; set; }

        // Round number within this BracketType (e.g. WinnerBracket round 1,
        // 2, 3... independent of LoserBracket's own round numbering).
        public int RoundNumber { get; set; }

        // =========================
        // PARTICIPANTS
        // =========================
        // Null until a participant has actually been placed into this slot -
        // matches later in the bracket start empty and get filled in as
        // earlier matches resolve and advance their winners/losers forward.
        public int? ParticipantAId { get; set; }
        public ColosseumParticipant? ParticipantA { get; set; }

        public int? ParticipantBId { get; set; }
        public ColosseumParticipant? ParticipantB { get; set; }

        public int? WinnerParticipantId { get; set; }

        // =========================
        // BRACKET ADVANCEMENT
        // =========================
        // Where the winner of this match gets placed next. Null only for the
        // very last match of the tournament (GrandFinal if it wasn't reset,
        // or BracketReset if it was).
        public int? NextMatchOnWinId { get; set; }

        // Where the loser of this match gets placed next. Only set for
        // WinnerBracket matches (drops into LoserBracket) and for the
        // GrandFinal (see BracketReset in ColosseumBracketType). Null for
        // LoserBracket matches, since losing there eliminates the participant.
        public int? NextMatchOnLoseId { get; set; }

        // =========================
        // DISCORD + COMBAT LOG
        // =========================
        // Discord thread this match's combat log gets posted to. Created by
        // ColosseumBracketService just before the match resolves.
        public ulong ThreadId { get; set; } = 0;

        // Full round-by-round auto-resolved fight text, produced by
        // ColosseumCombatService. Posted to ThreadId once resolved.
        public string? CombatLog { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }
    }
}