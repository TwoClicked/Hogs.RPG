using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Data.Repositories;

namespace Hogs.RPG.Services.ColosseumServices
{
    /// <summary>
    /// Builds and advances the double-elimination bracket for one
    /// Colosseum tournament. Bracket is always exactly 32 participants
    /// (real players + bot fill), which means no byes anywhere - every
    /// match always starts with two real participant slots once seeded.
    ///
    /// Bracket shape for 32:
    ///   Winner Bracket: 5 rounds (16 -> 8 -> 4 -> 2 -> 1)
    ///   Loser Bracket: 8 rounds, alternating between absorbing a fresh
    ///     wave of WB dropouts and playing off survivors from the round
    ///     before, converging down to 1
    ///   Grand Final: WB winner vs LB winner
    ///   Bracket Reset: only created if the LB winner wins the Grand Final
    ///
    /// This service only manages the bracket structure and advancement -
    /// actual match resolution is ColosseumCombatService's job, and posting
    /// to Discord threads is the scheduler/bot layer's job.
    /// </summary>
    public class ColosseumBracketService
    {
        private readonly ColosseumRepository _repository;
        private readonly Random _random = new();

        public ColosseumBracketService(ColosseumRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Seeds the entire 32-slot double-elimination bracket structure for
        /// a tournament whose participants are all finalized (real + bots).
        /// Creates every match up front, including empty ones later in the
        /// bracket, and wires up NextMatchOnWinId/NextMatchOnLoseId so
        /// AdvanceAfterMatch can just follow pointers.
        /// </summary>
        public async Task SeedBracketAsync(ColosseumTournament tournament)
        {
            if (tournament.Participants.Count != tournament.BracketSize)
                throw new Exception($"Colosseum: expected {tournament.BracketSize} participants to seed the bracket, got {tournament.Participants.Count}.");

            var shuffled = tournament.Participants.OrderBy(_ => _random.Next()).ToList();

            // ===== Winner Bracket round 1 (16 matches, all 32 seeded in) =====
            var wbRound1 = new List<ColosseumMatch>();
            for (var i = 0; i < shuffled.Count; i += 2)
            {
                var match = await _repository.CreateMatchAsync(new ColosseumMatch
                {
                    ColosseumTournamentId = tournament.Id,
                    BracketType = ColosseumBracketType.WinnerBracket,
                    RoundNumber = 1,
                    ParticipantAId = shuffled[i].Id,
                    ParticipantBId = shuffled[i + 1].Id
                });
                wbRound1.Add(match);
            }

            // ===== Remaining Winner Bracket rounds (8 -> 4 -> 2 -> 1), empty until fed =====
            var previousWbRound = wbRound1;
            var wbRoundsByNumber = new Dictionary<int, List<ColosseumMatch>> { { 1, wbRound1 } };

            for (var roundNum = 2; previousWbRound.Count > 1; roundNum++)
            {
                var nextRound = new List<ColosseumMatch>();
                for (var i = 0; i < previousWbRound.Count; i += 2)
                {
                    var match = await _repository.CreateMatchAsync(new ColosseumMatch
                    {
                        ColosseumTournamentId = tournament.Id,
                        BracketType = ColosseumBracketType.WinnerBracket,
                        RoundNumber = roundNum
                    });
                    nextRound.Add(match);

                    // The two matches feeding this one both send their winner here.
                    previousWbRound[i].NextMatchOnWinId = match.Id;
                    previousWbRound[i + 1].NextMatchOnWinId = match.Id;
                    await _repository.SaveMatchAsync(previousWbRound[i]);
                    await _repository.SaveMatchAsync(previousWbRound[i + 1]);
                }

                wbRoundsByNumber[roundNum] = nextRound;
                previousWbRound = nextRound;
            }

            var wbFinalRound = previousWbRound[0]; // last WB match = WB champion

            // ===== Loser Bracket =====
            // For a 32-bracket, LB has 8 rounds. Odd rounds absorb a fresh
            // wave of WB dropouts against LB survivors; even rounds are
            // pure LB-survivor playoffs that halve the field. This is the
            // standard double-elim LB shape.
            var lbRoundsByNumber = new Dictionary<int, List<ColosseumMatch>>();

            // LB round 1: WB round 1's 16 losers play each other (8 matches).
            var lbRound1 = new List<ColosseumMatch>();
            for (var i = 0; i < wbRound1.Count; i += 2)
            {
                var match = await _repository.CreateMatchAsync(new ColosseumMatch
                {
                    ColosseumTournamentId = tournament.Id,
                    BracketType = ColosseumBracketType.LoserBracket,
                    RoundNumber = 1
                });
                wbRound1[i].NextMatchOnLoseId = match.Id;
                wbRound1[i + 1].NextMatchOnLoseId = match.Id;
                await _repository.SaveMatchAsync(wbRound1[i]);
                await _repository.SaveMatchAsync(wbRound1[i + 1]);
                lbRound1.Add(match);
            }
            lbRoundsByNumber[1] = lbRound1;

            var previousLbRound = lbRound1;
            var wbRoundNumberFeedingLb = 2; // WB round 2's losers feed LB round 2

            for (var lbRoundNum = 2; previousLbRound.Count >= 1 && wbRoundsByNumber.ContainsKey(wbRoundNumberFeedingLb); lbRoundNum++)
            {
                var isDropInRound = lbRoundNum % 2 == 0; // even LB rounds absorb a fresh WB wave

                if (isDropInRound)
                {
                    // Pair each LB survivor with a dropping-down WB round loser.
                    var wbDropInMatches = wbRoundsByNumber[wbRoundNumberFeedingLb];
                    var round = new List<ColosseumMatch>();

                    for (var i = 0; i < previousLbRound.Count; i++)
                    {
                        var match = await _repository.CreateMatchAsync(new ColosseumMatch
                        {
                            ColosseumTournamentId = tournament.Id,
                            BracketType = ColosseumBracketType.LoserBracket,
                            RoundNumber = lbRoundNum
                        });

                        previousLbRound[i].NextMatchOnWinId = match.Id;
                        await _repository.SaveMatchAsync(previousLbRound[i]);

                        wbDropInMatches[i].NextMatchOnLoseId = match.Id;
                        await _repository.SaveMatchAsync(wbDropInMatches[i]);

                        round.Add(match);
                    }

                    lbRoundsByNumber[lbRoundNum] = round;
                    previousLbRound = round;
                    wbRoundNumberFeedingLb++;
                }
                else
                {
                    // Pure survivor playoff - halves the field.
                    var round = new List<ColosseumMatch>();
                    for (var i = 0; i < previousLbRound.Count; i += 2)
                    {
                        var match = await _repository.CreateMatchAsync(new ColosseumMatch
                        {
                            ColosseumTournamentId = tournament.Id,
                            BracketType = ColosseumBracketType.LoserBracket,
                            RoundNumber = lbRoundNum
                        });

                        previousLbRound[i].NextMatchOnWinId = match.Id;
                        previousLbRound[i + 1].NextMatchOnWinId = match.Id;
                        await _repository.SaveMatchAsync(previousLbRound[i]);
                        await _repository.SaveMatchAsync(previousLbRound[i + 1]);

                        round.Add(match);
                    }

                    lbRoundsByNumber[lbRoundNum] = round;
                    previousLbRound = round;
                }
            }

            var lbFinalRound = previousLbRound[0]; // last LB match = LB champion

            // ===== Grand Final: WB champion vs LB champion =====
            var grandFinal = await _repository.CreateMatchAsync(new ColosseumMatch
            {
                ColosseumTournamentId = tournament.Id,
                BracketType = ColosseumBracketType.GrandFinal,
                RoundNumber = 1
            });

            wbFinalRound.NextMatchOnWinId = grandFinal.Id;
            await _repository.SaveMatchAsync(wbFinalRound);

            lbFinalRound.NextMatchOnWinId = grandFinal.Id;
            await _repository.SaveMatchAsync(lbFinalRound);
        }

        /// <summary>
        /// Advances the bracket after one match resolves: fills in the
        /// winner's and loser's next match slots per the match's stored
        /// pointers, and handles the two Grand Final special cases (WB
        /// champion wins outright, vs LB champion forces a Bracket Reset).
        /// Returns the tournament's winner/runner-up participant ids once
        /// the whole bracket is fully decided, otherwise null.
        /// </summary>
        public async Task<(int winnerId, int runnerUpId)?> AdvanceAfterMatchAsync(ColosseumMatch resolvedMatch, int winnerParticipantId, int loserParticipantId)
        {
            resolvedMatch.WinnerParticipantId = winnerParticipantId;
            resolvedMatch.ResolvedAt = DateTime.UtcNow;
            await _repository.SaveMatchAsync(resolvedMatch);

            // Keep the participants' CurrentBracket/Eliminated state in sync.
            var winner = await _repository.GetParticipantAsync(winnerParticipantId);
            var loser = await _repository.GetParticipantAsync(loserParticipantId);

            if (winner == null || loser == null)
                throw new Exception("Colosseum: could not load participants to advance the bracket.");

            if (resolvedMatch.BracketType == ColosseumBracketType.WinnerBracket)
            {
                loser.CurrentBracket = ColosseumBracketType.LoserBracket;
                await _repository.SaveParticipantAsync(loser);
            }
            else if (resolvedMatch.BracketType == ColosseumBracketType.LoserBracket)
            {
                loser.Eliminated = true;
                await _repository.SaveParticipantAsync(loser);
            }
            else if (resolvedMatch.BracketType == ColosseumBracketType.GrandFinal)
            {
                // If the WB champion won, it's over - they never lost.
                // If the LB champion won, this is the WB champion's first
                // loss, so we're not done yet: force a Bracket Reset.
                var wbChampionWon = winner.CurrentBracket == ColosseumBracketType.WinnerBracket;

                if (!wbChampionWon)
                {
                    loser.Eliminated = false; // not eliminated yet - they get the reset match
                    winner.CurrentBracket = ColosseumBracketType.LoserBracket; // tracks "has one loss" going into the reset
                    await _repository.SaveParticipantAsync(loser);
                    await _repository.SaveParticipantAsync(winner);

                    var bracketReset = await _repository.CreateMatchAsync(new ColosseumMatch
                    {
                        ColosseumTournamentId = resolvedMatch.ColosseumTournamentId,
                        BracketType = ColosseumBracketType.BracketReset,
                        RoundNumber = 1,
                        ParticipantAId = winner.Id,
                        ParticipantBId = loser.Id
                    });

                    resolvedMatch.NextMatchOnWinId = bracketReset.Id;
                    await _repository.SaveMatchAsync(resolvedMatch);

                    return null; // tournament isn't over - bracket reset still to come
                }

                loser.Eliminated = true;
                await _repository.SaveParticipantAsync(loser);
                return (winner.Id, loser.Id); // tournament decided, no reset needed
            }
            else if (resolvedMatch.BracketType == ColosseumBracketType.BracketReset)
            {
                loser.Eliminated = true;
                await _repository.SaveParticipantAsync(loser);
                return (winner.Id, loser.Id); // tournament decided
            }

            // Not a finals-type match with a decided outcome yet - push
            // winner (and loser, for WB matches) into their next match slots.
            if (resolvedMatch.NextMatchOnWinId.HasValue)
                await PlaceParticipantAsync(resolvedMatch.NextMatchOnWinId.Value, winnerParticipantId);

            if (resolvedMatch.NextMatchOnLoseId.HasValue)
                await PlaceParticipantAsync(resolvedMatch.NextMatchOnLoseId.Value, loserParticipantId);

            return null;
        }

        // Fills whichever of a match's two participant slots is still empty.
        private async Task PlaceParticipantAsync(int targetMatchId, int participantId)
        {
            var match = await _repository.GetMatchAsync(targetMatchId);
            if (match == null)
                throw new Exception($"Colosseum: target match {targetMatchId} not found while advancing bracket.");

            if (match.ParticipantAId == null)
                match.ParticipantAId = participantId;
            else if (match.ParticipantBId == null)
                match.ParticipantBId = participantId;
            else
                throw new Exception($"Colosseum: match {targetMatchId} already has both participant slots filled.");

            await _repository.SaveMatchAsync(match);
        }

        /// <summary>
        /// Finds every match in a tournament that has both participants
        /// filled in but hasn't been resolved yet - i.e. what's ready to
        /// fight right now. Used by the scheduler each pass to know what to
        /// resolve next.
        /// </summary>
        public async Task<List<ColosseumMatch>> GetReadyMatchesAsync(int tournamentId)
        {
            var matches = await _repository.GetMatchesForTournamentAsync(tournamentId);
            return matches
                .Where(m => m.ParticipantAId.HasValue && m.ParticipantBId.HasValue && m.WinnerParticipantId == null)
                .ToList();
        }
    }
}