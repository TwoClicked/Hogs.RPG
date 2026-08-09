using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities.ColosseumObjects
{
    /// <summary>
    /// One entry in a ColosseumTournament - either a real player who signed
    /// up and paid the buy-in, or a bot generated to fill out the 32-slot
    /// bracket. Tracks where they currently stand in the double-elimination
    /// bracket (which side, still alive or eliminated) and their final
    /// placement once the tournament ends.
    ///
    /// The actual gear/pet/passive/buff loadout lives on the linked
    /// ColosseumBuild, not here - this entity is about bracket state and
    /// identity, not build contents.
    /// </summary>
    public class ColosseumParticipant
    {
        public int Id { get; set; }

        public int ColosseumTournamentId { get; set; }
        public ColosseumTournament ColosseumTournament { get; set; } = null!;

        // =========================
        // IDENTITY
        // =========================
        // 0 for bots (no Discord identity). Always check IsBot rather than
        // relying on DiscordId == 0 alone, to keep intent explicit at call sites.
        public ulong DiscordId { get; set; } = 0;

        public bool IsBot { get; set; } = false;

        // Flavor name shown in bracket displays / match threads for bots,
        // e.g. "Arena Bot #7" or a randomized name. Null for real players -
        // use their Discord display name instead when rendering.
        public string? BotDisplayName { get; set; }

        // =========================
        // BRACKET STATE
        // =========================
        // Which bracket this participant is currently competing in. Starts
        // in WinnerBracket for everyone; moves to LoserBracket after a first
        // loss. GrandFinal/BracketReset are set only for the two finalists
        // once the bracket collapses down to them.
        public ColosseumBracketType CurrentBracket { get; set; } = ColosseumBracketType.WinnerBracket;

        // True once this participant has lost in the LoserBracket (their
        // second loss) or lost the GrandFinal/BracketReset outright. A
        // WinnerBracket loss does NOT set this - it just moves them to
        // LoserBracket instead (see CurrentBracket above).
        public bool Eliminated { get; set; } = false;

        // 1 = tournament winner, 2 = runner-up. Null for everyone else /
        // while the tournament is still in progress. Useful for results
        // announcements and any future standings history.
        public int? FinalPlacement { get; set; }

        // =========================
        // BUILD TRACKING
        // =========================
        // True if this participant (or the no-show randomizer) finalized
        // their build before the registration lock. Every participant ends
        // up true by the time the tournament goes InProgress - this exists
        // mainly to distinguish an active hand-picked lock-in from a
        // still-in-progress build during the registration window.
        public bool BuildLocked { get; set; } = false;

        // True if this build was generated automatically rather than
        // hand-picked - i.e. this is a bot, or a real player who didn't
        // finish/lock their build before the 12h deadline. Surfaced in
        // announcements so it's clear why a player's build looks random.
        public bool BuildWasRandomized { get; set; } = false;

        // =========================
        // NAVIGATION
        // =========================
        public ColosseumBuild? Build { get; set; }
    }
}